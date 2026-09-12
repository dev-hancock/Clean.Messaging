using Clean.Messaging.Abstractions;

namespace Clean.Messaging;

/// <summary>Ambient metadata for the current message delivery.</summary>
public sealed class MessageContext : IMessageContext
{
    private readonly AsyncLocal<State?> _current = new();

    public Guid? MessageId => _current.Value?.MessageId;

    public Guid? CorrelationId => _current.Value?.CorrelationId;

    public Guid? CausationId => _current.Value?.CausationId;

    public string? ConsumerId => _current.Value?.ConsumerId;

    public IDisposable Push(
        Guid messageId,
        Guid? correlationId,
        Guid? causationId = null,
        string? consumerId = null)
    {
        var previous = _current.Value;

        _current.Value = new(
            messageId,
            correlationId,
            causationId,
            consumerId);

        return new ContextScope(this, previous);
    }

    private sealed record State(
        Guid MessageId,
        Guid? CorrelationId,
        Guid? CausationId,
        string? ConsumerId);

    private sealed class ContextScope(
        MessageContext context,
        State? previous) : IDisposable
    {
        private MessageContext? _context = context;

        public void Dispose()
        {
            var current = Interlocked.Exchange(ref _context, null);

            if (current is not null)
            {
                current._current.Value = previous;
            }
        }
    }
}