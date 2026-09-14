using Clean.Messaging.Exceptions;

namespace Clean.Messaging.Sagas.EntityFrameworkCore;

internal sealed class SagaRaceException(
    string sagaType,
    SagaKey key,
    Guid messageId)
    : MessageException(
        "saga.start.race",
        $"Saga '{sagaType}' start raced for key '{key}' with message '{messageId}'.",
        FailureAction.Retry)
{
    public string SagaType { get; } = sagaType;

    public SagaKey Key { get; } = key;

    public Guid MessageId { get; } = messageId;
}
