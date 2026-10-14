# TRANSPORT-004 — Incoming typed admission and metadata loss boundary

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Ingress callers need external payloads to become typed durable consumer work with stable identity, without equating mapping or admission with consumer effects.

Authority: Public typed IInboxIngress and InboxMessage identity contracts plus coding standards on validation, distinct representations and identity. Ignoring declared contract text and dropping arbitrary metadata are not authorized by those obligations. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Successful ingress MUST admit the message produced by its configured typed payload-to-message mapping and preserve valid identities/context; transport and durable contract names may differ.
- Missing or empty delivery identity MUST be rejected without replacement or anonymous admission. Absent event identity MUST use delivery identity as the inbox default; explicitly empty event identity MUST fail inbox validation.
- Null envelopes and absent mappings MUST fail without admission.

## Contract boundaries

The payload serializer, typed mapper and compatible durable contracts/consumers must be configured. Success means awaited admission, not consumer completion. Provider contract validation, generic declaration authority and metadata propagation are distinct boundaries. Mapping owns no transaction or acknowledgement.

## Acceptance criteria

### TRANSPORT-004-AC-01 — Typed mapping admits the mapped consumer message

- **Given** an A-to-B map, compatible A bytes, valid IDs and configured B consumers.
- **When** ingress acceptance succeeds.
- **Then** durable admission contains mapped B and supplied context/identity even if registered A and B contract names differ.

### TRANSPORT-004-AC-03 — Anonymous or empty identity is rejected

- **Given** compatible input with absent/empty delivery ID, or valid delivery ID with absent/empty event ID.
- **When** ingress admission is requested.
- **Then** missing/empty delivery identity raises InboxIngressException without admission or replacement; absent event identity defaults to delivery identity on success, while explicitly empty event identity fails inbox validation.

### TRANSPORT-004-AC-05 — Null envelope or missing map is not accepted

- **Given** a null envelope or an absent exact ingress pair.
- **When** acceptance is requested.
- **Then** null fails argument validation and the missing pair raises InboxIngressException, without admission.

## Open decisions

The original IDs and heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### TRANSPORT-004-AC-02 — Generic ingress trusts the selected pair rather than contract text

Does the selected type pair alone authorize admission when declared contract text disagrees, or must that declaration also be validated? Decision [GAP-INTENT-TX-001](../GAPS.md#gap-intent-tx-001) remains unapproved.

### TRANSPORT-004-AC-04 — Transport-only metadata does not become consumer data

Which headers/content type should survive durable mapping or be available to a mapper? See [GAP-TX-004](../GAPS.md#gap-tx-004); the recommendation remains unapproved.
