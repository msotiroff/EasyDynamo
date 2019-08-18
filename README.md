<h1>
	<img src="https://github.com/msotiroff/EasyDynamo/blob/master/logo.jpg" align="left" alt="EasyDynamo">EasyDynamo - fluent configuring and access for DynamoDB with code first aproach.
</h1>

------------

EasyDynamo is a small library that helps developers to access and configure DynamoDB easier. Different configurations can be applied for different environments (development, staging, production) as well as using a local dynamo instance for non production environment. Supports creating dynamo tables correspondings to your models using code first aproach.

### Installation:
You can install this library using NuGet into your project.

`Install-Package EasyDynamo`

### How to configure the database access:
#### 1. Make your own database context class and inherit from EasyDynamo.Core.DynamoContext as below:

```csharp
public class BlogDbContext : DynamoContext
{
    public BlogDbContext(IServiceProvider serviceProvider) 
        : base(serviceProvider) { }
}
```
The IServiceProvider will be resolved by the MVC or you can pass your own if the application is not an ASP.NET app.

#### 2. Make your model classes:

```csharp
public class User
{
    public string Id { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime DateRegistered { get; set; }

    public DateTime LastActivity { get; set; }
}
```
```csharp
public class Article
{
    public string Id { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public DateTime CreatedOn { get; set; }

    public string AuthorId { get; set; }
}
```

#### 3. Add all your models to your database context class as DynamoDbSets:

```csharp
public class BlogDbContext : DynamoContext
{
    public BlogDbContext(IServiceProvider serviceProvider)
      : base(serviceProvider) { }

   public IDynamoDbSet<User> Users { get; set; }

   public IDynamoDbSet<Article> Articles { get; set; }
}
```

#### 4. Add your database context class to the service resolver in Startup.cs:

```csharp
public void ConfigureServices(IServiceCollection services)
{
   services.AddDynamoContext<BlogDbContext>(this.Configuration);

   services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
}
```

#### 5. Add whatever options you want to the dynamo configuration, you can hardcode them or you can use the application configuration files or environment variables:

```csharp
services.AddDynamoContext<BlogDbContext>(this.Configuration, options => 
   {
      options.Profile = "MyAmazonProfile";
      options.RegionEndpoint = RegionEndpoint.USEast1;
      options.AccessKeyId = Environment.GetEnvironmentVariable("AccessKey");
      options.SecretAccessKey = this.Configuration.GetValue<string>("Credentials:SecretKey");
	  options.Conversion = DynamoDBEntryConversion.V2;
   });
```

##### 5.1. Another way to add configuration and keep startup.cs thin:
Just override OnConfiguring method of your database context class and specify your options there. Of course you can hardcode them or you can use the application configuration files or environment variables.

```csharp
public class BlogDbContext : DynamoContext
{
    public BlogDbContext(IServiceProvider serviceProvider)
        : base(serviceProvider) { }

    public DynamoDbSet<User> Users { get; set; }

    public DynamoDbSet<Article> Articles { get; set; }

    protected override void OnConfiguring(
        DynamoContextOptionsBuilder builder, IConfiguration configuration)
    {
        builder.UseAccessKeyId(Environment.GetEnvironmentVariable("AccessKey"));
        builder.UseSecretAccessKey(configuration.GetValue<string>("AWS:Credentials:SecretKey"));
        builder.UseRegionEndpoint(RegionEndpoint.USEast1);
		builder.UseEntryConversionV2();

        base.OnConfiguring(builder, configuration);
    }
}
```

You are ready to use your database context class wherever you want:

```csharp
public class HomeController : Controller
{
    private readonly BlogDbContext context;

    public HomeController(BlogDbContext context)
    {
        this.context = context;
    }

    public async Task<IActionResult> Index()
    {
        var articles = await this.context.Articles.GetAsync();

        return View(articles);
    }
}
```

### How to configure your models:

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

#### How to configure a local instance of DynamoDB.
You can run dynamo locally using [Docker Image](https://hub.docker.com/r/amazon/dynamodb-local/ "Docker Image") or [DynamoDBLocal.jar file](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.DownloadingAndRunning.html "DynamoDBLocal.jar file").

##### How to configure local dynamo for a specific environment? 
Important: if a local mode is enabled you should specify the ServiceUrl with the port you use for the local instance of DynamoDB!
##### 1. In Startup.cs:
```csharp
services.AddDynamoContext<BlogDbContext>(this.Configuration, options => 
{
    options.LocalMode = this.Configuration
        .GetValue<bool>("DynamoOptions:LocalMode");
    options.ServiceUrl = this.Configuration
        .GetValue<string>("DynamoOptions:ServiceUrl");
});
```
appsettings.json:
```json
{
  "DynamoOptions": {
    "LocalMode": false
}
```
appsettings.Development.json:
```json
{
  "DynamoOptions": {
    "LocalMode": true,
    "ServiceUrl": "http://localhost:8000"
}
```
This code will run local client when environment is Development and cloud mode when it's production.

#### 2. In your database context class:
```csharp
protected override void OnConfiguring(
    DynamoContextOptionsBuilder builder, IConfiguration configuration)
{
    var shouldUseLocalInstance = configuration.GetValue<bool>("DynamoOptions:LocalMode");

    if (shouldUseLocalInstance)
    {
        var serviceUrl = configuration.GetValue<string>("DynamoOptions:ServiceUrl");

        builder.UseLocalMode(serviceUrl);
    }

    base.OnConfiguring(builder, configuration);
}
```
### How to create a dynamo table by code first aproach.
You can run a simple code (using the facade method EnsureCreatedAsync) on building your application to ensure all the tables for your models are created before your application has started:

#### 1. Directly get the context instance from the application services in Startup.cs (not recommended, because the resourses can be disposed before tables have been created).
```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseMvc();

    var context = app.ApplicationServices.GetRequiredService<BlogDbContext>();

    context.Database.EnsureCreatedAsync().Wait();
}
```
#### 2. The right way => extension over IApplicationBuilder and using resourses in a separate scope.
Add an extention method in a separate class:
```csharp
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder EnsureDatabaseCreated(this IApplicationBuilder app)
    {
        var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

            Task.Run(async () =>
            {
                await context.Database.EnsureCreatedAsync();
            })
            .Wait();
        }

        return app;
    }
}
```
Then call this method in Startup.cs:
```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseMvc();

    app.EnsureDatabaseCreated();
}
```

### How to work with your database context class.
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
        return await this.context.Articles.GetAsync();
    }
}
```
##### 1.2. Get an item by primary key:
```csharp
public async Task<Article> GetArticleAsync(string id)
{
    return await this.context.Articles.GetAsync(id);
}
```
##### 1.3. Get an item by primary key and range key:
```csharp
public async Task<Article> GetArticleAsync(string primaryKey, DateTime rangeKey)
{
    return await this.context.Articles.GetAsync(primaryKey, rangeKey);
}
```
##### 1.4. Get a paginated set of items:
You should cache the pagination token from the response and pass it to the next call in order to retrieve the next set of items.
```csharp
public async Task<IEnumerable<Article>> GetNextPageAsync(int itemsPerPage, string paginationToken)
{
    var response = await this.context.Articles.GetAsync(itemsPerPage, paginationToken);

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

### How to extend and use you own DynamoDbSets.
You may extend the default implementation of IDynamoDbSet like that:
#### 1. Create your own implementation:
```
public class ExtendedDynamoDbSet<TEntity> : DynamoDbSet<TEntity>, IExtendedDynamoDbSet 
    where TEntity : class, new()
{
    public ExtendedDynamoDbSet(
        IAmazonDynamoDB client,
        IDynamoDBContext dbContext,
        IIndexExtractor indexExtractor,
        ITableNameExtractor tableNameExtractor,
        IPrimaryKeyExtractor primaryKeyExtractor,
        IEntityValidator<TEntity> validator)
        : base(client,
              dbContext,
              indexExtractor,
              tableNameExtractor,
              primaryKeyExtractor,
              validator)
    {
    }

    public async Task<ListBackupsResponse> GetBackupsAsync()
    {
        var request = new ListBackupsRequest
        {
            BackupType = BackupTypeFilter.ALL,
            TableName = base.TableName
        };

        return await base.Client.ListBackupsAsync(request);
    }
}
```
#### 2. Add the specific custom sets to the DI container.
```
public void ConfigureServices(IServiceCollection services)
{
    services.AddDynamoContext<BlogDbContext>(this.Configuration);

    services.AddTransient<IDynamoDbSet<User>, ExtendedDynamoDbSet<User>>();

    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
}
```