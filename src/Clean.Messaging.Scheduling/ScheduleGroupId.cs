namespace Clean.Messaging.Scheduling;

public readonly record struct ScheduleGroupId(Guid Value)
{
    public static ScheduleGroupId New()
    {
        return new(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString("N");
    }
}