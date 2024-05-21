# JF_SWEngineeringAssessment_20240517
This Application processes data from Azure using the Graph API. 

## Help
You can always use the help feature in the application, as in the following examples:

General help:
```
GraphProcessor.exe help
GraphProcessor 1.0.0+19e59a88f38e644ca0e145a06eb730371fd9df2b
Copyright (C) 2024 GraphProcessor

  groups     Download AAD Groups into a specified folder

  help       Display more information on a specific command.

  version    Display version information.
```

Help on a specific command:
```
GraphProcessor.exe groups --help
GraphProcessor 1.0.0+19e59a88f38e644ca0e145a06eb730371fd9df2b
Copyright (C) 2024 GraphProcessor

  --output     Output folder's path; current directory is used as default

  --help       Display this help screen.

  --version    Display version information.
```

## Commands
A command name is always required as first argument. Each command might expect different arguments, provided via the `--[attributeName] attributeValue` syntax.

### Groups
Finds and downloads all AAD Groups into a specified location as multiple Json files, one for each group. More precisely, the command creates a subfolder named *MSGraph/Groups* into thespecified path. If no path is specified, the current directory is used. If a group has an empty *DisplayName* property, it's ignored.

**Warning**: The output subfolder is cleaned up before each execution.

Usage:
```
GraphProcessor.exe groups
GraphProcessor.exe groups --output "D:\SomeFolder"
```

Known Issues:
* Command fails when processing a Group having an illegal character for the current File System in its name, e.g. backslash *\\*

## Authentication Setup
There are multiple ways to authenticate to the correct Azure Tenant. Available methods are chosen in this order:
1. User Secrets (debugging only)
2. AppSettings file
3. [DefaultAzureCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet)'s methods
    1. Environment Variables
    2. Workload
    3. Managed Identity
    4. Etc.

If you have your credentials already set in Visual Studio, or if your machine has an already configured managed identity, then you don't need to do anything.

### User Secrets
**Warning**: This works only if debugging, more precisely if the Application is running with the DOTNET_ENVIRONMENT env. variable set to "Development".

You should place a *secrets.json* file at a specific path. You can access the file directly in Visual Studio by right-clicking the GraphProcessor project and selecting *Manage User Secrets*. In Windows: 
```
%APPDATA%\Microsoft\UserSecrets\5f8d1eee-60df-4cc7-9301-27d70a22c9bc\secrets.json
```

```json
{
  "TokenCredentialFactory": {
    "TenantId": "xxxx",
    "ClientId": "yyyy",
    "Secret": "zzzz"
  }
}
```

You can find more info on this here: [Safe storage of app secrets in development in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows)

###  AppSettings file
Add to either the *appsettings.json* or *appsettings.\[Environment\].json* file the following section (be sure to not accidentally push secrets into the repo):

```json
"TokenCredentialFactory": {
    "TenantId": "xxxx",
    "ClientId": "yyyy",
    "Secret": "zzzz"
  }
```

### DefaultAzureCredentials
This class is by default configured with different authentication methods. 

Required environment variables, when using the [EnvironmentCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential?view=azure-dotnet) method, are:
* AZURE_TENANT_ID
* AZURE_CLIENT_ID
* AZURE_CLIENT_SECRET

## Debugging
Solution is already shipped with a LaunchProfile called "Debug". This sets the *DOTNET_ENVIRONMENT* environment variable as *Development* and executes the "groups" command.

Provided you have correctly configured the authentication, there's nothing else to do in order to be able to debug.
