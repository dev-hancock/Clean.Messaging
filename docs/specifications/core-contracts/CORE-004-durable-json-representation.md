# CORE-004 — Durable JSON representation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** MessageSerializer resolves concrete runtime type and uses Web JSON defaults or DI JsonSerializerOptions.

Pre: nonnull registered concrete message. Post: MessageData.Type is its contract and Value is JSON. A configured durable serializer does not configure transport serialization.

## CORE-004-AC-01 — Concrete runtime serialization
**Given** an object held in a base-typed variable whose concrete type is registered.
**When** it is serialized.
**Then** the retained contract and JSON correspond to the concrete registered type.

## CORE-004-AC-02 — Default web JSON
**Given** a registered message with a PascalCase property.
**When** it is serialized using default options.
**Then** its ordinary property name uses web camel-case and compatible deserialization restores its value.

## CORE-004-AC-03 — Configured JSON options
**Given** DI supplies custom JsonSerializerOptions, including a converter or naming policy.
**When** a supported message is serialized and deserialized.
**Then** that durable serializer applies the supplied options rather than independently replacing them.

## CORE-004-AC-04 — Invalid JSON and null result
**Given** a known contract paired with malformed JSON, unsupported JSON conversion, or JSON `null`.
**When** deserialization is requested.
**Then** a Fault MessageException reports `serialization.deserialize.failed` and no message is returned.

## CORE-004-AC-05 — Serialization failure
**Given** a registered object whose JSON serialization throws JsonException or NotSupportedException.
**When** serialization is requested.
**Then** a Fault MessageException reports `serialization.serialize.failed` and preserves the original exception as diagnostic cause.

## CORE-004-AC-06 — Null input
**Given** a null object.
**When** Serialize is called.
**Then** ArgumentNullException is raised before a successful MessageData is produced.
