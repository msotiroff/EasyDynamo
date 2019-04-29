###  How to work with your database context class

Every DynamoDbSet in your database context class is a wrapper around Amazon.DynamoDBv2.DataModel.IDynamoDBContext class. Every DynamoDbSet has basic methods for CRUD operations as well as for filter/scan operations. You can use them as well as the Base property that gives you an access to the Amazon.DynamoDBv2.DataModel.IDynamoDBContext implementation.

#### 1. Get operations:
##### 1.1. Get all items from a table using GetAsync() method:
```csharp
public class ArticleService : IArticleService
{
    private readonly BlogDbContext context;

    public ArticleService(BlogDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Article>> GetArticlesAsync()
    {
        return await context.Articles.GetAsync();
    }
}
```
##### 1.2. Get an item by primary key:
```csharp
public async Task<Article> GetArticleAsync(string id)
{
    return await context.Articles.GetAsync(id);
}
```
##### 1.3. Get an item by primary key and range key:
```csharp
public async Task<Article> GetArticleAsync(string primaryKey, DateTime rangeKey)
{
    return await context.Articles.GetAsync(primaryKey, rangeKey);
}
```
##### 1.4. Get a paginated set of items:
You should cache the pagination token from the response and pass it to the next call in order to retrieve the next set of items.
```csharp
public async Task<IEnumerable<Article>> GetNextPageAsync(int itemsPerPage, string paginationToken)
{
    var response = await context.Articles.GetAsync(itemsPerPage, paginationToken);

    return response.NextResultSet;
}
```
#### 2. Create operations:
##### 2.1. Add a new item in a table using AddAsync() method:
If an entity with the same primary key already exist an exception will be thrown!
```csharp
public async Task CreateAsync(Article article)
{
    await this.context.Articles.AddAsync(article);
}
```
##### 2.2. Add or update an item using SaveAsync() method:
If an entity with the same primary key already exist it will be updated, otherwise will be created.
```csharp
public async Task AddOrUpdateAsync(Article article)
{
    await this.context.Articles.SaveAsync(article);
}
```
##### 2.3. Add/update multiple items using SaveManyAsync() method:
If an entity with the same primary key already exist it will be updated, otherwise will be created.
```csharp
public async Task AddOrUpdateManyAsync(IEnumerable<Article> articles)
{
    await this.context.Articles.SaveManyAsync(articles);
}
```
#### 3. Update operations
##### 3.1. Update an item using UpdateAsync() method:
If an entity with the same primary key does not exist an exception will be thrown!
```csharp
public async Task UpdateAsync(Article article)
{
    await this.context.Articles.UpdateAsync(article);
}
```
#### 4. Delete operations
##### 4.1. Delete an item using RemoveAsync(item) method:
```csharp
public async Task DeleteAsync(Article article)
{
    await this.context.Articles.RemoveAsync(article);
}
```
##### 4.2. Delete an item by primary key using RemoveAsync(id) method:
```csharp
public async Task DeleteAsync(string primaryKey)
{
    await this.context.Articles.RemoveAsync(primaryKey);
}
```
#### 5. Filter/Query operations
##### 5.1. Filter items by predicate expression using FilterAsync(expression) method:
Warning: Can be a very slow operation when using over a big table.
```csharp
public async Task<IEnumerable<Article>> GetLatestByTitleTermAsync(string searchTerm)
{
    return await this.context.Articles.FilterAsync(
        a => a.Title.Contains(searchTerm) && a.CreatedOn > DateTime.UtcNow.AddYears(-1));
}
```
##### 5.2. Filter items by member match:
If there is an index with hash key the given property,  the query operation will be made against that index.
```csharp
public async Task<IEnumerable<Article>> GetAllByTitleMatchAsync(string title)
{
    return await this.context.Articles.FilterAsync(
        a => a.Title, ScanOperator.Contains, title);
}
```
##### 5.3. Filter items by member name and value:
Query operation against an index. If index is not passed, the first index with hash key the given property found will be used.
```csharp
public async Task<IEnumerable<Article>> FilterByTitle(string title)
{
    return await this.context.Articles.FilterAsync(
        nameof(Article.Title), title, "gsi_articles_title");
}
```
#### 6. Make other operations using the Base property
You have an access to the wrapped Amazon.DynamoDBv2.DataModel.IDynamoDBContext via Base property in each DynamoDbSet you declared in the database context class.
Examples:
```csharp
public string GetTableName()
{
    var tableInfo = this.context.Articles.Base.GetTargetTable<Article>();

    return tableInfo.TableName;
}
```
```csharp
public async Task<IEnumerable<Article>> GetAllAsync()
{
    var batchGet = this.context.Articles.Base.CreateBatchGet<Article>();

    await batchGet.ExecuteAsync();

    return batchGet.Results;
}
```