# OUTBOX-013 — Owned durable state, processor policy and cancellation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: OutboxEntry, OutboxProcessor.Process, inherited DurableStore, ClaimRenewal, retry/failure factories. [Processing](../../reference/processing.md) provides descriptive defaults/metrics.

Contracts: IOutboxStore inherited Claim/Renew/Complete/Retry/DeadLetter/Cleanup; entries carry common identity/payload/context/lifecycle/diagnostics plus Transport/Destination; Key is MessageId/TargetId. Claim positive limit/lease, unresolved due <=now and lease absent/expired <=now; Attempts increments on claim. Settlement requires exact key/token, unresolved state, no NextAttemptAt and ClaimedUntil > now. Processor pushes/restores metadata, renews during processing, links handling to host and claim loss; failed settlement timeout linked to claim loss only. Retry max attempts is failure-policy threshold, not hard maximum crash claims.

## OUTBOX-013-AC-01 — Claim and fence target state

- **Given** due/future/terminal/actively leased/expired entries and nonpositive claim inputs.
- **When** Claim or stale-owner settlement runs.
- **Then** only eligible bounded work is owned with increased Attempts; invalid limits/lease fail and stale/expired Renew/Complete/Retry/DeadLetter return false.

## OUTBOX-013-AC-02 — Keep target completion independent

- **Given** several targets share an event and one succeeds while another fails.
- **When** processors settle valid owned rows.
- **Then** successful target alone completes; failed target retries or dead-letters according to transient/permanent/unknown classification with preserved identity/payload/diagnostics.

## OUTBOX-013-AC-03 — Expose and restore ambient metadata

- **Given** owned stored delivery and prior ambient context.
- **When** Process completes or fails.
- **Then** delivery MessageId/correlation/causation/target are exposed during dispatch and previous context restored afterward.

## OUTBOX-013-AC-04 — Separate shutdown and ownership cancellation

- **Given** handling is interrupted by host cancellation or claim renewal false/error.
- **When** Process handles the interruption.
- **Then** host cancellation propagates without ordinary failure settlement; claim-loss stops authorizing settlement and leaves unresolved work recoverable.

## OUTBOX-013-AC-05 — Do not invent completion on settlement infrastructure failure

- **Given** handling failed but retry/dead-letter persistence times out or fails.
- **When** the processor/work loop handles the failure.
- **Then** the error is observable/logged and unresolved work recovers after expiry; separate settlement timeout may run after host stop.
