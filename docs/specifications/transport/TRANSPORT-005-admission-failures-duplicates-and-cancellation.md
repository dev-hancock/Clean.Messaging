# TRANSPORT-005 — Admission failures, duplicates and cancellation

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Ingress operators need retained retries to recognize work, diagnostic failures and cancellation without manufactured success. Caller failure must not erase an independently committed admission.

Authority: Coding standards explicitly require failure distinctions, cooperative cancellation, retained duplicate protection and transaction boundaries. Independent admission is the persistence integration boundary, not an inferred join to caller transactions. Blanket synchronous wrapping and identity-content conflict rules lack approval. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Mapping and admission failures MUST remain distinguishable with diagnostic causes; neither MUST become successful admission.
- Concurrent legitimate repeats with the same identity MUST leave one retained admission/consumer-obligation set without resetting retained completed targets.
- Admission I/O MUST receive the attempt token and observed I/O cancellation MUST remain cancellation without invented success.
- Independent relational admission MUST reject ambient participation and remain independent of unrelated caller rollback after admission committed.

## Contract boundaries

Duplicate protection requires correct relational mappings, distinct-operation identity uniqueness and retained evidence; this is not provider certification. Failed/cancelled calls may have uncertain commit outcomes, not proven rollback. Generic ingress supplies no acknowledgement or separate duplicate store.

## Acceptance criteria

### TRANSPORT-005-AC-02 — Concurrent retained duplicate admissions create one obligation set

- **Given** identical legitimate envelopes and correct relational storage with retained consumer work.
- **When** concurrent admissions of that identity succeed.
- **Then** one admission and one event/consumer obligation remain without resetting completed targets.

### TRANSPORT-005-AC-04 — Admission I/O cancellation remains cancellation

- **Given** mapping succeeded and admission is blocked on cancellable I/O.
- **When** the supplied attempt token is cancelled.
- **Then** I/O cancellation propagates without ordinary ingress wrapping or invented success; committed work is not undone.

### TRANSPORT-005-AC-06 — Independent admission rejects ambient transactions

- **Given** valid input and independently composed relational admission.
- **When** admission occurs inside an ambient transaction, or unrelated caller work fails after separate admission committed.
- **Then** ambient participation is rejected and a separately committed admission is not erased merely by caller failure.

## Open decisions

The original IDs and heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### TRANSPORT-005-AC-01 — Mapping and persistence failures remain distinguishable

The original exception-wrapping criterion covers all serializer/mapper exceptions, including cancellation. Resolve that classification before restoring this composite ID. See [GAP-TX-002](../GAPS.md#gap-tx-002); the recommendation remains unapproved.

### TRANSPORT-005-AC-03 — Identity conflict does not overwrite retained data

How should reused identity with different content be treated? Coordinate this incoming case with the retained-content conflict decision rather than approving acceptance without comparison. See [GAP-DUR-007](../GAPS.md#gap-dur-007); the recommendation remains unapproved.

### TRANSPORT-005-AC-05 — Synchronous mapper cancellation currently becomes a mapping fault

How should synchronous cancellation differ from attempt shutdown and ordinary mapping failure? See [GAP-TX-002](../GAPS.md#gap-tx-002); the recommendation remains unapproved.
