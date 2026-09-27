namespace Clean.Messaging.Inbox.Ingress;

public sealed class InboxIngressException
    : Exception
{
    public InboxIngressException(
        string message)
        : base(message)
    {
    }

    public InboxIngressException(
        string message,
        Exception innerException)
        : base(
            message,
            innerException)
    {
    }
}