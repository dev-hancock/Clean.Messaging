# DOMAIN-007 — Invalid custom facts and targetless acknowledgement

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Custom sources must not turn invalid facts into successful durable capture. Applications also need a clear distinction between committing target work, committing a targetless save and merely failing to observe a commit.

## Guarantees

- Capture of an invalid ID, invalid payload or unresolved durable contract/required consumer MUST fail rather than report successful capture; its source facts MUST remain pending.
- Failed commit notification without durable capture evidence MUST NOT be treated as proof of successful capture or justify an invented acknowledgement.

## Contract boundaries

Normal durable fan-out requires a nonempty ID, serializable nonnull concrete payload, registered contract and applicable targets. Validation of the standalone RaisedEvent constructor is not selected here. Whether a valid custom targetless batch should fail, stay pending or be acknowledged is undecided; no downstream delivery follows from an empty batch.

## Acceptance criteria

### DOMAIN-007-AC-03 — Keep targetless failed commit uncertain

- **Given** a targetless captured fact has no outgoing keys and its commit notification fails.
- **When** capture reconciliation attempts to establish the outcome.
- **Then** outgoing-key verification cannot prove capture committed; acknowledgement remains dependent on observed or verified success rather than assumed success.

## Open decisions

These original identifiers and heading anchors remain reserved, not normative scenarios. No replacement IDs are assigned before the underlying policy and identity migration are decided.

### DOMAIN-007-AC-01 — Fail invalid custom fact capture

Reserved — [GAP-DUR-010](../GAPS.md#gap-dur-010). The original composite includes invalid-fact rejection and the absence of constructor validation. Rejection is retained in Guarantees; the entire ID is reserved rather than silently dropping the constructor clause.

### DOMAIN-007-AC-02 — Observe targetless successful acknowledgement

Reserved — [GAP-DUR-011](../GAPS.md#gap-dur-011). Acknowledgement of a valid zero-target capture is undecided. The original no-row acknowledgement assertion remains reserved, not a delivery guarantee.

- [GAP-DUR-008](../GAPS.md#gap-dur-008): the related policy remains undecided; its recommendation is not approved.
