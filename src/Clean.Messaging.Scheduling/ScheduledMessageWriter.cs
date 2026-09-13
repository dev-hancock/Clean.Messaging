using Clean.Messaging.Serialization;

namespace Clean.Messaging.Scheduling;

internal sealed class ScheduledMessageWriter(
    IScheduledMessageStore store,
    IMessageSerializer serializer,
    TimeProvider time)
    : IScheduledMessageWriter
{
    public ScheduleId Schedule<TMessage>(
        ScheduleId id,
        TMessage message,
        DateTimeOffset dueAt,
        ScheduledMessageTarget target,
        ScheduleGroupId? groupId,
        Guid? correlationId,
        Guid? causationId)
        where TMessage : notnull
    {
        Validate(id);

        if (groupId is { } group)
        {
            Validate(group);
        }

        ArgumentNullException.ThrowIfNull(message);

        store.Add(
            new()
            {
                Id = id,
                GroupId = groupId,
                Target = target,
                Message = serializer.Serialize(message),
                DueAt = dueAt.UtcDateTime,
                CorrelationId = correlationId,
                CausationId = causationId,
                CreatedAt = time.GetUtcNow().UtcDateTime
            });

        return id;
    }

    public ValueTask Cancel(
        ScheduleId id,
        ScheduledMessageTarget target,
        ScheduleGroupId? groupId,
        CancellationToken cancellationToken)
    {
        Validate(id);

        if (groupId is { } group)
        {
            Validate(group);
        }

        return store.Cancel(
            id,
            groupId,
            target,
            time.GetUtcNow().UtcDateTime,
            cancellationToken);
    }

    public ValueTask CancelGroup(
        ScheduleGroupId groupId,
        ScheduledMessageTarget target,
        ScheduleId? exceptId,
        CancellationToken cancellationToken)
    {
        Validate(groupId);

        if (exceptId is { } id)
        {
            Validate(id);
        }

        return store.CancelGroup(
            groupId,
            target,
            exceptId,
            time.GetUtcNow().UtcDateTime,
            cancellationToken);
    }

    private static void Validate(
        ScheduleId id)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Schedule ID cannot be empty.",
                nameof(id));
        }
    }

    private static void Validate(
        ScheduleGroupId id)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Schedule group ID cannot be empty.",
                nameof(id));
        }
    }
}
