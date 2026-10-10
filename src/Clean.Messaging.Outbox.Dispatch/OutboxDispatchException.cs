using Clean.Messaging.Exceptions;

namespace Clean.Messaging.Outbox.Dispatch;

public sealed class OutboxDispatchException
    : MessageException
{
    public OutboxDispatchException(
        string message,
        Exception? innerException = null)
        : base(
            "outbox.dispatch.failed",
            message,
            FailureAction.Fault,
            innerException)
    {
    }
}