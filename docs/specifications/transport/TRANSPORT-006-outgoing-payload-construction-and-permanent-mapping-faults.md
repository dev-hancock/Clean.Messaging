# TRANSPORT-006 — Outgoing payload construction and permanent mapping faults

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Outbox implementers need stored consumer messages transformed into registered transport payloads with useful failures rather than invalid publishable results.

Authority: Public IOutboxDispatch/DispatchMap, public OutboxDispatchException code/action and separate serializer contracts; coding standards protect representation, diagnostics and extension compatibility. Blanket cancellation faults and empty metadata need separate decisions. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Successful construction MUST serialize the mapped payload under its registered outgoing contract and preserve stored message/event/context IDs.
- Invalid stored data, incompatible mappings and missing payload contracts MUST NOT yield successful construction. Ordinary dispatch mapping faults MUST preserve the public permanent outbox.dispatch.failed contract and diagnostic cause.
- Null envelopes MUST fail argument validation; absent mappings MUST raise permanent OutboxDispatchException. Construction MUST NOT publish or settle targets.
- Durable and transport serializers MUST retain separate configuration authority rather than silently share options.

## Contract boundaries

Source and payload contracts must be compatible and resolvable. Construction is not a network result or durable completion. Built-in transport serialization uses UTF-8 web-default JSON unless replaced. Synchronous cancellation and metadata propagation remain unresolved; no schema negotiation is promised.

## Acceptance criteria

### TRANSPORT-006-AC-03 — Null envelope and missing dispatch map are distinct errors

- **Given** a null outgoing envelope or an absent exact dispatch pair.
- **When** construction is requested.
- **Then** null fails argument validation and the missing pair raises permanent OutboxDispatchException without publication/completion.

### TRANSPORT-006-AC-04 — Durable JSON configuration is independent of transport JSON

- **Given** custom durable JSON options and unchanged default transport serialization.
- **When** stored A is read and mapped B is serialized.
- **Then** durable options govern A and web-default transport JSON governs B; options are not shared or negotiated.

## Open decisions

The original IDs and heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### TRANSPORT-006-AC-01 — Stored source maps to registered outgoing contract

The original ID combines mapped contract/identity with null content type and empty headers. Reserve the composite, not a silently narrowed mapping-only scenario. See [GAP-TX-004](../GAPS.md#gap-tx-004); the recommendation remains unapproved.

### TRANSPORT-006-AC-02 — Invalid persisted or mapped data is permanent failure

The original permanent-fault criterion covers all throwing mappers/serializers. Resolve cancellation before restoring the composite. See [GAP-TX-002](../GAPS.md#gap-tx-002); the recommendation remains unapproved.
