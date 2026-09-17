using Clean.Messaging.Exceptions;

namespace Clean.Messaging.Sagas.Exceptions;

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