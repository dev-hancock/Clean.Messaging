# CORE-004 — Durable JSON representation

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Durable storage needs a consistent named JSON representation that restores supported messages and reports unusable data as failure, not a successful empty message.

## Guarantees

- Serialization MUST use the registered concrete runtime type and retain its contract name and represented JSON values.
- The default durable JSON format MUST use web property naming; explicitly supplied JsonSerializerOptions MUST govern supported durable serialization/deserialization instead of being replaced by defaults.
- Malformed/unsupported JSON or JSON null for a known contract MUST report permanent serialization.deserialize.failed without returning a message.
- JsonException/NotSupportedException during serialization MUST report permanent serialization.serialize.failed and preserve the original cause; null Serialize input MUST raise ArgumentNullException.

## Contract boundaries

The host registers concrete message types and configures compatible serializer options before use. Durable JSON and transport payload serialization are separate. Converter/handler failures outside the stated JSON exceptions are not universally wrapped. This is not schema negotiation, live reload or a guarantee for default MessageData with null fields.

## Acceptance criteria

### CORE-004-AC-01 — Concrete runtime serialization

- **Given** a base-typed variable contains a concrete message whose concrete type is registered.
- **When** the durable serializer writes that value.
- **Then** the retained contract identifies the concrete type and its JSON preserves the represented concrete message values.

### CORE-004-AC-02 — Default web JSON

- **Given** a registered message with a PascalCase property.
- **When** it is serialized using default options.
- **Then** its ordinary property name uses web camel-case and compatible deserialization restores its value.

### CORE-004-AC-03 — Configured JSON options

- **Given** the host supplies compatible JsonSerializerOptions through DI, with a converter or naming policy, before serializer use.
- **When** a supported message is durably serialized and deserialized.
- **Then** its representation and restored values use those configured options rather than independently substituted defaults.

### CORE-004-AC-04 — Invalid JSON and null result

- **Given** a known contract paired with malformed JSON, unsupported JSON conversion, or JSON `null`.
- **When** deserialization is requested.
- **Then** a Fault MessageException reports `serialization.deserialize.failed` and no message is returned.

### CORE-004-AC-05 — Serialization failure

- **Given** a registered object whose JSON serialization throws JsonException or NotSupportedException.
- **When** serialization is requested.
- **Then** a Fault MessageException reports `serialization.serialize.failed` and preserves the original exception as diagnostic cause.

### CORE-004-AC-06 — Null input

- **Given** a null object.
- **When** Serialize is called.
- **Then** ArgumentNullException is raised before a successful MessageData is produced.

## Open decisions

[GAP-CORE-008](../GAPS.md#gap-core-008) covers malformed public records; [GAP-CORE-009](../GAPS.md#gap-core-009) covers serializer/options reconfiguration.
