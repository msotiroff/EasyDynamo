﻿using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime.CredentialManagement;
using EasyDynamo.Abstractions;
using EasyDynamo.Attributes;
using EasyDynamo.Builders;
using EasyDynamo.Config;
using EasyDynamo.Core;
using EasyDynamo.Exceptions;
using EasyDynamo.Tools;
using EasyDynamo.Tools.Resolvers;
using EasyDynamo.Tools.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyDynamo.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static HashSet<Type> contextTypesAdded = new HashSet<Type>();

        /// <summary>
        /// Adds your database context class to the service provider.
        /// Use to configure the DynamoContext options.
        /// </summary>
        /// <typeparam name="TContext">A custom type that implements EasyDynamo.Core.DynamoContext</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="optionsExpression">Action over EasyDynamo.Config.DynamoContextOptions.</param>
        public static IServiceCollection AddDynamoContext<TContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<DynamoContextOptions> optionsExpression)
            where TContext : DynamoContext
        {
            InputValidator.ThrowIfNull(optionsExpression);

            var contextOptions = new DynamoContextOptions(typeof(TContext));

            optionsExpression(contextOptions);

            services.AddSingleton<IDynamoContextOptions>(sp => contextOptions);

            services.AddDynamoContext<TContext>(configuration);
            
            return services;
        }

        /// <summary>
        /// Adds your database context class to the service provider.
        /// </summary>
        /// <typeparam name="TContext">
        /// A custom type that implements EasyDynamo.Core.DynamoContext.
        /// </typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        public static IServiceCollection AddDynamoContext<TContext>(
            this IServiceCollection services, IConfiguration configuration) 
            where TContext : DynamoContext
        {
            services.EnsureContextNotAdded<TContext>();

            services.AddSingleton<IDependencyResolver, ServiceProviderDependencyResolver>();
            services.AddCoreServices();

            var awsOptions = configuration.GetAWSOptions();
            var contextOptions = services
                .BuildServiceProvider()
                .GetRequiredService<IDynamoContextOptionsProvider>()
                .TryGetContextOptions<TContext>();

            if (contextOptions == null)
            {
                contextOptions = new DynamoContextOptions(typeof(TContext));

                services.AddSingleton(sp => contextOptions);
            }

            services.AddDefaultAWSOptions(awsOptions);

            var contextInstance = Instantiator.GetConstructorlessInstance<TContext>();

            services.BuildModels(contextInstance, configuration, contextOptions);

            BuildConfiguration(contextInstance, configuration, contextOptions);

            services.AddSingleton<TContext>();
            services.AddSingleton(typeof(IDynamoDbSet<>), typeof(DynamoDbSet<>));

            services.AddDynamoClient(awsOptions, contextOptions);

            contextTypesAdded.Add(typeof(TContext));
            
            return services;
        }
        
        private static IServiceCollection AddDynamoClient(
            this IServiceCollection services, 
            AWSOptions awsOptions, 
            IDynamoContextOptions options)
        {
            var awsCredentials = awsOptions?.Credentials?.GetCredentials();

            options.Profile = options.Profile ?? awsOptions?.Profile;
            options.AccessKeyId = options.AccessKeyId ?? awsCredentials?.AccessKey;
            options.SecretAccessKey = options.SecretAccessKey ?? awsCredentials?.SecretKey;

            if (options.LocalMode)
            {
                AddDynamoLocalClient(services, options);

                return services; ;
            }

            options.RegionEndpoint = options.RegionEndpoint ?? awsOptions?.Region;

            AddDynamoCloudClient(services, options, awsOptions);

            return services;
        }

        private static void AddDynamoCloudClient(
            IServiceCollection services, 
            IDynamoContextOptions contextOptions,
            AWSOptions awsOptions)
        {
            awsOptions.Profile = awsOptions?.Profile ?? contextOptions.Profile;
            awsOptions.Region = awsOptions?.Region ?? contextOptions.RegionEndpoint;

            contextOptions.AwsOptions = awsOptions;

            //contextOptions.ValidateCloudMode();

            services.AddAWSService<IAmazonDynamoDB>(awsOptions);
        }

        private static void AddDynamoLocalClient(
            IServiceCollection services,
            IDynamoContextOptions options)
        {
            options.AwsOptions.Credentials = AWSCredentialsFactory.GetAWSCredentials(
                new CredentialProfile(options.Profile, new CredentialProfileOptions
                {
                    AccessKey = options.AccessKeyId,
                    SecretKey = options.SecretAccessKey
                }),
                new CredentialProfileStoreChain());

            options.AwsOptions.DefaultClientConfig.ServiceURL = options.ServiceUrl;

            //options.ValidateLocalMode();

            services.AddAWSService<IAmazonDynamoDB>(options.AwsOptions);
        }

        private static void BuildConfiguration<TContext>(
            TContext contextInstance, 
            IConfiguration configuration,
            IDynamoContextOptions contextOptions) 
            where TContext : DynamoContext
        {
            var dynamoOptionsBuilder = new DynamoContextOptionsBuilder(contextOptions);
            var configuringMethod = typeof(TContext)
                .GetMethod("OnConfiguring", BindingFlags.Instance | BindingFlags.NonPublic);

            configuringMethod.Invoke(
                contextInstance, new object[] { dynamoOptionsBuilder, configuration });
        }

        private static void BuildModels<TContext>(
            this IServiceCollection services,
            TContext contextInstance, 
            IConfiguration configuration, 
            IDynamoContextOptions contextOptions) 
            where TContext : DynamoContext
        {
            var modelBuilder = new ModelBuilder(contextOptions);
            var modelCreatingMethod = typeof(TContext)
                .GetMethod("OnModelCreating", BindingFlags.Instance | BindingFlags.NonPublic);

            modelCreatingMethod.Invoke(contextInstance, new object[] { modelBuilder, configuration });

            modelBuilder.EntityConfigurations
                .Values
                .ToList()
                .ForEach(config => services.AddSingleton(config));
        }

        private static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            typeof(ServiceCollectionExtensions)
                .Assembly
                .GetTypes()
                .Where(t => t.IsClass &&
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    t.GetCustomAttribute<IgnoreAutoResolvingAttribute>() == null &&
                    t.GetInterfaces()
                        .Any(i => i.Name == $"I{t.Name}"))
                .Select(t => new
                {
                    Interface = t.GetInterface($"I{t.Name}"),
                    Implementation = t
                })
                .ToList()
                .ForEach(s => services.AddTransient(s.Interface, s.Implementation));

            return services;
        }

        private static IServiceCollection EnsureContextNotAdded<TContext>(
            this IServiceCollection services)
        {
            var isAlreadyAddedInServices = services
                .Any(s => s.ImplementationType == typeof(TContext));

            if (contextTypesAdded.Contains(typeof(TContext)) || isAlreadyAddedInServices)
            {
                throw new DynamoContextConfigurationException(
                    $"You can invoke AddDynamoContext<{typeof(TContext).FullName}> only once.");
            }

            return services;
        }
    }
}
