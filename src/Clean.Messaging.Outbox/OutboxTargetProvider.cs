using Clean.Messaging.Consumers;

namespace Clean.Messaging.Outbox;

public sealed class OutboxTargetProvider(
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
                    TargetId: consumer.ConsumerId,
                    Transport: LocalTransport.Name,
                    Destination: null));
    }
}