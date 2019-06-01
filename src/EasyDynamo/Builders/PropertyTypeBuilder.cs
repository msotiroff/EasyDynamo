using EasyDynamo.Abstractions;
using EasyDynamo.Config;

namespace EasyDynamo.Builders
{
    public class PropertyTypeBuilder<TEntity> : IPropertyTypeBuilder
    {
        private readonly PropertyConfiguration<TEntity> configuration;

        protected internal PropertyTypeBuilder(PropertyConfiguration<TEntity> configuration)
        {
            this.configuration = configuration;
        }
        
        /// <summary>
        /// Adds a specific value as default. If a value is missing the specified default 
        /// value will be set before saving the entity to the database.
        /// </summary>
        public IPropertyTypeBuilder HasDefaultValue(object defaultValue)
        {
            this.configuration.DefaultValue = defaultValue;

            return this;
        }

        /// <summary>
        /// Specifies either the member is required or not.
        /// </summary>
        public IPropertyTypeBuilder IsRequired(bool required = true)
        {
            this.configuration.IsRequired = required;

            return this;
        }
    }
}