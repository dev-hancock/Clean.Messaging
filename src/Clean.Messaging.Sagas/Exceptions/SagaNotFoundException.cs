using Clean.Messaging.Exceptions;

namespace Clean.Messaging.Sagas.Exceptions;

public sealed class SagaNotFoundException
    : MessageException
{
    public SagaNotFoundException(
        string sagaType,
        SagaKey key)
        : base(
            "saga.not_found",
            $"Saga '{sagaType}' was not found for key '{key}'.",
            FailureAction.Retry)
    {
    }

    public SagaNotFoundException(
        SagaId sagaId)
        : base(
            "saga.not_found",
            $"Saga '{sagaId}' was not found.",
            FailureAction.Retry)
    {
    }
}
