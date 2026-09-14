using Clean.Messaging.Exceptions;

namespace Clean.Messaging.Sagas;

public sealed class SagaConflictException(
    string sagaType,
    SagaKey key)
    : MessageException(
        "saga.start.conflict",
        $"Active saga '{sagaType}' already exists for key '{key}'.",
        FailureAction.Fault)
{
    public string SagaType { get; } = sagaType;

    public SagaKey Key { get; } = key;
}

public sealed class SagaClosedException(
    string sagaType,
    SagaKey key)
    : MessageException(
        "saga.start.closed",
        $"Terminal saga '{sagaType}' already exists for key '{key}'.",
        FailureAction.Fault)
{
    public string SagaType { get; } = sagaType;

    public SagaKey Key { get; } = key;
}

public sealed class SagaNotFoundException(
    string sagaType,
    SagaKey key)
    : MessageException(
        "saga.not_found",
        $"Saga '{sagaType}' was not found for key '{key}'.",
        FailureAction.Retry)
{
}

public sealed class SagaTimerNotFoundException(
    SagaId sagaId)
    : MessageException(
        "saga.timer.saga_not_found",
        $"Saga '{sagaId}' for timer delivery was not found.",
        FailureAction.Fault)
{
}

public sealed class SagaSerializationException(
    string message,
    Exception? innerException = null)
    : MessageException(
        "saga.serialization.failed",
        message,
        FailureAction.Fault,
        innerException)
{
}