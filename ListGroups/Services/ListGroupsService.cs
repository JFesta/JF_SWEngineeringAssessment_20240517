using Azure.Core;
using ListGroups.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace ListGroups.Services;

/// <summary>
/// Main business logic
/// </summary>
public class ListGroupsService(ILogger<ListGroupsService> logger, IOptions<ListGroupsOptions> options, GraphServiceClient graphClient)
{

    public async Task ExecuteAsync()
    {
        var optionsValue = options.Value;

        //execute first call setting the page size
        var response = await graphClient.Groups.GetAsync(requestConfig =>
        {
            requestConfig.QueryParameters.Top = optionsValue.PageSize;
        });

        if (response == null)
        {
            logger.LogWarning("Empty response");
            return;
        }

        //setup the pageIterator: needs a callback to be executed for each read item on each page
        var pageIterator = PageIterator<Group, GroupCollectionResponse>
            .CreatePageIterator(
                graphClient,
                response,
                (group) =>
                {
                    logger.LogInformation("{Id}\t{DisplayName}", group.Id, group.DisplayName);
                    return true;
                });

        await pageIterator.IterateAsync();
    }
}
