using EasyDynamo.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDynamo.Abstractions
{
    public interface IEntityValidator
    {
        IEnumerable<ValidationResult> GetValidationResults<TEntity>(
            Type contextType, TEntity entity) where TEntity : class;

        void Validate<TEntity>(Type contextType, TEntity entity) where TEntity : class;
    }
}