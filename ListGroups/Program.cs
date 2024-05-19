using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Core;
using ListGroups.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CommandLine;
using ListGroups.Services;

//CL arguments parsing
CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
    .WithParsed(async clOptions =>
    {
        //actions if parsing OK
        try
        {
            using var host = InitApplication(clOptions);
            await StartApplication(host);
        }
        catch (Exception ex)
        {
            //catch-all block
            Console.Error.WriteLine(ex);
        }
    });

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
    builder.Services.AddScoped<GraphServiceClient>();

    //Business Logic
    builder.Services.AddScoped<ListGroupsService>();

    return builder.Build();
}

async Task StartApplication(IHost host)
{
    using var scope = host.Services.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<ListGroupsService>();
    await service.ExecuteAsync();
}

void AddConsoleArguments(CommandLineOptions clOptions, ConfigurationManager configurationManager)
{
    configurationManager[$"ListGroups:{nameof(ListGroupsOptions.OutputPath)}"] = clOptions.OutputPath ?? Environment.CurrentDirectory;

    configurationManager[$"TokenCredentialFactory:{nameof(TokenCredentialFactoryOptions.TenantId)}"] = clOptions.TenantId;
    configurationManager[$"TokenCredentialFactory:{nameof(TokenCredentialFactoryOptions.ClientId)}"] = clOptions.ClientId;
    configurationManager[$"TokenCredentialFactory:{nameof(TokenCredentialFactoryOptions.Secret)}"] = clOptions.Secret;
}