using Azure.Core;
using GraphProcessor.Configs;
using GraphProcessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace GraphProcessor.CommandHandlers;

/// <summary>
/// Command execution result
/// </summary>
/// <param name="Location"></param>
/// <param name="Count"></param>
public record DownloadGroupsResult(string Location, long Count);

/// <summary>
/// Main business logic
/// </summary>
public class DownloadGroupsCommandHandler(ILogger<DownloadGroupsCommandHandler> logger, GraphServiceClient graphClient, GroupWriter groupWriter)
{
    public async Task<DownloadGroupsResult> ExecuteAsync(string outputPath)
    {
        var outputLocation = Path.Combine(outputPath, "MSGraph", "Groups");

        //execute first call setting the page size
        var response = await graphClient.Groups.GetAsync(requestConfig =>
        {
            requestConfig.QueryParameters.Top = 10;
            requestConfig.QueryParameters.Count = true;
            requestConfig.Headers.Add("ConsistencyLevel", "eventual");
        });

        //if empty response or zero results: return
        if (response == null || (response.OdataCount ?? 0L) == 0L)
        {
            logger.LogDebug("Empty response");
            return new DownloadGroupsResult(outputLocation, 0);
        }

        //clear the output folder
        groupWriter.Reset(outputLocation);

        //setup the pageIterator: needs a callback to be executed for each read item in each page
        var pageIterator = PageIterator<Group, GroupCollectionResponse>
            .CreatePageIterator(
                graphClient,
                response,
                async (group) =>
                {
                    await groupWriter.WriteGroupAsync(outputLocation, group);
                    return true;
                });

        await pageIterator.IterateAsync();
        return new DownloadGroupsResult(outputLocation, response.OdataCount ?? 0L);
    }
}