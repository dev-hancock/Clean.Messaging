using Microsoft.Extensions.Options;
using MQTTnet.Protocol;

namespace Clean.Messaging.Inbox.Mqtt.Configuration;

internal sealed class MqttInboxOptionsValidator
    : IValidateOptions<MqttInboxOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        MqttInboxOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            failures.Add(
                "MQTT host is required.");
        }

        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            failures.Add(
                "MQTT client ID is required.");
        }

        if (options.Port <= 0)
        {
            failures.Add(
                "MQTT port must be greater than zero.");
        }

        if (options.SessionExpiryInterval == 0)
        {
            failures.Add(
                "MQTT session expiry must be greater than zero.");
        }

        if (options.ReconnectDelay <= TimeSpan.Zero)
        {
            failures.Add(
                "MQTT reconnect delay must be greater than zero.");
        }

        if (options.ReconnectDelay >
            TimeSpan.FromMilliseconds(uint.MaxValue - 1))
        {
            failures.Add(
                "MQTT reconnect delay is too large.");
        }

        if (options.QualityOfServiceLevel is not
            MqttQualityOfServiceLevel.AtLeastOnce and not
            MqttQualityOfServiceLevel.ExactlyOnce)
        {
            failures.Add(
                "MQTT inbox requires QoS 1 or QoS 2.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}