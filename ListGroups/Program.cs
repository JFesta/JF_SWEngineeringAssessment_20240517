using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ListGroups;
using Azure.Core;

/*
 * Default Host creation:
 * - Sets the Environment using the DOTNET_ENVIRONMENT env. variable
 * - Initializes the ILogger with the Console sink
 * - Initializes the IConfigurationBuilder with arguments, env. variables, and - if launched with the Debug launch setting - the user secrets
*/
var builder = Host.CreateApplicationBuilder(args);

//TokenCredential initialize
var tokenCredentials = InitTokenCredential();

//DI configuration
builder.Services.AddSingleton(tokenCredentials);
builder.Services.AddHostedService<ListGroupsHostedService>();

using IHost host = builder.Build();
await host.RunAsync();

TokenCredential InitTokenCredential()
{
    return new DefaultAzureCredential();
}