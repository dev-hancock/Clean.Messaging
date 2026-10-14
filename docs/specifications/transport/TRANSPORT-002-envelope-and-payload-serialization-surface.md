# TRANSPORT-002 — Envelope and payload serialization surface

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Transport implementers need an envelope for payload and metadata, and an interoperable payload serializer without a hidden envelope.

Authority: Deliberate public TransportMessage/TransportHeader representation and advertised TransportSerializers.Json format, protected by coding standards on serialized shapes and compatibility. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- The public envelope MUST retain explicitly supplied payload, contract and header-list values, including repeated header names and absent optional metadata. Construction MUST NOT invent identity.
- The built-in JSON serializer MUST encode supported payload values as UTF-8 web-default JSON and read compatible payloads without adding an envelope.
- Malformed, incompatible or null JSON payload results MUST fail rather than become accepted default messages.

## Contract boundaries

This covers supported built-in JSON payloads, not arbitrary serializers or schema evolution. Required members are initialization obligations, not runtime identity validation. The synchronous payload mapper receives a value, not an envelope. Optional metadata remains representable even where a later boundary rejects it.

## Acceptance criteria

### TRANSPORT-002-AC-01 — Web JSON payload round trip

- **Given** a supported payload and the built-in JSON serializer.
- **When** the value is serialized and deserialized as the same compatible type.
- **Then** UTF-8 web-default JSON preserves its values and contains payload rather than an added envelope.

### TRANSPORT-002-AC-02 — Invalid or null JSON is not a successful payload

- **Given** malformed JSON, an incompatible shape or a JSON null result.
- **When** built-in deserialization is requested.
- **Then** it throws rather than accepting a default message; null results raise InvalidOperationException.

### TRANSPORT-002-AC-03 — Header multiplicity and absent metadata remain representable

- **Given** an envelope with repeated same-name headers and absent optional IDs/content type.
- **When** its initialized values are read.
- **Then** both headers and absent values remain unchanged without deduplication, registration checks or generated identity.

## Open decisions

None.
