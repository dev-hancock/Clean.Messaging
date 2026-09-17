namespace Clean.Messaging.Scheduling;

public readonly record struct ScheduleTarget
{
    public ScheduleTarget(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > 100 ||
            value.Any(character => character > 127))
        {
            throw new ArgumentException(
                "Scheduled message targets must be at most 100 ASCII characters.",
                nameof(value));
        }

        Value = value;
    }

    public static ScheduleTarget Message { get; } = new("message");

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}