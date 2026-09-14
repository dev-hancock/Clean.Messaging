namespace Clean.Messaging.Inbox.Mqtt;

internal sealed class RouteFilter
{
    private RouteFilter(
        string value,
        string[] levels)
    {
        Value = value;
        Levels = levels;
    }

    public string Value { get; }

    private string[] Levels { get; }

    public static RouteFilter Parse(
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var levels = value.Split('/');

        for (var index = 0; index < levels.Length; index++)
        {
            var level = levels[index];

            if (level == "#" &&
                index != levels.Length - 1)
            {
                throw new InvalidOperationException(
                    $"MQTT topic filter '{value}' is invalid because '#' must be the last topic level.");
            }

            if (level != "#" &&
                level.Contains('#', StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"MQTT topic filter '{value}' is invalid because '#' must occupy an entire topic level.");
            }

            if (level != "+" &&
                level.Contains('+', StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"MQTT topic filter '{value}' is invalid because '+' must occupy an entire topic level.");
            }
        }

        return new(
            value,
            levels);
    }

    public bool Overlaps(
        RouteFilter other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (IsSystemTopic(Levels[0]) !=
            IsSystemTopic(other.Levels[0]))
        {
            return false;
        }

        var index = 0;

        while (index < Levels.Length &&
            index < other.Levels.Length)
        {
            var first = Levels[index];
            var second = other.Levels[index];

            if (first == "#" ||
                second == "#")
            {
                return true;
            }

            if (first != "+" &&
                second != "+" &&
                !string.Equals(
                    first,
                    second,
                    StringComparison.Ordinal))
            {
                return false;
            }

            index++;
        }

        if (index == Levels.Length &&
            index == other.Levels.Length)
        {
            return true;
        }

        if (index == Levels.Length)
        {
            return other.Levels[index] == "#";
        }

        return Levels[index] == "#";
    }

    private static bool IsSystemTopic(
        string level) =>
        level.StartsWith('$');
}
