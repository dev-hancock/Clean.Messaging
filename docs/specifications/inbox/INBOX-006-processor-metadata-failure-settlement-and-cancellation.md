# INBOX-006 — Processor metadata, failure settlement and cancellation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: `InboxProcessor.Process`, shared `ClaimRenewal`, `RetryPolicy`, `MessageFailureFactory`, `MessageContext`; [processing](../../reference/processing.md) describes outcomes.

Contracts: InboxProcessor.Process and IInboxDispatcher.Dispatch bool; handling context contains delivery ID, correlation, causation and target consumer ID and is restored afterward. Failures: permanent MessageException faults immediately; transient/unknown exceptions retry within policy; max attempts is evaluated after a failed claim and is not a hard ceiling on reclaimed crash attempts. Claim-loss and shutdown cancellation are distinct from ordinary failure. Settlement has a timeout linked to claim loss, not the host token.

## INBOX-006-AC-01 — Expose and restore delivery metadata

- **Given** an owned entry and preexisting ambient context.
- **When** the processor dispatches and finishes or throws.
- **Then** the handler sees persisted delivery metadata and target ID; the previous context is restored.

## INBOX-006-AC-02 — Classify retry and permanent failure

- **Given** a target throws transient/unknown failure below the retry threshold or a permanent failure / failure at the threshold.
- **When** the processor settles while it still owns the claim.
- **Then** the former is due later with failure diagnostics; the latter is dead-lettered; successful settlement clears claim ownership.

## INBOX-006-AC-03 — Keep shutdown separate from message failure

- **Given** the host token is cancelled during handling.
- **When** the handler observes cancellation and the processor exits.
- **Then** shutdown cancellation propagates rather than generating an ordinary retry/dead-letter; unsettled work can recover after expiry.

## INBOX-006-AC-04 — Stop on claim loss or renewal error

- **Given** renewal returns false or fails while handling.
- **When** claim-loss cancellation reaches the handler or dispatch returns false.
- **Then** no ordinary failure settlement is authorized by the lost owner; recovery can reclaim unresolved work.

## INBOX-006-AC-05 — Bound failed settlement independently

- **Given** ordinary handling failed, the host later stops, and the owner still has a lease.
- **When** retry/dead-letter settlement runs.
- **Then** settlement may finish using its separate timeout; timeout/ownership loss can leave unresolved leased work, not manufactured completion.
