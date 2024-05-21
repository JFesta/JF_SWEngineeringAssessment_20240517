using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using GraphProcessor.Configs;
using GraphProcessor.Arguments;
using GraphProcessor.Tokens;
using GraphProcessor.Services;
using GraphProcessor.CommandHandlers;

try
{
    if (TryParseArgs(args, out var parsedOptions, out var parsedType))
    {
        using var host = InitApplication();
        switch (parsedOptions)
        {
            case CliDownloadGroupsOptions cliDownloadGroupsOptions:
                await ExecuteDownloadGroups(host, cliDownloadGroupsOptions);
                break;
        }
    }
}
catch (Exception ex)
{
    //catch-all block
    Console.Error.WriteLine(ex);
}

IHost InitApplication()
{
    /*
     * Default Host creation:
     * - Sets the Environment using the DOTNET_ENVIRONMENT env. variable
     * - Initializes the ILogger with the Console sink
     * - Initializes the IConfigurationBuilder with env. variables, the appsettings.json file, and - if launched with the Debug launch setting - the user secrets
    */
    var builder = Host.CreateApplicationBuilder();

    //Options setup
    builder.Services.Configure<TokenCredentialFactoryOptions>(builder.Configuration.GetSection("TokenCredentialFactory"));

    //TokenCredential: singleton token handler
    builder.Services.AddSingleton<TokenCredentialFactory>();
    builder.Services.AddSingleton<TokenCredential>(provider => provider.GetRequiredService<TokenCredentialFactory>().Create());

    //Graph SDK client
    builder.Services.AddScoped<GraphServiceClient>();

    //Business Logic
    builder.Services.AddScoped<DownloadGroupsCommandHandler>();
    builder.Services.AddScoped<GroupWriter>();

    return builder.Build();
}

async Task ExecuteDownloadGroups(IHost host, CliDownloadGroupsOptions input)
{
    using var scope = host.Services.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<DownloadGroupsCommandHandler>();
    var result  = await service.ExecuteAsync(input.OutputPath!);
    Console.WriteLine("Completed Group download!");
    Console.WriteLine("Group Count: {0}", result.Count);
    Console.WriteLine("Output location: {0}", result.Location);
}

/*
 * Returns true if the arguments were correctly parsed; in this case the actual object and its type are returned as well as
 * output parameters.
*/
bool TryParseArgs(string[] args, out object parsedOptions, out Type parsedType)
{
    var result = CommandLine.Parser.Default.ParseArguments(args, typeof(CliDownloadGroupsOptions));
    parsedOptions = result.Value;
    parsedType = result.TypeInfo.Current;
    return !(result.Errors?.Any() ?? false);
}