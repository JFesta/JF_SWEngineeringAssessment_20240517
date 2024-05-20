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
    public async Task<ListGroupsResult> ExecuteAsync()
    {
        var optionsValue = options.Value;
        var outputLocation = Path.Combine(optionsValue.OutputPath, "MSGraph/Groups");

        //execute first call setting the page size
        var response = await graphClient.Groups.GetAsync(requestConfig =>
        {
            requestConfig.QueryParameters.Top = optionsValue.PageSize;
            requestConfig.QueryParameters.Count = true;
            requestConfig.Headers.Add("ConsistencyLevel", "eventual");
        });

        if (response == null || (response.OdataCount ?? 0L) == 0L)
        {
            logger.LogDebug("Empty response");
            return new ListGroupsResult(outputLocation, 0);
        }

        //setup the pageIterator: needs a callback to be executed for each read item on each page
        var pageIterator = PageIterator<Group, GroupCollectionResponse>
            .CreatePageIterator(
                graphClient,
                response,
                async (group) =>
                {
                    logger.LogInformation("{Id}\t{DisplayName}", group.Id, group.DisplayName);
                    return true;
                });

        await pageIterator.IterateAsync();
        return new ListGroupsResult(outputLocation, response.OdataCount ?? 0L);
    }
}