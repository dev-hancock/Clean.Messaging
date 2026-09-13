namespace Clean.Messaging.Scheduling;

public interface ISchedulingEngine
{
    void Wake();

    ValueTask Wait(
        DateTime wakeAt,
        CancellationToken cancellationToken);
}
