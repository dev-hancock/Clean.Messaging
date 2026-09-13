using Clean.Messaging.Serialization;

namespace Clean.Messaging.Scheduling;

public readonly record struct ScheduledMessageDispatch(
    ScheduleId ScheduleId,
    ScheduleGroupId? ScheduleGroupId,
    MessageData MessageData,
    Guid? CorrelationId,
    Guid? CausationId);
