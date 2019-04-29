### How to configure a local instance of DynamoDB
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