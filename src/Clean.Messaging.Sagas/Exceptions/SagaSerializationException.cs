using Clean.Messaging.Exceptions;

namespace Clean.Messaging.Sagas.Exceptions;

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