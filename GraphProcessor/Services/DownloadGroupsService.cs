﻿using Azure.Core;
using GraphProcessor.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace GraphProcessor.Services;

/// <summary>
/// Main business logic
/// </summary>
public class DownloadGroupsService(ILogger<DownloadGroupsService> logger, IOptions<DownloadGroupsOptions> options, GraphServiceClient graphClient, GroupWriter groupWriter)
{
    public async Task<DownloadGroupsResult> ExecuteAsync()
    {
        var optionsValue = options.Value;
        var outputLocation = Path.Combine(optionsValue.OutputPath, "MSGraph", "Groups");

        //execute first call setting the page size
        var response = await graphClient.Groups.GetAsync(requestConfig =>
        {
            requestConfig.QueryParameters.Top = optionsValue.PageSize;
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