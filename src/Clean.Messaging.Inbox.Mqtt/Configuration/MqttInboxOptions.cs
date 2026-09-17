using MQTTnet.Protocol;

namespace Clean.Messaging.Inbox.Mqtt.Configuration;

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

    /// <summary>
    /// Discards the broker-held session on the first successful connection
    /// after this process starts. Subsequent reconnects remain persistent.
    /// </summary>
    public bool ResetSessionOnStart { get; set; }
}
