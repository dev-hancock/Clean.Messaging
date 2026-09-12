using Clean.Messaging.Consumers;

namespace Clean.Messaging.Outbox;

internal sealed class OutboxTargetProvider(
    IConsumerRegistry consumers)
    : IOutboxTargetProvider
{
    public IEnumerable<OutboxTarget> GetTargets(
        Type messageType,
        object message)
    {
        return consumers
            .Get(messageType)
            .Select(consumer =>
                new OutboxTarget(
                    consumer.ConsumerId,
                    LocalTransport.Name,
                    null));
    }
}