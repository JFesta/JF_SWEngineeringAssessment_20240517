using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Core;
using ListGroups.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ListGroups.Services;
using Microsoft.Graph.Models;
using ListGroups.Tokens;

try
{
    if (TryParseArgs(args, out var clOptions))
    {
        using var host = InitApplication(clOptions);
        await ExecuteApplication(host);
    }
}
catch (Exception ex)
{
    //catch-all block
    Console.Error.WriteLine(ex);
}

IHost InitApplication(CommandLineOptions clOptions)
{
    /*
     * Default Host creation:
     * - Sets the Environment using the DOTNET_ENVIRONMENT env. variable
     * - Initializes the ILogger with the Console sink
     * - Initializes the IConfigurationBuilder with env. variables, the appsettings.json file, and - if launched with the Debug launch setting - the user secrets
     * Arguments aren't injected directly since we need to convert their names first, and this is done by the AddConsoleArguments method
    */
    var builder = Host.CreateApplicationBuilder();

    //registers console args individually into the ConfigurationManager
    AddConsoleArguments(clOptions, builder.Configuration);

    //Options setup
    builder.Services.Configure<ListGroupsOptions>(builder.Configuration.GetSection("ListGroups"));
    builder.Services.Configure<TokenCredentialFactoryOptions>(builder.Configuration.GetSection("TokenCredentialFactory"));

    //TokenCredential: singleton token handler
    builder.Services.AddSingleton<TokenCredentialFactory>();
    builder.Services.AddSingleton<TokenCredential>(provider => provider.GetRequiredService<TokenCredentialFactory>().Create());

    //Graph SDK client
    builder.Services.AddScoped<GraphServiceClient>(provider => new GraphServiceClient(provider.GetRequiredService<TokenCredential>()));

    //Business Logic
    builder.Services.AddScoped<ListGroupsService>();

    return builder.Build();
}

async Task ExecuteApplication(IHost host)
{
    using var scope = host.Services.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<ListGroupsService>();
    var result  = await service.ExecuteAsync();
    Console.WriteLine("Completed Group list!");
    Console.WriteLine("Group Count: {0}", result.Count);
    Console.WriteLine("Output location: {0}", result.Location);
}

bool TryParseArgs(string[] args, out CommandLineOptions clOptions)
{
    var result = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args);
    clOptions = result.Value;
    if (result.Errors?.Any() ?? false)
        return false;
    clOptions = result.Value;
    return true;
}

void AddConsoleArguments(CommandLineOptions clOptions, ConfigurationManager configurationManager)
{
    configurationManager[$"ListGroups:{nameof(ListGroupsOptions.OutputPath)}"] = clOptions.OutputPath ?? Environment.CurrentDirectory;

    if (!string.IsNullOrWhiteSpace(clOptions.TenantId) && !string.IsNullOrWhiteSpace(clOptions.ClientId) && !string.IsNullOrWhiteSpace(clOptions.Secret))
    {
        configurationManager[$"TokenCredentialFactory:{nameof(TokenCredentialFactoryOptions.TenantId)}"] = clOptions.TenantId;
        configurationManager[$"TokenCredentialFactory:{nameof(TokenCredentialFactoryOptions.ClientId)}"] = clOptions.ClientId;
        configurationManager[$"TokenCredentialFactory:{nameof(TokenCredentialFactoryOptions.Secret)}"] = clOptions.Secret;
    }
}