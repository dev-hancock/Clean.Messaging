namespace Clean.Messaging.Abstractions;

public interface IMessageConsumer<in TMessage>
    where TMessage : notnull
{
    ValueTask Handle(
        TMessage message,
        CancellationToken cancellationToken);
}
