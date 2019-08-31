using EasyDynamo.Core;

namespace EasyDynamo.Abstractions
{
    public interface IEntityTypeConfiguration<TContext, TEntity>
        where TContext : DynamoContext
        where TEntity : class
    {
        void Configure(IEntityTypeBuilder<TContext, TEntity> builder);
    }
}