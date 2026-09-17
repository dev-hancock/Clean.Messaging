using Microsoft.Extensions.Options;
using MQTTnet.Protocol;

namespace Clean.Messaging.Outbox.Mqtt;

internal sealed class MqttOutboxOptionsValidator
    : IValidateOptions<MqttOutboxOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        MqttOutboxOptions options)
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

        if (options.QualityOfService is not
            MqttQualityOfServiceLevel.AtLeastOnce and not
            MqttQualityOfServiceLevel.ExactlyOnce)
        {
            failures.Add(
                "MQTT outbox requires QoS 1 or QoS 2.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}