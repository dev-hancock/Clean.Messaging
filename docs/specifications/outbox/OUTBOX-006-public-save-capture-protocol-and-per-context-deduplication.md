# OUTBOX-006 — Public save capture protocol and per-context deduplication

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Capture integrations need to recognize repeated facts within one owning context/transaction without acknowledging them before commit or sharing mutable lifecycle state across unrelated contexts.

## Guarantees

- Repeated capture of the same capture type/event ID within one context and pending owning transaction MUST authorize only the first capture.
- Capture state MUST be independent between contexts and between capture types; acknowledgement MUST remain deferred until known commit, not merely a successful explicit-transaction save.
- Capture, entry tracking and acknowledgement registration MUST require an active valid save; null acknowledgement callbacks, nested saves and ambient transactions MUST fail visibly.
- Public saving/committing indicators and pending-key access MUST reflect the lifecycle phase, not imply successful commit merely because keys are available.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Public OutboxTracker extension compatibility and coding standards sections 6, 8, 9 and 14 establish per-context state ownership, capture identity and truthful save-versus-commit phases.

## Contract boundaries

The tracker is an integration lifecycle contract, not an application unit-of-work abstraction. A capture token records eligibility to stage, not publication success. Per-context/per-capture-type isolation does not decide how multiple acknowledgement callbacks sharing an event ID are identified. Changed pending transaction identity is covered separately by [OUTBOX-007](OUTBOX-007-save-failure-cancellation-rollback-and-retry-tracking.md).

## Acceptance criteria

### OUTBOX-006-AC-01 — Deduplicate captures across saves in one transaction

- **Given** the same capture type and event ID recurs in successful saves on one context within an explicit transaction.
- **When** capture participation is requested before commit.
- **Then** only its first capture is authorized and duplicate capture rows are not added; pending acknowledgements remain deferred until observed commit.

### OUTBOX-006-AC-03 — Reject invalid capture protocol

- **Given** capture, acknowledgement or entry tracking is requested without an active save, a callback is null, a save nests, or an ambient transaction is present.
- **When** the integration participates in capture.
- **Then** invalid lifecycle/input fails instead of silently recording successful capture.

### OUTBOX-006-AC-04 — Expose saving/committing and pending-key boundaries

- **Given** the context is idle, actively saving or in a marked commit phase.
- **When** an integration observes saving/committing status and pending keys.
- **Then** status reflects its actual phase, and pending keys are exposed during the marked commit phase and are absent outside it, rather than their availability establishing successful commit.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### OUTBOX-006-AC-02 — Distinguish contexts and capture types

Reserved pending [GAP-DUR-009](../GAPS.md#gap-dur-009). Per-context and per-capture-type isolation remains established. The original composite additionally deduplicates acknowledgements by event ID alone, so distinct capture types may not each receive a callback; that acknowledgement identity needs approval.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
