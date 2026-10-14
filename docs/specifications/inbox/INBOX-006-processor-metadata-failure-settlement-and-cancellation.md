# INBOX-006 — Processor metadata, failure settlement and cancellation

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need each processing attempt to retain its incoming identity and classify retryable failure, permanent failure, host shutdown and lost ownership differently so recovery does not fabricate completion or damage another owner's work.

## Guarantees

- During processing, the persisted delivery, correlation, causation and target identities MUST be exposed as current context, and the previous context MUST be restored afterward.
- Permanent message failure MUST dead-letter immediately. Ordinary transient or unknown failure MUST retry below the configured failure threshold and dead-letter when that threshold is exhausted, retaining diagnosis.
- Host-requested cancellation MUST propagate as shutdown, not become an ordinary failed-message retry or dead letter.
- Ownership loss or failed renewal MUST cancel handling cooperatively and MUST NOT settle as though ownership remained valid.
- Failure settlement MUST have a separate bounded lifetime linked to claim loss, not be automatically abandoned solely because host shutdown began after the message failure.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 7 and 9 for cancellation distinctions, bounded failure handling, ambient identity restoration and conditional settlement; public retry configuration establishes ordinary failure thresholds.

## Contract boundaries

Configured failure policy does not define whether acquisition, queued abandonment or transaction re-execution spends an attempt; see [GAP-CLAIM-001](../GAPS.md#gap-claim-001). Settlement is not handler duration or remote-publication timeout. Cancellation cannot retract effects, and an unsuccessful settlement leaves recovery dependent on retained unresolved work.

## Acceptance criteria

### INBOX-006-AC-01 — Expose and restore delivery metadata

- **Given** an owned entry has persisted metadata and a prior ambient context exists.
- **When** the attempt executes and then exits.
- **Then** handlers see the persisted delivery/correlation/causation and target values, and the prior context is restored after exit.

### INBOX-006-AC-02 — Classify retry and permanent failure

- **Given** a processing failure is permanent, or is transient/unknown below or at the configured failure threshold.
- **When** a still-authorized attempt settles the failure.
- **Then** permanent and exhausted failures dead-letter; a below-threshold retry retains diagnosis and a later due time rather than successful completion, and successful retry/dead-letter settlement clears claim ownership.

### INBOX-006-AC-03 — Keep shutdown separate from message failure

- **Given** the host stopping token causes handling cancellation.
- **When** the attempt exits.
- **Then** shutdown cancellation propagates without recording an ordinary retry or dead-letter solely for that cancellation; retained unresolved work can recover after lease expiry.

### INBOX-006-AC-04 — Stop on claim loss or renewal error

- **Given** renewal is rejected or fails, or settlement authority has been lost.
- **When** processing responds to the ownership loss.
- **Then** handling is cancellation-requested and stale settlement cannot overwrite state or be classified as an ordinary owned failure; retained unresolved work can be reclaimed after expiry.

### INBOX-006-AC-05 — Bound failed settlement independently

- **Given** ordinary handling failed and shutdown begins while valid ownership remains.
- **When** failure settlement runs.
- **Then** settlement has its own configured timeout and claim-loss cancellation rather than immediate host-token cancellation; timeout or failed settlement is not reported as success.

## Open decisions

None.
