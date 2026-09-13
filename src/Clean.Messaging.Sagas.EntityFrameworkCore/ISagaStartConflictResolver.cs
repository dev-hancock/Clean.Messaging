namespace Clean.Messaging.Sagas.EntityFrameworkCore;

internal interface ISagaStartConflictResolver
{
    Exception? Resolve(
        Exception exception);

    ValueTask<Exception?> ResolveAsync(
        Exception exception,
        CancellationToken cancellationToken);
}
