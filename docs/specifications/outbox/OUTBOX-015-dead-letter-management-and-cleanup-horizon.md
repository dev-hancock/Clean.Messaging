# OUTBOX-015 — Dead-letter management and cleanup horizon

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need to inspect, requeue or discard failed outgoing targets without erasing their history or resetting successful siblings, and clean old terminal evidence with an explicit duplicate-protection horizon.

## Guarantees

- Inspection MUST list only nondiscarded retained dead letters with a positive bounded limit. Requeue/discard MUST transition only eligible keys and report actual successful transitions.
- Requeue MUST preserve identity, payload and diagnosis while clearing terminal/claim/due state and resetting attempts. Discard MUST retain diagnosis and dead-letter history without pretending success.
- Per-key management MUST remain independently committed, not batch-atomic; ambient mutations and null input MUST fail, while valid empty collections MUST report zero transitions.
- Cleanup MUST delete at most the positive limit of terminal rows strictly older than the cutoff and preserve pending/equality/newer evidence.
- Cleanup failure MUST be reported and permit later periodic retry, without claiming deletion succeeded. Removed unique evidence MUST not be advertised as unlimited duplicate protection.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Public management/cleanup compatibility and coding standards sections 7 and 9 establish truthful per-key transitions, retained diagnosis, bounded retention and no permanent deduplication claim after deletion.

## Contract boundaries

Requested counts include supplied repeated keys; success counts eligible transitions. Successful requeue hints processing but does not prove delivery. Management cannot reset successful rows and supplies no historical replay API. A cleaned event/target can become insertable again; applications own repeat-sensitive recipient protection.

## Acceptance criteria

### OUTBOX-015-AC-01 — Inspect and manage eligible dead letters

- **Given** retained keys include active dead letters, ineligible and repeated keys, a valid empty collection, or inspection is requested with a nonpositive limit.
- **When** inspection, requeue or discard is requested.
- **Then** positive inspection respects its limit and active eligibility while nonpositive inspection fails; only eligible mutations count, Requested includes supplied keys without duplicate extra transitions, successful requeue signals processing and empty collections return zero transitions.

### OUTBOX-015-AC-02 — Preserve diagnosis and partial transitions

- **Given** eligible work has diagnosis and an earlier per-key mutation commits before a later failure, cancellation or ambient request.
- **When** management requeues/discards or rejects the unsupported boundary.
- **Then** requeue resets attempts and terminal/claim/due state while preserving identity/payload/diagnosis; discard retains history, prior committed keys remain changed, and ambient mutation fails.

### OUTBOX-015-AC-03 — Delete only old terminal evidence

- **Given** pending and terminal rows older than, at and newer than the cutoff exist.
- **When** bounded cleanup completes and a cleaned identity is later staged.
- **Then** only up-to-limit strictly old terminal evidence is deleted, pending/equality/newer rows remain, and deleted unique evidence alone does not prevent same event/target restaging.

### OUTBOX-015-AC-04 — Do not hide cleanup failure

- **Given** periodic cleanup fails under a running host or shutdown cancels it.
- **When** the cycle ends.
- **Then** ordinary failure is warned and a later tick can retry, shutdown ends cooperatively and neither failure nor cancellation is claimed successful deletion.

### OUTBOX-015-AC-05 — Reject null key collections and invalid cleanup limit

- **Given** requeue/discard receives null, or cleanup receives a nonpositive limit.
- **When** management or cleanup is requested.
- **Then** invalid input fails instead of becoming a valid empty request or unbounded delete.

## Open decisions

None.
