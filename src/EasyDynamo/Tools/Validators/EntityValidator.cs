using Amazon.DynamoDBv2.DataModel;
using EasyDynamo.Abstractions;
using EasyDynamo.Exceptions;
using EasyDynamo.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EasyDynamo.Tools.Validators
{
    public class EntityValidator : IEntityValidator
    {
        private readonly IEntityConfigurationProvider entityConfigurationProvider;

        public EntityValidator(IEntityConfigurationProvider entityConfigurationProvider)
        {
            this.entityConfigurationProvider = entityConfigurationProvider;
        }

        public IEnumerable<ValidationResult> GetValidationResults<TEntity>(Type contextType, TEntity entity)
            where TEntity : class
        {
            var validationContext = new ValidationContext(entity);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(
                entity, validationContext, validationResults, true);

            return validationResults;
        }

        public void Validate<TEntity>(Type contextType, TEntity entity) where TEntity : class
        {
            this.ValidatePrimaryKey(contextType, entity);

            this.ValidateByConfiguration(contextType, entity);

            this.ValidateByAttributes(contextType, entity);
        }

        private void ValidateByConfiguration<TEntity>(Type contextType, TEntity entity)
            where TEntity : class
        {
            var errors = new List<string>();
            var entityConfig = this.entityConfigurationProvider.GetEntityConfiguration(
                contextType, typeof(TEntity))  as IEntityConfiguration<TEntity>;

            foreach (var memberConfig in entityConfig.Properties)
            {
                if (!memberConfig.IsRequired)
                {
                    continue;
                }

                if (entityConfig.IgnoredMembersNames.Contains(memberConfig.MemberName))
                {
                    continue;
                }

                var propertyInfo = typeof(TEntity)
                    .GetProperty(memberConfig.MemberName);
                var propertyValue = propertyInfo.GetValue(entity);

                if (propertyValue != null)
                {
                    continue;
                }

                errors.Add($"{memberConfig.MemberName} is required.");
            }

            if (errors.Count == 0)
            {
                return;
            }

            throw new EntityValidationFailedException(errors.JoinByNewLine());
        }

        private void ValidateByAttributes<TEntity>(Type contextType, TEntity entity)
            where TEntity : class
        {
            var errors = this.GetValidationResults<TEntity>(contextType, entity)
                .Select(vr => vr.ErrorMessage);

            if (errors.Any())
            {
                throw new EntityValidationFailedException(errors.JoinByNewLine());
            }
        }

        private void ValidatePrimaryKey<TEntity>(Type contextType, TEntity entity)
            where TEntity : class
        {
            var entityConfig = this.entityConfigurationProvider.GetEntityConfiguration(
                contextType, typeof(TEntity)) as IEntityConfiguration<TEntity>;
            var hashKeyType = entityConfig?.HashKeyMemberType;
            var hashKeyTypeDefaultValue = hashKeyType?.GetDefaultValue();
            var hashKeyFunction = entityConfig?.HashKeyMemberExpression?.Compile();
            var hashKeyMember = entity
                .GetType()
                .GetProperties()
                .FirstOrDefault(pi => pi
                    .GetCustomAttributes(false)
                        .Any(attr => attr.GetType() == typeof(DynamoDBHashKeyAttribute)));
            var hashKeyExist = 
                (!hashKeyFunction?.Invoke(entity)?.Equals(hashKeyTypeDefaultValue) ?? false) || 
                hashKeyMember?.GetValue(entity) != null;

            if (hashKeyExist)
            {
                return;
            }

            throw new EntityValidationFailedException($"Hash key {hashKeyMember?.Name} not set.");
        }
    }
}
