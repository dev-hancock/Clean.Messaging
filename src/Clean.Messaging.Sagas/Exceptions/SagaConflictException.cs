using Clean.Messaging.Exceptions;

namespace Clean.Messaging.Sagas.Exceptions;

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