namespace Clean.Messaging.Outbox.Transport;

public sealed record OutboxTarget(
    string TargetId,
    string Transport,
    string? Destination);


public interface IOutboxTargetProvider
{
    IEnumerable<OutboxTarget> GetTargets(
        Type messageType,
        object message);
}