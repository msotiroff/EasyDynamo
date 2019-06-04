using Amazon.DynamoDBv2.DataModel;
using EasyDynamo.Abstractions;
using EasyDynamo.Config;
using EasyDynamo.Exceptions;
using EasyDynamo.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EasyDynamo.Tools.Validators
{
    public class EntityValidator<TEntity> : IEntityValidator<TEntity> 
        where TEntity : class, new()
    {
        private readonly EntityConfiguration<TEntity> entityConfiguration;

        public EntityValidator()
        {
            this.entityConfiguration = EntityConfiguration<TEntity>.Instance;
        }

        public IEnumerable<ValidationResult> GetValidationResults(TEntity entity)
        {
            var validationContext = new ValidationContext(entity);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(
                entity, validationContext, validationResults, true);

            return validationResults;
        }

        public void Validate(TEntity entity)
        {
            this.ValidatePrimaryKey(entity);

            this.ValidateByConfiguration(entity);

            this.ValidateByAttributes(entity);
        }

        private void ValidateByConfiguration(TEntity entity)
        {
            var errors = new List<string>();

            foreach (var memberConfig in this.entityConfiguration.Properties)
            {
                if (!memberConfig.IsRequired)
                {
                    continue;
                }

                if (entityConfiguration.IgnoredMembersNames.Contains(memberConfig.MemberName))
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

        private void ValidateByAttributes(TEntity entity)
        {
            var errors = this.GetValidationResults(entity)
                .Select(vr => vr.ErrorMessage);

            if (errors.Any())
            {
                throw new EntityValidationFailedException(errors.JoinByNewLine());
            }
        }

        private void ValidatePrimaryKey(TEntity entity)
        {
            var hashKeyType = this.entityConfiguration.HashKeyMemberType;
            var hashKeyTypeDefaultValue = hashKeyType?.GetDefaultValue();
            var hashKeyFunction = this.entityConfiguration.HashKeyMemberExpression.Compile();
            var hashKeyMember = entity
                .GetType()
                .GetProperties()
                .FirstOrDefault(pi => pi
                    .GetCustomAttributes(false)
                        .Any(attr => attr.GetType() == typeof(DynamoDBHashKeyAttribute)));
            var hashKeyExist = 
                (!hashKeyFunction(entity)?.Equals(hashKeyTypeDefaultValue) ?? false) || 
                hashKeyMember?.GetValue(entity) != null;

            if (hashKeyExist)
            {
                return;
            }

            throw new EntityValidationFailedException($"Hash key {hashKeyMember?.Name} not set.");
        }
    }
}
