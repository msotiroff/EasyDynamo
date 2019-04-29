### How to create a dynamo table by code first aproach

You can run a simple code (using the facade method EnsureCreatedAsync) on building your application to ensure all the tables for your models are created:

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
