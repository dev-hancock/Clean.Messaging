using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Formatter;

namespace Clean.Messaging.Outbox.Mqtt.Connection;

internal sealed class MqttConnection(
    IOptions<MqttOutboxOptions> options) : IDisposable
{
    private readonly IMqttClient _client =
        new MqttClientFactory().CreateMqttClient();
    private readonly MqttOutboxOptions _options =
        options.Value;

    private readonly SemaphoreSlim _connect = new(1, 1);

    public async ValueTask<IMqttClient> GetClient(
        CancellationToken cancellationToken)
    {
        if (_client.IsConnected)
        {
            return _client;
        }

        await _connect.WaitAsync(cancellationToken);

        try
        {
            if (_client.IsConnected)
            {
                return _client;
            }

            await _client.ConnectAsync(
                CreateOptions(),
                cancellationToken);

            if (!_client.IsConnected)
            {
                throw new InvalidOperationException(
                    "The MQTT broker rejected the connection.");
            }

            return _client;
        }
        finally
        {
            _connect.Release();
        }
    }

    private MqttClientOptions CreateOptions()
    {
        var builder = new MqttClientOptionsBuilder()
            .WithProtocolVersion(
                MqttProtocolVersion.V500)
            .WithClientId(
                _options.ClientId)
            .WithTcpServer(
                _options.Host,
                _options.Port);

        if (!string.IsNullOrWhiteSpace(
                _options.Username))
        {
            builder.WithCredentials(
                _options.Username,
                _options.Password);
        }

        if (_options.UseTls)
        {
            builder.WithTlsOptions(_ => { });
        }

        return builder.Build();
    }

    public void Dispose()
    {
        _connect.Dispose();
        _client.Dispose();
    }
}
