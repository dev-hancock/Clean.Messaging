namespace Clean.Messaging.Processing;

public interface IWorkSignal
{
    ValueTask WaitAsync(
        CancellationToken cancellationToken);

    void Wake();
}
