using Clean.Messaging.Serialization;

namespace Clean.Messaging.Scheduling.Delivery;

public readonly record struct ScheduledDispatch(
    ScheduleId ScheduleId,
    ScheduleGroupId? ScheduleGroupId,
    MessageData MessageData,
    Guid? CorrelationId,
    Guid? CausationId);