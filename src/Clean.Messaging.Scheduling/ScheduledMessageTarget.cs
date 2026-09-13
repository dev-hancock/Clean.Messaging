namespace Clean.Messaging.Scheduling;

internal readonly record struct ScheduledMessageTarget // todo: rename to ScheduleTarget?
{
    public static ScheduledMessageTarget Message { get; } = new("message");

    public ScheduledMessageTarget(string value)
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

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}
