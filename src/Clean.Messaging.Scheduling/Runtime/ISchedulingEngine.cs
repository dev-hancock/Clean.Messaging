namespace Clean.Messaging.Scheduling.Runtime;

public interface ISchedulingEngine
{
    void Wake();

    ValueTask Wait(
        DateTime wakeAt,
        CancellationToken cancellationToken);
}