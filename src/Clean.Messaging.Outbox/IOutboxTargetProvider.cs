namespace Clean.Messaging.Outbox;

public sealed record OutboxTarget(
    string Id,
    string Transport,
    string? Destination);


public interface IOutboxTargetProvider
{
    IEnumerable<OutboxTarget> GetTargets(
        Type messageType,
        object message);
}