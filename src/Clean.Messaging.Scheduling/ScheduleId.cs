namespace Clean.Messaging.Scheduling;

public readonly record struct ScheduleId(Guid Value)
{
    public static ScheduleId New()
    {
        return new(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString("N");
    }
}
