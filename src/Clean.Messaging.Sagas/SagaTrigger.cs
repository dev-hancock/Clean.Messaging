namespace Clean.Messaging.Sagas;

internal readonly record struct SagaTrigger(
    Guid MessageId,
    Guid? CorrelationId,
    Guid? CausationId,
    Guid? TimerId = null);