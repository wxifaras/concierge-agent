namespace concierge_agent_api.Services;

using System.Threading;
using System.Threading.Tasks;

public class SmsQueueProcessor : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}