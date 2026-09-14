namespace Clean.Messaging.Sagas.EntityFrameworkCore;

internal interface ISagaConflictResolver
{
    Exception? Resolve(
        Exception exception);

    ValueTask<Exception?> ResolveAsync(
        Exception exception,
        CancellationToken cancellationToken);
}
