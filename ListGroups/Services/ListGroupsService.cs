using ListGroups.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace ListGroups.Services;

/// <summary>
/// Main business logic
/// </summary>
public class ListGroupsService(ILogger<ListGroupsService> logger, IOptions<ListGroupsOptions> options, GraphServiceClient graphClient)
{

    public async Task ExecuteAsync()
    {
        //TODO: BL here
        logger.LogInformation("Goodbye, World {OutputPath}", options.Value.OutputPath);
    }
}
