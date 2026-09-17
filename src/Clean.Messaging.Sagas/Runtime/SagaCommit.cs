namespace Clean.Messaging.Sagas.Runtime;

internal sealed record SagaCommit(
    long ExpectedVersion,
    string State,
    SagaResult Result);