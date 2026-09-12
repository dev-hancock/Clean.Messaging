using Clean.Messaging.Exceptions;

namespace Clean.Messaging.Failures;

public sealed record MessageFailure(
    FailureCategory Category,
    string Code,
    string ExceptionType,
    string Message,
    DateTime FailedAt,
    int Attempt);

public enum FailureCategory
{
    Transient,
    Permanent,
    Unknown
}

public sealed class MessageFailureFactory
{
    private const int MaxCodeLength = 200;
    private const int MaxTypeLength = 500;
    private const int MaxMessageLength = 500;

    public MessageFailure Create(
        Exception exception,
        int attempt,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is MessageException message
            ? Create(message, attempt, now)
            : CreateUnknown(exception, attempt, now);
    }

    private static MessageFailure Create(
        MessageException exception,
        int attempt,
        DateTimeOffset now)
    {
        return new(
            GetCategory(exception.Action),
            Truncate(exception.Code, MaxCodeLength),
            Truncate(GetTypeName(exception), MaxTypeLength),
            Truncate(exception.Message, MaxMessageLength),
            now.UtcDateTime,
            attempt);
    }

    private static MessageFailure CreateUnknown(
        Exception exception,
        int attempt,
        DateTimeOffset now)
    {
        return new(
            FailureCategory.Unknown,
            ErrorCodes.Processing.MessageUnhandled,
            Truncate(GetTypeName(exception), MaxTypeLength),
            "An unexpected message processing failure occurred.",
            now.UtcDateTime,
            attempt);
    }

    private static FailureCategory GetCategory(FailureAction action)
    {
        if (action == FailureAction.Retry)
        {
            return FailureCategory.Transient;
        }

        if (action == FailureAction.Fault)
        {
            return FailureCategory.Permanent;
        }

        return FailureCategory.Unknown;
    }

    private static string GetTypeName(Exception exception)
    {
        return exception.GetType().FullName ?? exception.GetType().Name;
    }

    private static string Truncate(string value, int length)
    {
        return value.Length <= length ? value : value[..length];
    }
}