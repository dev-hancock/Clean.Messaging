using Clean.Messaging.Exceptions;

namespace Clean.Messaging.Scheduling;

internal sealed record ScheduledMessageFailure(
    string Code,
    string Message,
    string ExceptionType)
{
    public static ScheduledMessageFailure From(
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var code = exception is MessageException message
            ? message.Code
            : "scheduling.unhandled";

        return new(
            Truncate(code, 200),
            Truncate(exception.Message, 500),
            Truncate(
                exception.GetType().FullName ?? exception.GetType().Name,
                500));
    }

    private static string Truncate(
        string value,
        int length)
    {
        return value.Length <= length
            ? value
            : value[..length];
    }
}
