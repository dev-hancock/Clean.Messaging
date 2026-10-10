namespace Clean.Messaging.Inbox.Ingress;

public readonly record struct IngressMap(
    Type PayloadType,
    Type MessageType);