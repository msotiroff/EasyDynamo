### How to configure your database models

#### 1. You can use standart DynamoDB attributes in your model class like that:

```csharp
[DynamoDBTable("blog_articles_production")]
public class Article
{
    [DynamoDBHashKey]
    public string Id { get; set; }

    [DynamoDBGlobalSecondaryIndexHashKey("gsi_articles_title")]
    public string Title { get; set; }

    public string Content { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("gsi_articles_title")]
    public DateTime CreatedOn { get; set; }

    public string AuthorId { get; set; }
}
```
... but then you cannot use different tables for different environments!

#### 2. Better way is you override OnModelCreating method in your database context class:

```csharp
protected override void OnModelCreating(ModelBuilder builder, IConfiguration configuration)
{
    builder.Entity<Article>(entity =>
    {
        entity.HasTable(configuration.GetValue<string>("DynamoOptions:ArticlesTableName"));
        entity.HasPrimaryKey(a => a.Id);
        entity.HasGlobalSecondaryIndex(index =>
        {
            index.IndexName = configuration
                .GetValue<string>("DynamoOptions:Indexes:ArticleTitleGSI");
            index.HashKeyMemberName = nameof(Article.Title);
            index.RangeKeyMemberName = nameof(Article.CreatedOn);
            index.ReadCapacityUnits = 3;
            index.WriteCapacityUnits = 3;
        });
        entity.Property(a => a.CreatedOn).HasDefaultValue(DateTime.UtcNow);
        entity.Property(a => a.Content).IsRequired();
    });

    base.OnModelCreating(builder, configuration);
}
```
appsettings.json:
```json
{
  "DynamoOptions": {
    "ArticlesTableName": "blog_articles_production",
    "Indexes": {
      "ArticleTitleGSI": "gsi_articles_title"
    }
  }
}
```
appsettings.Development.json:
```json
{
  "DynamoOptions": {
    "ArticlesTableName": "blog_articles_development",
    "Indexes": {
      "ArticleTitleGSI": "gsi_articles_title"
    }
  }
}
```
That way different table names will be applied for production and development environments.
