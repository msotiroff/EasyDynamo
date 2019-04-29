using Amazon;
using EasyDynamo.Config;
using EasyDynamo.Exceptions;

namespace EasyDynamo.Builders
{
    public class DynamoContextOptionsBuilder
    {
        private static volatile DynamoContextOptionsBuilder instance;
        private static readonly object instanceLoker = new object();

        private readonly DynamoContextOptions options;

        private DynamoContextOptionsBuilder()
        {
            this.options = DynamoContextOptions.Instance;
        }

        internal static DynamoContextOptionsBuilder Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLoker)
                    {
                        if (instance == null)
                        {
                            instance = new DynamoContextOptionsBuilder();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Adds a specific name for the table corresponding to the given entity.
        /// </summary>
        public DynamoContextOptionsBuilder UseTableName<TEntity>(string tableName) 
            where TEntity : class, new()
        {
            this.options.UseTableName<TEntity>(tableName);

            return this;
        }

        /// <summary>
        /// Adds a specific access key to the dynamo client's credentials
        /// </summary>
        /// <exception cref="DynamoContextConfigurationException"></exception>
        public DynamoContextOptionsBuilder UseAccessKeyId(string accessKey)
        {
            if (string.IsNullOrWhiteSpace(accessKey))
            {
                throw new DynamoContextConfigurationException(
                    $"Parameter cannot be empty: {nameof(accessKey)}.");
            }

            this.options.AccessKeyId = accessKey;

            return this;
        }

        /// <summary>
        /// Adds a specific access secret to the dynamo client's credentials
        /// </summary>
        /// <exception cref="DynamoContextConfigurationException"></exception>
        public DynamoContextOptionsBuilder UseSecretAccessKey(string accessSecret)
        {
            if (string.IsNullOrWhiteSpace(accessSecret))
            {
                throw new DynamoContextConfigurationException(
                    $"Parameter cannot be empty: {nameof(accessSecret)}.");
            }

            this.options.SecretAccessKey = accessSecret;

            return this;
        }

        /// <summary>
        /// Use a local instance of a dynamoDb on a given service url. For example: "http://localhost:8013".
        /// </summary>
        /// <param name="serviceUrl">Required parameter.</param>
        /// <exception cref="DynamoContextConfigurationException"></exception>
        public DynamoContextOptionsBuilder UseLocalMode(string serviceUrl)
        {
            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new DynamoContextConfigurationException(
                    $"{nameof(serviceUrl)} must be provided.");
            }

            options.ServiceUrl = serviceUrl ?? options.ServiceUrl;

            return this;
        }

        /// <summary>
        /// Adds a specific service url to the configuration. For example: "http://localhost:8013".
        /// </summary>
        /// <param name="serviceUrl">Required parameter.</param>
        /// <exception cref="DynamoContextConfigurationException"></exception>
        public DynamoContextOptionsBuilder UseServiceUrl(string serviceUrl)
        {
            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new DynamoContextConfigurationException(
                    $"{nameof(serviceUrl)} must be provided.");
            }

            this.options.ServiceUrl = serviceUrl;

            return this;
        }

        /// <summary>
        /// Adds a specific region to the configuration.
        /// </summary>
        /// <param name="region">Required parameter.</param>
        /// <exception cref="DynamoContextConfigurationException"></exception>
        public DynamoContextOptionsBuilder UseRegionEndpoint(RegionEndpoint region)
        {
            this.options.RegionEndpoint = region 
                ?? throw new DynamoContextConfigurationException(
                    $"{nameof(RegionEndpoint)} must be provided.");

            return this;
        }
    }
}
