using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ListGroups;
using Azure.Core;
using ListGroups.Configs;
using CommandLine;
using Microsoft.Extensions.Logging;

//CL arguments parsing
CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
    .WithParsed(async options =>
     {
         await InitAndStartApplication(options);
     });

async Task InitAndStartApplication(CommandLineOptions clOptions)
{
    /*
     * Default Host creation:
     * - Sets the Environment using the DOTNET_ENVIRONMENT env. variable
     * - Initializes the ILogger with the Console sink
     * - Initializes the IConfigurationBuilder with arguments, env. variables, and - if launched with the Debug launch setting - the user secrets
    */
    var builder = Host.CreateApplicationBuilder();

    //TokenCredential initialize
    var tokenCredentials = InitTokenCredential(clOptions);

    //DI configuration
    builder.Services.AddSingleton(tokenCredentials);
    builder.Services.AddSingleton<GraphServiceClient>();
    builder.Services.AddHostedService<ListGroupsHostedService>();

    using IHost host = builder.Build();
    await host.RunAsync();
}

/*
 * Credentials info are selected from (from the highest to the lowest priority):
 * - Console args
 * - User Secrets (only if Environment = Development)
 * - Environment Variables
 * - Other mechanisms, e.g. Managed Identity (check the DefaultAzureCredential class' doc: https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet)
 */
TokenCredential InitTokenCredential(CommandLineOptions clOptions)
{
    return (!string.IsNullOrWhiteSpace(clOptions.TenantId) && !string.IsNullOrWhiteSpace(clOptions.ClientId) && !string.IsNullOrWhiteSpace(clOptions.Secret))
        ? new ClientSecretCredential(clOptions.TenantId, clOptions.ClientId, clOptions.Secret)
        : new DefaultAzureCredential();
}