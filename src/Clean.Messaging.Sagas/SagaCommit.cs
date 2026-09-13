namespace Clean.Messaging.Sagas;

internal sealed record SagaCommit(
    long ExpectedVersion,
    string State,
    SagaResult Result);