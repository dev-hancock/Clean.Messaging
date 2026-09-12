using MQTTnet.Protocol;

namespace Clean.Messaging.Inbox.Mqtt;

public sealed class MqttInboxOptions
{
    public required string Host { get; set; }

    public int Port { get; set; } = 1883;

    public required string ClientId { get; set; }

    public uint SessionExpiryInterval { get; set; } = uint.MaxValue;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool UseTls { get; set; }

    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);

    public MqttQualityOfServiceLevel QualityOfServiceLevel { get; set; }
        = MqttQualityOfServiceLevel.AtLeastOnce;

    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Host);
        ArgumentException.ThrowIfNullOrWhiteSpace(ClientId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Port);
        ArgumentOutOfRangeException.ThrowIfZero(
            SessionExpiryInterval);

        if (ReconnectDelay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ReconnectDelay),
                ReconnectDelay,
                "Reconnect delay must be greater than zero.");
        }

        if (ReconnectDelay > TimeSpan.FromMilliseconds(uint.MaxValue - 1))
        {
            throw new ArgumentOutOfRangeException(
                nameof(ReconnectDelay),
                ReconnectDelay,
                "Reconnect delay must be supported by Task.Delay.");
        }
    }
}
