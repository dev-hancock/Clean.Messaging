namespace Clean.Messaging.Exceptions;

public class MessageException(
    string code,
    string message,
    FailureAction action,
    Exception? exception = null)
    : Exception(message, exception)
{
    public string Code { get; } = code;

    public FailureAction Action { get; } = action;

    public bool IsTransient => Action == FailureAction.Retry;
}

public enum FailureAction
{
    Retry,
    Fault
}

internal static class ErrorCodes
{
    internal static class Contract
    {
        internal const string TypeNotRegistered = "contract.type.not_registered";

        internal const string IdentifierNotRegistered = "contract.identifier.not_registered";
    }

    internal static class Serialization
    {
        internal const string SerializeFailed = "serialization.serialize.failed";

        internal const string DeserializeFailed = "serialization.deserialize.failed";
    }

    internal static class Processing
    {
        internal const string MessageUnhandled = "processing.message.unhandled";
    }
}