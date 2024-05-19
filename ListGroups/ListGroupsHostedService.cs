using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ListGroups;

internal class ListGroupsHostedService(IHostApplicationLifetime applicationLifetime, ILogger<ListGroupsHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //TODO: BL here

        //When the Business Logic terminates, Application Stop is requested. This might not work properly when there are multiple hosted services
        applicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
