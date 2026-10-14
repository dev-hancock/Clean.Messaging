# OUTBOX-013 — Owned durable state, processor policy and cancellation

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Outgoing target recovery needs temporary ownership and independent progress so a failing destination cannot erase a successful sibling or classify shutdown and stale ownership as ordinary message failure.

## Guarantees

- Acquisition MUST be bounded, exclude terminal/future-due/still-owned work and preserve the target envelope; nonpositive limits or leases MUST fail. Stale-owner settlement MUST not overwrite current target state.
- Completion MUST remain per target. Permanent failure MUST dead-letter immediately, transient/unknown failure MUST retry below the configured failure threshold and dead-letter at exhaustion, without resetting completed siblings.
- Processing MUST expose persisted delivery, correlation, causation and target metadata and restore the previous ambient context on exit.
- Shutdown cancellation MUST not become ordinary message failure, and renewal/ownership loss MUST cancel handling without granting stale settlement authority.
- Ordinary failure settlement MUST use a separate bounded lifetime linked to claim loss. Infrastructure failure MUST not manufacture completed, retried or dead-lettered success.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 7 and 9 establish per-target progress, stable retained metadata, stale-owner fencing and bounded distinct failure settlement; retry configuration governs ordinary failure thresholds.

## Contract boundaries

Acquisition/abandonment accounting and exact expiry equality are unresolved. Settlement timeout is not publication timeout or handler duration. Claim fencing cannot retract an external effect, and lease-expiry recovery depends on retained unresolved work and running infrastructure. FIFO completion and universally eventual delivery are not promised.

## Acceptance criteria

### OUTBOX-013-AC-02 — Keep target completion independent

- **Given** one logical event has a completed target and another still-authorized target fails permanently or at/below the configured failure threshold.
- **When** failure policy settles the unresolved target.
- **Then** the completed target stays complete; permanent/exhausted failures dead-letter and below-threshold transient/unknown failures retry with identities/payload/diagnosis preserved.

### OUTBOX-013-AC-03 — Expose and restore ambient metadata

- **Given** a retained target has persisted metadata and a previous ambient context exists.
- **When** the outgoing attempt executes and exits.
- **Then** delivery/correlation/causation and target values are exposed during processing, including the target in the consumer-context slot, and the previous context is restored.

### OUTBOX-013-AC-04 — Separate shutdown and ownership cancellation

- **Given** host shutdown cancels handling, or renewal fails/ownership is lost.
- **When** the attempt exits or responds to claim loss.
- **Then** shutdown cancellation propagates rather than generating ordinary retry/dead-letter, and claim loss cancellation does not permit stale settlement or undo an already-issued external effect; retained unresolved work remains recoverable after expiry.

### OUTBOX-013-AC-05 — Do not invent completion on settlement infrastructure failure

- **Given** handling failed but failure settlement times out, throws or is ownership-rejected.
- **When** the bounded separate settlement ends.
- **Then** the settlement failure remains observable or logged, claim-loss-linked settlement may run after host shutdown within its own bound, and it does not claim a transition that was not durably established; unresolved work remains eligible for later lease recovery.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### OUTBOX-013-AC-01 — Claim and fence target state

Reserved pending [GAP-CLAIM-001](../GAPS.md#gap-claim-001), [GAP-CLAIM-002](../GAPS.md#gap-claim-002). The original composite includes attempt increments and exact due/expiry boundaries alongside supported bounded acquisition, invalid-input rejection and stale-owner fencing. Accounting and temporal policy need approval; supported protections remain above.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
