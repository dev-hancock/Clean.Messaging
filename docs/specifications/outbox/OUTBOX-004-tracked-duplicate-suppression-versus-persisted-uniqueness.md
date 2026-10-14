# OUTBOX-004 — Tracked duplicate suppression versus persisted uniqueness

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications need retained event/target identity to prevent duplicate obligations while understanding that a staged duplicate guard is not an unconditional idempotent database enqueue or a content-conflict resolver.

## Guarantees

- Retained persisted work MUST enforce event/target uniqueness without replacing the original durable envelope as an incidental duplicate effect.
- Staging MUST NOT be represented as unconditional database idempotency: a duplicate retained but untracked pair can fail the owning save, and that failure MUST remain visible.
- A deleted tracked pair MUST NOT suppress a legitimate replacement solely as active staged work. Deleted retained uniqueness evidence MUST NOT be claimed to protect an identity indefinitely.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 9 and 14 establish per-target persisted uniqueness and retention-limited duplicate memory; public writer/owning-save boundaries deliberately do not promise lookup-backed idempotent enqueue.

## Contract boundaries

The owning save, retained records and tracked staging state have distinct boundaries. Equivalent repeated work and conflicting identity/content reuse need an explicit policy. Removal of the only unique pair record can permit a later same-identity insertion, not prove its recipient effect is new or harmless.

## Acceptance criteria

### OUTBOX-004-AC-02 — Expose duplicate persisted save failure

- **Given** the same event/target is already persisted but not tracked as active staged work in a fresh owning context.
- **When** another enqueue is staged and saved.
- **Then** staging does not guarantee silent database idempotency, and a persisted uniqueness failure at save remains visible rather than authorizing duplicate work.

### OUTBOX-004-AC-03 — Allow deleted tracked replacements and cleaned identities

- **Given** a tracked pair is marked deleted, or its prior persisted row has been cleaned away.
- **When** the same event/target is staged again.
- **Then** the deleted tracked pair does not suppress replacement, and removed uniqueness evidence alone no longer blocks reinsertion or protects against repeated external effects.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### OUTBOX-004-AC-01 — Suppress only nondeleted tracked target pairs

Reserved pending [GAP-DUR-007](../GAPS.md#gap-dur-007). The original composite retains conflicting nondeleted tracked content while adding missing targets, without equivalence comparison. Stable per-target uniqueness remains required; identity conflict handling needs approval rather than narrowing this ID to duplicate suppression alone.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
