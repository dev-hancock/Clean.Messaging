using MQTTnet.Protocol;

namespace Clean.Messaging.Outbox.Mqtt;


public sealed class MqttOutboxOptions
{
    public required string Host { get; set; }

    public int Port { get; set; } = 1883;

    public required string ClientId { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool UseTls { get; set; }

    public MqttQualityOfServiceLevel QualityOfService { get; set; } =
        MqttQualityOfServiceLevel.AtLeastOnce;
}