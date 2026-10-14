# OUTBOX-011 — Transport dispatch and external repeat window

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications stage outgoing work so that a temporary transport failure does not silently lose the intent to deliver it. External recipients need retries to retain the same logical identity, while operators need completion to distinguish a successful publication attempt from failed or interrupted work.

## Guarantees

- External delivery MUST NOT record successful target completion without a successful publication outcome from the selected transport. Transport failure or observed cancellation MUST NOT manufacture completion.
- Repeated delivery of the same durable target MUST preserve delivery/message ID, event ID, target ID, correlation ID and causation ID. A retry is not a new logical operation.
- Completion after publication MUST require current settlement authority. A former owner MUST NOT overwrite another owner's target state.
- Transport failure, caller cancellation and ownership loss MUST retain their distinct meanings for processing policy rather than being converted into success.

The [coding standards](../../CODING_STANDARDS.md) establish stable delivery identity, conditional ownership, cancellation/failure distinctions and the prohibition on exactly-once external-effect claims. These guarantees do not require a particular dispatcher lookup algorithm or storage update sequence.

Coding standards sections 7, 8 and 9 explicitly establish stable retry identity, fenced settlement, distinct failure/cancellation and external repeat uncertainty; transport success is not exactly-once recipient effect.

## Contract boundaries

The outgoing target is durably staged and identifies a configured external transport and destination. The transport defines its successful publication outcome. A successful return is not a library guarantee that the remote application applied the message or that a broker acknowledgement and local completion committed atomically.

External publication and local durable completion do not form one transaction. A process can publish successfully and then fail before recording completion. A transport can also fail or observe cancellation after an external system has accepted work. Either uncertainty can allow repeat publication; failure does not prove that no external effect occurred.

Recipients own duplicate protection appropriate to their effects. Stable identifiers support that protection but do not implement it. Claim loss cannot revoke a publication already made or forcibly stop an uncooperative transport. This specification does not guarantee eventual delivery despite permanent failure, choose a retry budget, or define transport-name comparison and configuration-error timing.

## Acceptance criteria

### OUTBOX-011-AC-01 — Preserve envelope across retries

- **Given** a durable external target was published, but its completion was not durably established before failure or interruption.
- **When** that unresolved target is recovered and delivered again.
- **Then** publication may repeat with the same delivery/message, event and target IDs and correlation/causation metadata; no exactly-once downstream effect is promised.

### OUTBOX-011-AC-02 — Do not complete on transport failure

- **Given** external publication fails or observes cancellation before reporting success.
- **When** the delivery attempt ends.
- **Then** it does not record successful completion for that attempt, and processing policy can distinguish failure from cancellation; an uncertain remote outcome is not treated as proof of successful local completion.

### OUTBOX-011-AC-03 — Fence settlement after successful publication

- **Given** external publication succeeded, but the publishing worker lost settlement authority before recording completion.
- **When** that former owner attempts to complete the durable target.
- **Then** its transition is rejected without overwriting the new owner's state; the publication is not undone and another delivery remains possible.

## Open decisions

Retry-budget exhaustion and abandoned-claim recovery are governed by the unresolved [attempt-budget decision](../GAPS.md#gap-claim-001). No choice in that decision removes the external repeat window or authorizes settlement by a stale owner.
