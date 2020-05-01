### Prep
1. Open following tabs in browser:
    1. https://app.stackery.io logged in as demo@stackery.io
1. Open VS with dotnet-framework-webapi solution open
1. Authenticate to AWS management console
1. Set up `demo` profile for AWS
1. Set system env vars:
    * AWS_PROFILE=demo
    * STACKERY_ENV_NAME=development

### Demo
1. Show VS with framework solution
    1. Open App_Start/WebApiConfig.cs and explain how it's an api where we can hit routes like /api/<controller>
    1. Open Controllers/BooksController.cs and Controllers/AuthorsController.cs and explain how it maps to a Books record in our DB
    1. Explain this is a Windows .NET Framework solution running in AWS Elastic Beanstalk
    1. Open AWS Explorer and find Beanstalk App
    1. Open app URL in browser at /api/books and /api/authors
    1. Note how responses come back in XML because that's the default for .NET Framework
1. Explain our goal: Modernize the books routes because they are high throughput and not scaling well, but leave the authors route because we don't want to mess with it right now

#### Create a new stack
1. Create a new stack in stackery (dotnet-core-webapp-#)
1. Open up a second VS instance and clone repo
1. Switch to folder view in solution explorer
1. Add .vs to .gitignore, save and close
1. Open template.yaml to show the empty template

#### Create a new proxy API
1. Open powershell and descend into cloned sources (source/stackery/dotnet-core-webapp-#)
1. Run `stackery edit`
1. Add an HTTP API resource
    1. Update sole route to be: ANY $default and save
    1. Update route settings to proxy to existing service URL from the beanstalk app
1. Switch back to VS and show how template.yaml was updated
1. Commit changes and push in Team Explorer
1. Open a second powershell instance
1. Deploy stack: `stackery deploy` and explain what's happening
1. Copy HTTP url endpoint at the end and open it in the browser, adding /api/books and /api/authors to show off how we're proxying the routes

### Create a new ASP.NET Core Web API
1. In VS, File -> New Project
    1. Choose ASP.NET Core Web Application
    1. Project Name: webapi (to keep the namespace consistent with the existing app)
    1. Put project in src
    1. Check the box to put solution and project in the same directory
    1. Choose API type and uncheck Configure for HTTPS (as API Gateway will handle that for us)
    1. Add .gitignore to src\webapi and add `bin`, `obj`, and `packages` to it
    1. Add .stackery-config.yaml and add `function-id: WebApi` and `template-path: ../../template.yaml`
    1. Open webapi.csproj
    1. Debug project and navigate to /weatherforecast
    
### Let's get the new API running serverlessly
1. In stackery editor, add a new route GET /weatherforecast
1. Add a new function
    * Logical ID: WebApi
    * Runtime: dotnetcore3.1
    * Source Directory: src/webapi
    * Handler: `webapi::webapi.LambdaEntryPoint::FunctionHandlerAsync` (we'll see this created in a new class in a minute)
1. Connect new route to function
1. In VS, add a new class `LambdaEntryPoint.cs` to the solution (explain this is where the code to start our function will live)
1. Open https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.AspNetCoreServer to show documentation for the AspNetCoreServer package
1. Right click on the webapi project in the solution explorer and click `Manage NuGet Packages`
1. Search and install `Amazon.Lambda.AspNetCoreServer`
1. Copy the code sample and paste it into the new LambdaEntry.cs file
1. Update the namespace to `webapi`
1. Update the `Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction` code to `Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction`
1. Push to master:
    1. Save All
    1. Stage src and template.yaml changes in Team Explorer
    1. Commit
    1. Sync
1. Run `stackery-deploy` in powershell
1. Go to deployed api in browser and navigate to /weatherforecast to see the new route

### Now let's extend it to reimplement the Books controller
1. In stackery editor:
    1. Add API routes `/api/books` and `/api/books/{proxy+}` and wire them to function
    1. Add database resource and do use existing with DB address and port from AWS Explorer
    1. Connect function to DB
    1. Open environments in a new tab, explain
    1. Go into development and note the dbPassword secret and how it's in a namespace
    1. Go back to editor and add secrets resource and connect function to it
    1. Open function, note the permission that was added and the env var for the namespace
1. Add webapi-framework existing project to webapi solution (just so we can copy files over easily)
1. Copy Models folder over
1. Copy Data folder over
1. Update Data\webapiContext.cs
    1. Remove unnecessary usings
    1. Find and install Microsoft.EntityFrameworkCore
    1. Add private members:
        ```c#
        private Task<GetSecretValueResponse> secretResponseTask;
        ```
    1. Remove constructor base call
    1. Add this line to the constructor: `AmazonSecretsManagerClient client = new AmazonSecretsManagerClient();`
    1. Right click on class name and generate overrides
    1. Add the following code to the constructor:
        ```c#
        secretResponseTask = client.GetSecretValueAsync(new Amazon.SecretsManager.Model.GetSecretValueRequest
        {
            SecretId = $"{Environment.GetEnvironmentVariable("SECRETS_NAMESPACE")}dbPassword"
        });
        ```
    1. Deselect all methods and select OnConfiguring()
    1. Delete `base.OnConfiguring(optionsBuilder);`
    1. Add the following code:
        ```c#
        GetSecretValueResponse response = secretResponseTask.Result;
            
        String dbPassword = response.SecretString;
        String dbAddress = Environment.GetEnvironmentVariable("DB_ADDRESS");
        String dbPort = Environment.GetEnvironmentVariable("DB_PORT");
        ```
    1. Now we need to connect to a SQL Server: Add NuGet package Microsoft.EntityFrameworkCore.SqlServer
    1. Add: `optionsBuilder.UseSqlServer($"Data Source={dbAddress},{dbPort};Initial Catalog=books;User ID=root;Password={dbPassword}");`
1. Copy Controllers/BooksController.cs over
    1. Open https://docs.microsoft.com/en-us/aspnet/core/migration/webapi?view=aspnetcore-3.1#migrate-models-and-controllers to show there are docs for how to port .NET Framework controllers to .NET Core
    1. Remove unnecessary usings
    1. Decorate class with
        ```c#
        [Route("api/[controller]")]
        [ApiController]
        ```
    1. Change ApiController base class to Controller
    1. Decorate each method with something like `[HttpGet]` or `[HttpGet("{id}")]`
    1. Change IQueryable to IEnumerable
    1. Change IActionResult to ActionResult<Type>
    1. Change things like `StatusCode(HttpStatusCode.NoContent)` to `NoContent()
1. Remove webapi-framework solution
1. Commit and push changes
1. Run `stackery deploy` again
1. Test out /api/books and compare against /api/authors
