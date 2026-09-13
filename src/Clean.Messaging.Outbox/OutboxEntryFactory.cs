using Clean.Messaging.Abstractions;
using Clean.Messaging.Serialization;

namespace Clean.Messaging.Outbox;

public sealed class OutboxEntryFactory(
    IEnumerable<IOutboxTargetProvider> targets,
    IMessageSerializer serializer,
    IMessageContext context,
    TimeProvider time)
{
    public OutboxBatch Create<T>(
        OutboxMessage<T> message)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Message);

        if (message.Id == Guid.Empty)
        {
            throw new ArgumentException(
                "Message ID cannot be empty.",
                nameof(message));
        }

        var messageId = Guid.NewGuid();
        var messageType = message.Message.GetType();
        var payload = serializer.Serialize(message.Message);
        var createdAt = time.GetUtcNow().UtcDateTime;

        var delivery = targets
            .SelectMany(provider =>
                provider.GetTargets(
                    messageType,
                    message.Message))
            .ToArray();

        Validate(delivery);

        var entries = delivery
            .Select(target =>
                new OutboxEntry
                {
                    MessageId = messageId,
                    EventId = message.Id,
                    TargetId = target.TargetId,
                    Transport = target.Transport,
                    Destination = target.Destination,
                    Message = payload,
                    CorrelationId =
                        message.CorrelationId ??
                        context.CorrelationId,
                    CausationId =
                        message.CausationId ??
                        context.MessageId,
                    CreatedAt = createdAt
                })
            .ToArray();

        return new(
            messageId,
            message.Id,
            entries);
    }

    private static void Validate(
        IReadOnlyCollection<OutboxTarget> targets)
    {
        foreach (var target in targets)
        {
            ValidateTargetId(target);
            ValidateTransport(target);
        }

        ValidateDuplicates(targets);
    }

    private static void ValidateTargetId(
        OutboxTarget target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            target.TargetId);

        if (target.TargetId.Length > 200 ||
            target.TargetId.Any(character => character > 127))
        {
            throw new ArgumentException(
                "Outbox target identities must be at most 200 ASCII characters.",
                nameof(target));
        }
    }

    private static void ValidateTransport(
        OutboxTarget target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            target.Transport);
    }

    private static void ValidateDuplicates(
        IEnumerable<OutboxTarget> targets)
    {
        var duplicate = targets
            .GroupBy(
                target => target.TargetId,
                StringComparer.Ordinal)
            .FirstOrDefault(group =>
                group.Skip(1).Any());

        if (duplicate is null)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Outbox target '{duplicate.Key}' is registered more than once.");
    }
}