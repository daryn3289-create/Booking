namespace Profile.Api.Consumers;

public class UserCreatedConsumerHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserCreatedConsumerHostedService> _logger;

    public UserCreatedConsumerHostedService(
        ILogger<UserCreatedConsumerHostedService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserCreatedConsumer started");
        await using var scope = _scopeFactory.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<UserCreatedConsumer>();
        
        await consumer.StartAsync(stoppingToken);
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await using var scope = _scopeFactory.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<UserCreatedConsumer>();
        
        await consumer.DisposeAsync();
        _logger.LogInformation("UserCreatedConsumer stopped");
    }
}