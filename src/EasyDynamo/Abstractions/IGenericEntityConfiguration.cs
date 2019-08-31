using EasyDynamo.Config;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EasyDynamo.Abstractions
{
    public interface IEntityConfiguration<TEntity> : IEntityConfiguration
        where TEntity : class
    {
        Expression<Func<TEntity, object>> HashKeyMemberExpression { get; }

        ICollection<PropertyConfiguration<TEntity>> Properties { get; }
    }
}