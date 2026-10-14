# DOMAIN-008 — Custom source collisions and acknowledgement failures

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Application-defined sources are an extension boundary where colliding IDs or acknowledgement errors can leave facts pending despite committed outgoing state. Applications must be able to reason about that irreversible commit without mistaking callback success for database durability.

## Guarantees

- A postcommit acknowledgement error MUST NOT be interpreted as undoing application changes or outgoing targets already committed.
- Ordinary capture acknowledgement MUST denote local capture commit, not a downstream delivery receipt.

## Contract boundaries

The application owns custom source behavior. This specification does not choose first-source-wins, collision rejection, collection snapshotting, acknowledgement removal validation, callback isolation/retry or error visibility during reconciliation. No durable callback ledger is established by the pending-event interface.

## Acceptance criteria

The original criteria are reserved below pending decisions; none is an unconditional acceptance requirement.

## Open decisions

These original identifiers and heading anchors remain reserved, not normative scenarios. No replacement IDs are assigned before the underlying policy and identity migration are decided.

### DOMAIN-008-AC-01 — Observe duplicate IDs across tracked sources

Reserved — [GAP-DUR-010](../GAPS.md#gap-dur-010). First-source capture and suppression of a colliding source are not an approved conflict policy.

### DOMAIN-008-AC-02 — Expose throwing acknowledgement after commit

Reserved — [GAP-DUR-009](../GAPS.md#gap-dur-009). The original composite couples irreversible committed state with interrupted callbacks/wake. Irreversibility remains a guarantee; the entire ID is reserved pending callback policy.

### DOMAIN-008-AC-03 — Do not infer retry-safe custom acknowledgement

Reserved — [GAP-DUR-010](../GAPS.md#gap-dur-010). Whether the extension requires identity-specific, retry-safe removal or validates a custom acknowledgement remains undecided.

### DOMAIN-008-AC-04 — Preserve observed ambiguity of callback error handling

Reserved — [GAP-DUR-009](../GAPS.md#gap-dur-009). Swallowing a callback error into unknown tracker state is not an approved error-visibility policy.
