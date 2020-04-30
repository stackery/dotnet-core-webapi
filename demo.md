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
1. Create a new stack in stackery (dotnet-core-webapp-#)
1. Open up a second VS instance and clone repo
1. Switch to folder view in solution explorer
1. Open template.yaml to show the empty template
1. Open powershell and descend into cloned sources (source/stackery/dotnet-core-webapp-#)
1. Run `stackery edit`
1. Add an HTTP API resource
    1. Update sole route to be: ANY $default and save
    1. Update route settings to proxy to existing service URL from the beanstalk app
1. Switch back to VS and show how template.yaml was updated
1. Commit changes and push in Team Explorer
1. Open a second powershell instance
1. Deploy stack: `stackery deploy`
