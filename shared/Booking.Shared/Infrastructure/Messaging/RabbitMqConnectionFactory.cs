using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Booking.Shared.Infrastructure.Messaging;

public interface IRabbitMqFactory
{
    ValueTask<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
    ValueTask<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);
}

public class RabbitMqFactory : IRabbitMqFactory, IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;

    public RabbitMqFactory(IOptions<RabbitMqOptions> options)
    {
        var o = options.Value;
        _factory = new ConnectionFactory
        {
            HostName = o.HostName,
            Port = o.Port,
            UserName = o.Username,
            Password = o.Password,
            VirtualHost = o.VirtualHost,
        };
    }

    public async ValueTask<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        _connection = await _factory.CreateConnectionAsync(cancellationToken);
        return _connection;
    }

    public async ValueTask<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        var connection = await CreateConnectionAsync(cancellationToken);
        return await connection.CreateChannelAsync(cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }
}
