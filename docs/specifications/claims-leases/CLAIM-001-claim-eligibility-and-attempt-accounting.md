# CLAIM-001 — Recoverable ownership of durable work

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Workers need temporary authority to process unfinished durable targets without reviving completed work or allowing an obsolete owner to settle it. Operators need abandoned work to remain recoverable, and callers need acquisition to respect their processing capacity.

## Guarantees

- Acquiring work MUST NOT reopen a completed, dead-lettered or discarded target. Recovery and explicit management transitions are distinct operations.
- A successful acquisition MUST establish durable, time-limited ownership of each returned target. Ownership authorizes conditional settlement; acquiring work is not evidence that its handler ran or that delivery completed.
- Acquisition MUST respect the caller's positive batch limit. It MUST NOT return more owned targets than requested; it need not fill the batch.
- Ownership and recovery MUST preserve logical message and target identity. A lease does not grant authority to overwrite another owner's settlement or make external effects atomic.

Attempt accounting, budget exhaustion and exact deadline equality are separate policy choices. They do not follow from acquiring ownership or from a particular storage algorithm.

## Contract boundaries

The caller supplies a valid positive limit and lease and a consistent time basis. Acquisition of durable targets is distinct from handler invocation, scheduling fairness and forced worker termination. The batch limit bounds returned ownership, not the number of handler invocations or external effects.

Recovery does not promise unlimited reacquisition or select an acquisition cap. The relationship between abandoned work and retry exhaustion remains an open decision. A stale worker can continue executing or perform an external effect; only current ownership may authorize a durable transition.

Provider concurrency and uncertain acquisition outcomes remain subject to [GAP-CORE-005](../GAPS.md#gap-core-005). The [processing reference](../../reference/processing.md) describes current acquisition and retry mechanics without approving an accounting policy.

## Acceptance criteria

### CLAIM-001-AC-02 — Terminal exclusion

- **Given** completed, dead-lettered and discarded targets coexist with unfinished work.
- **When** a worker acquires a batch of work.
- **Then** none of those terminal targets is acquired, even if its previous due time or lease would otherwise make it available.

## Open decisions

The criteria below retain their original identities and meaning pending decisions on eligibility and attempt accounting. Their linked decisions preserve the original assertions, alternatives and consequences. None is reassigned or approved by this document.

### CLAIM-001-AC-01 — Eligible boundaries

Exact due-time and lease-expiry equality, together with exclusion of future-due and still-owned work: [deadline and eligibility decision](../GAPS.md#gap-claim-002).

### CLAIM-001-AC-03 — Claim count

The original criterion combines bounded durable ownership with one attempt increment per acquisition. The first obligation is retained in Guarantees; the accounting clause is undecided. The composite ID is reserved rather than silently narrowed: [attempt and criterion-splitting decision](../GAPS.md#gap-claim-001).

### CLAIM-001-AC-04 — Abandoned claim

Whether an acquisition abandoned before handler invocation spends an attempt: [attempt-budget decision](../GAPS.md#gap-claim-001).

### CLAIM-001-AC-05 — No absolute claim cap

Whether budget exhaustion may coexist with further acquisition after abandonment: [attempt-budget decision](../GAPS.md#gap-claim-001).

No replacement acceptance-criteria IDs are assigned before approval.
