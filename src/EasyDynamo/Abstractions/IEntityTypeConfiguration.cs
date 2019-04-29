namespace EasyDynamo.Abstractions
{
    public interface IEntityTypeConfiguration<TEntity> where TEntity : class
    {
        void Configure(IEntityTypeBuilder<TEntity> builder);
    }
}