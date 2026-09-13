namespace Clean.Messaging.Scheduling;

internal interface ISchedulingEngine
{
    void Wake();

    ValueTask Wait(
        DateTime wakeAt,
        CancellationToken cancellationToken);
}
