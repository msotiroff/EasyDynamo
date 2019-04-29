using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDynamo.Abstractions
{
    public interface IEntityValidator<TEntity> where TEntity : class, new()
    {
        IEnumerable<ValidationResult> GetValidationResults(TEntity entity);

        void Validate(TEntity entity);

        void ValidateByAttributes(TEntity entity);

        void ValidateByConfiguration(TEntity entity);
    }
}