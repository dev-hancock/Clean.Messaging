using Clean.Messaging.Consumers;
using Clean.Messaging.Serialization;

namespace Clean.Messaging.Processing;

public sealed class ConsumerDispatcher(
    IConsumerRegistry consumers,
    IMessageSerializer serializer,
    IServiceProvider services)
{
    public ValueTask Dispatch(
        string consumerId,
        MessageData data,
        CancellationToken cancellationToken)
    {
        var consumer = consumers.Get(consumerId);
        var message = serializer.Deserialize(data);

        return consumer.Invoke(
            services,
            message,
            cancellationToken);
    }
}
