# TRANSPORT-006 — Outgoing payload construction and permanent mapping faults

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** [IOutboxDispatch](../../../src/Clean.Messaging.Outbox.Dispatch/IOutboxDispatch.cs), [DispatchMap](../../../src/Clean.Messaging.Outbox.Dispatch/DispatchMap.cs), [TransportDispatch](../../../src/Clean.Messaging.Outbox.Dispatch/TransportDispatch.cs), [factory](../../../src/Clean.Messaging.Outbox.Dispatch/TransportMessageFactory.cs), [exception](../../../src/Clean.Messaging.Outbox.Dispatch/OutboxDispatchException.cs), runtime [serializer](../../../src/Clean.Messaging/Serialization/MessageSerializer.cs) and [registry](../../../src/Clean.Messaging/Contracts/MessageContractRegistry.cs).

## TRANSPORT-006-AC-01 — Stored source maps to registered outgoing contract
**Given** persisted A data, an A-to-B dispatch map and both source and B contracts registered, **When** `Create` succeeds, **Then** its payload is serialized mapped B data, `Contract` is B's registered name, stored IDs/context are copied, and Content Type is null with empty headers in the built-in factory.

## TRANSPORT-006-AC-02 — Invalid persisted or mapped data is permanent failure
**Given** unknown stored contract, invalid stored JSON, an incompatible deserialized source, a throwing mapper, missing payload contract or failing transport serialization, **When** dispatch creates a transport message, **Then** it throws `OutboxDispatchException` with code `outbox.dispatch.failed`, action `Fault` and preserved diagnostic cause where wrapping occurred; no publishable successful result is returned.

## TRANSPORT-006-AC-03 — Null envelope and missing dispatch map are distinct errors
**Given** a null envelope or absent exact dispatch map, **When** `Create` is called, **Then** the null envelope fails argument validation while the missing map reports permanent `OutboxDispatchException`; neither publishes nor completes work.

## TRANSPORT-006-AC-04 — Durable JSON configuration is independent of transport JSON
**Given** custom durable JSON options and the unchanged default transport serializer, **When** stored A is deserialized and mapped B is serialized, **Then** durable options govern reading A while web-default transport JSON governs B. No automatic shared-options negotiation is performed.
