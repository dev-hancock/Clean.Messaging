using Microsoft.Extensions.Options;
using MQTTnet.Protocol;

namespace Clean.Messaging.Inbox.Mqtt;

internal sealed class MqttInboxOptionsValidator
    : IValidateOptions<MqttInboxOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        MqttInboxOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
        {
            return ValidateOptionsResult.Fail(
                "MQTT host is required.");
        }

        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            return ValidateOptionsResult.Fail(
                "MQTT client ID is required.");
        }

        if (options.Port <= 0)
        {
            return ValidateOptionsResult.Fail(
                "MQTT port must be greater than zero.");
        }

        if (options.SessionExpiryInterval == 0)
        {
            return ValidateOptionsResult.Fail(
                "MQTT session expiry must be greater than zero.");
        }

        if (options.ReconnectDelay <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                "MQTT reconnect delay must be greater than zero.");
        }

        if (options.ReconnectDelay > TimeSpan.FromMilliseconds(uint.MaxValue - 1))
        {
            return ValidateOptionsResult.Fail(
                "MQTT reconnect delay is too large.");
        }

        if (options.QualityOfServiceLevel is not
                MqttQualityOfServiceLevel.AtLeastOnce and not
                MqttQualityOfServiceLevel.ExactlyOnce)
        {
            return ValidateOptionsResult.Fail(
                "MQTT inbox requires QoS 1 or QoS 2.");
        }

        return ValidateOptionsResult.Success;
    }
}
