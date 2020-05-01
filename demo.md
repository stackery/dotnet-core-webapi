### Prep
1. Open following tabs in browser:
    1. https://app.stackery.io logged in as demo@stackery.io
1. Open VS with dotnet-framework-webapi solution open
1. Authenticate to AWS management console

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
    1. Add .gitignore to src\webapi and add `bin` and `obj` to it
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
