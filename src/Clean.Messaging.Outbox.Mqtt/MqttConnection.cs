using MQTTnet;

namespace Clean.Messaging.Outbox.Mqtt;

internal sealed class MqttConnection(
    MqttClientOptions clientOptions) : IDisposable
{
    private readonly IMqttClient _client =
        new MqttClientFactory().CreateMqttClient();

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
                clientOptions,
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

    public void Dispose()
    {
        _connect.Dispose();
        _client.Dispose();
    }
}
