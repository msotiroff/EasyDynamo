## How to configure the database access

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

   public DynamoDbSet<User> Users { get; set; }

   public DynamoDbSet<Article> Articles { get; set; }
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