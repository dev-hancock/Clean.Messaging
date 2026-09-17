namespace Clean.Messaging.Sagas.Runtime;

internal readonly record struct SagaTrigger(
    Guid MessageId,
    Guid? CorrelationId,
    Guid? CausationId,
    Guid? TimerId = null);