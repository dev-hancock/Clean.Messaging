namespace Clean.Messaging.Outbox.Dispatch;

public readonly record struct DispatchMap(
    Type MessageType,
    Type PayloadType);