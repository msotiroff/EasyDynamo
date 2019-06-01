using Amazon;
using EasyDynamo.Abstractions;
using EasyDynamo.Exceptions;
using EasyDynamo.Tools.Validators;
using System;
using System.Collections.Generic;

namespace EasyDynamo.Config
{
    public class DynamoContextOptions : IDynamoContextOptions
    {
        private static volatile DynamoContextOptions instance;
        private static readonly object instanceLocker = new object();

        protected DynamoContextOptions()
        {
            this.TableNameByEntityTypes = new Dictionary<Type, string>();
        }

        internal static DynamoContextOptions Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLocker)
                    {
                        if (instance == null)
                        {
                            instance = new DynamoContextOptions();
                        }
                    }
                }

                return instance;
            }
        }

        protected internal IDictionary<Type, string> TableNameByEntityTypes { get; }

        public string AccessKeyId { get; set; }
        
        public string SecretAccessKey { get; set; }
        
        public bool LocalMode { get; set; }
        
        public string ServiceUrl { get; set; }
        
        public RegionEndpoint RegionEndpoint { get; set; }

        public string Profile { get; set; }

        protected internal void ValidateLocalMode()
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

        protected internal void ValidateCloudMode()
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
    }
}
