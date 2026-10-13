# TRANSPORT-002 — Envelope and payload serialization surface

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** all four `.cs` files and the [project](../../../src/Clean.Messaging.Transport/Clean.Messaging.Transport.csproj) in Transport; especially [serializer](../../../src/Clean.Messaging.Transport/TransportSerializer.cs). Source-only observation; no compatibility certification.

`TransportMessage` is a sealed record with required `Contract` and `Payload`, optional `MessageId`, `EventId`, `CorrelationId`, `CausationId`, `ContentType`, and default empty `Headers`. `TransportHeader(Name, Value)` permits repeated names. Required members impose compile-time initialization, not runtime nonblank/content validation. `Map(source)` is synchronous and receives neither envelope nor cancellation. Payload serializer methods constrain message types to `notnull`.

## TRANSPORT-002-AC-01 — Web JSON payload round trip
**Given** a supported payload type with ordinary public properties and the built-in `TransportSerializers.Json`, **When** the payload is serialized and deserialized as that type, **Then** UTF-8 web-default JSON preserves its values and the serialized bytes contain payload data rather than an added transport envelope.

## TRANSPORT-002-AC-02 — Invalid or null JSON is not a successful payload
**Given** malformed JSON, an unsupported JSON shape/type, or JSON `null` for a nullable reference deserialization result, **When** built-in payload deserialization runs, **Then** it throws rather than returning an accepted default message; a null result produces `InvalidOperationException` before any durable admission.

## TRANSPORT-002-AC-03 — Header multiplicity and absent metadata remain representable
**Given** an envelope with two headers of the same name, omitted optional IDs/content type, and explicitly initialized payload/contract, **When** its members are read, **Then** both headers remain present and optional values remain absent; the record performs no deduplication, contract registration check, or identity generation.
