# INBOX-002 — Independent atomic admission and duplicate retention

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

An adapter needs accepted work to survive its caller failing or losing the acknowledgement, while equivalent redelivery must not mutate the retained operation or multiply its target obligations.

## Guarantees

- Admission MUST commit its envelope and initial targets atomically and independently of the caller's unsaved state or explicit transaction.
- Admission inside an ambient transaction MUST fail rather than implying supported enlistment.
- An equivalent retry identified by either retained MessageId or EventId MUST leave the existing envelope and targets unchanged. Newly registered consumers MUST NOT be added merely because the same admission was redelivered.
- A concurrent duplicate outcome MUST be treated as accepted only after retained admission is confirmed; an unrelated persistence failure MUST remain a failure.
- Retained admission MUST remain recognizable after restart and lost transport acknowledgement.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 8, 9 and 14 establish transaction-boundary transparency and stable duplicate identity; independent admission and unsupported ambient enlistment are deliberate public integration boundaries.

## Contract boundaries

Independence concerns the configured relational admission save, not a distributed broker acknowledgement. A failed response can leave commit uncertain; callers must retry with stable identities. Duplicate memory requires retained admissions. Reuse of identity for conflicting content is not resolved by this contract.

## Acceptance criteria

### INBOX-002-AC-01 — Do not join caller changes

- **Given** the caller has unsaved changes or an explicit transaction in its own context.
- **When** valid Inbox admission succeeds and the caller later rolls back.
- **Then** the admitted envelope and targets remain durable while the caller's unrelated changes were not committed by admission.

### INBOX-002-AC-02 — Reject ambient admission

- **Given** an ambient transaction is active.
- **When** admission is requested.
- **Then** the unsupported boundary is rejected and admission is not presented as successfully enlisted.

### INBOX-002-AC-04 — Resolve concurrent duplicate admission

- **Given** independent callers race to admit the same stable operation, or a save fails for a reason unrelated to duplicate admission.
- **When** admission resolves the persistence outcome.
- **Then** a duplicate result is accepted only with confirmed retained identity, and an unconfirmed unrelated failure propagates.

### INBOX-002-AC-05 — Recover after admission and lost acknowledgement

- **Given** admission committed but its acknowledgement was lost before a restart.
- **When** the adapter repeats the equivalent envelope with the same delivery or event identity.
- **Then** the retained admission is recognized without replacing its envelope or multiplying initial targets.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### INBOX-002-AC-03 — Suppress duplicates by either retained identity

Reserved pending [GAP-DUR-007](../GAPS.md#gap-dur-007). The unresolved obligation requires first-write retention even when a repeated identity carries changed payload or newly registered consumers. Equivalent redelivery recognition and no incidental expansion of initial targets remain established; conflicting content needs an explicit immutable-identity policy.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
