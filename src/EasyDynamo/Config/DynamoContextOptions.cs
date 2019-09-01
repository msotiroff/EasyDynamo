using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using EasyDynamo.Abstractions;
using EasyDynamo.Attributes;
using EasyDynamo.Core;
using EasyDynamo.Exceptions;
using EasyDynamo.Tools.Validators;
using System;
using System.Collections.Generic;

namespace EasyDynamo.Config
{
    [IgnoreAutoResolving]
    public class DynamoContextOptions : IDynamoContextOptions
    {
        protected internal DynamoContextOptions(Type contextType)
        {
            this.EnsureValidContextType(contextType);

            this.ContextType = contextType;
            this.TableNameByEntityTypes = new Dictionary<Type, string>();
            this.AwsOptions = new AWSOptions();
        }

        public Type ContextType { get; }

        public IDictionary<Type, string> TableNameByEntityTypes { get; }

        public string AccessKeyId { get; set; }
        
        public string SecretAccessKey { get; set; }
        
        public bool LocalMode { get; set; }
        
        public string ServiceUrl { get; set; }
        
        public RegionEndpoint RegionEndpoint { get; set; }

        public string Profile { get; set; }

        public DynamoDBEntryConversion Conversion { get; set; }

        public AWSOptions AwsOptions { get; set; }

        public void ValidateLocalMode()
        {
            if (!this.LocalMode)
            {
                return;
            }

            var requiredValuesProvided = !string.IsNullOrWhiteSpace(this.ServiceUrl) &&
                !string.IsNullOrWhiteSpace(this.AccessKeyId) &&
                !string.IsNullOrWhiteSpace(this.SecretAccessKey);

            if (!requiredValuesProvided)
            {
                throw new DynamoContextConfigurationException(
                    $"When local mode is enabled the following values must be provided: " +
                    $"{nameof(this.ServiceUrl)}, " +
                    $"{nameof(this.AccessKeyId)}, " +
                    $"{nameof(this.SecretAccessKey)}.");
            }
        }

        public void ValidateCloudMode()
        {
            if (this.LocalMode)
            {
                return;
            }

            var requiredValuesProvided = this.RegionEndpoint != null &&
                !string.IsNullOrWhiteSpace(this.Profile);

            if (!requiredValuesProvided)
            {
                throw new DynamoContextConfigurationException(
                    $"When cloud mode is enabled the following values must be provided: " +
                    $"{nameof(this.RegionEndpoint)}, " +
                    $"{nameof(this.Profile)}.");
            }
        }

        public void UseTableName<TEntity>(string tableName) where TEntity : class, new()
        {
            InputValidator.ThrowIfNullOrWhitespace(
                tableName, $"Parameter name cannot be empty: {nameof(tableName)}.");

            this.TableNameByEntityTypes[typeof(TEntity)] = tableName;
        }

        private void EnsureValidContextType(Type contextType)
        {
            if (typeof(DynamoContext).IsAssignableFrom(contextType))
            {
                return;
            }

            throw new DynamoContextConfigurationException(
                $"{contextType.FullName} does not inherit from {nameof(DynamoContext)}.");
        }
    }
}
