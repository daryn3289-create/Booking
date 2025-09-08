using System.Text;
using System.Text.Json;
using Booking.Shared.Infrastructure.Messaging;
using Booking.Shared.Infrastructure.Messaging.Contracts;
using Profile.Domain;
using Profile.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Profile.Api.Consumers;

public class UserCreatedConsumer : IAsyncDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRabbitMqFactory _factory;
    private readonly ILogger<UserCreatedConsumer> _logger;
    private IChannel? _channel;

    public UserCreatedConsumer(
        IRabbitMqFactory factory,
        IServiceScopeFactory scopeFactory,
        ILogger<UserCreatedConsumer> logger)
    {
        _factory = factory;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = await _factory.CreateConnectionAsync(cancellationToken);
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: "users",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);

                var userCreated = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                if (userCreated is null)
                {
                    _logger.LogWarning("Received null UserCreatedEvent");
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, false, cancellationToken); // discard
                    return;
                }

                await using var scope = _scopeFactory.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();

                var user = new User
                {
                    ClientId = userCreated.ClientId,
                    Username = userCreated.Username,
                    Email = userCreated.Email,
                    FirstName = userCreated.FirstName,
                    LastName = userCreated.LastName
                };

                context.Users.Add(user);
                await context.SaveChangesAsync(cancellationToken);

                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false,
                    cancellationToken);
                _logger.LogInformation("Created profile for user {UserId}", userCreated.ClientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle UserCreatedEvent");
                if (_channel is not null)
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, true, cancellationToken); // requeue
            }
        };
        
        await _channel.BasicConsumeAsync(
            queue: "users",
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.CloseAsync();
        _channel?.Dispose();
        _logger.LogInformation("UserCreatedConsumer disposed");
    }
}