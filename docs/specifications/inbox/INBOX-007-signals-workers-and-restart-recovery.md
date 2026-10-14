# INBOX-007 — Signals, workers and restart recovery

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Durable incoming work must remain recoverable after a missed in-memory wake or a worker crash, while host load and infrastructure retry pressure remain bounded.

## Guarantees

- Wake signals MUST be coalescible and waits MUST honor cancellation without interpreting a wake count as durable work count.
- Startup and periodic recovery MUST permit rediscovery of eligible retained unresolved work without a surviving admission hint.
- Queued work and concurrent processing MUST remain within configured capacity/concurrency bounds, with lifetime-compatible scoped claiming and processing.
- Claim or processing infrastructure failure MUST be reported and permit later recovery; repeated claim failures MUST use bounded backoff rather than permanently stop recovery.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 6, 7 and 9 for scoped processing, bounded resources, restart recovery and truthful failure handling; the wake signal is a latency hint, not durable responsibility.

## Contract boundaries

Recovery is conditional on host progress, storage availability and eligible ownership; it promises neither immediate delivery nor FIFO completion. Claims can expire while queued. Attempt-budget effects of crash or queue abandonment are not selected here.

## Acceptance criteria

### INBOX-007-AC-01 — Coalesce wake hints and cancel a wait

- **Given** wake hints arrive more rapidly than a waiter consumes them, or a wait is pending during cancellation.
- **When** the signal is consumed or its wait is cancelled.
- **Then** hints can coalesce rather than form a work ledger, and the cancelled wait terminates without consuming durable responsibility.

### INBOX-007-AC-03 — Recover after claim or processing infrastructure failures

- **Given** claim storage or processing infrastructure fails while retained unresolved work and a running host remain.
- **When** workers continue their recovery cycle.
- **Then** the failure is logged, repeated claim recovery delays are bounded by configuration, and later eligible work remains processable within the configured queue/concurrency and scoped-lifetime limits.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### INBOX-007-AC-02 — Recover after crash without a hint

Reserved pending [GAP-CLAIM-001](../GAPS.md#gap-claim-001). Restart recovery remains required, but the original criterion additionally spends attempts on crashes that may never invoke a handler. Decide acquisition accounting and exhaustion before treating abandoned queued work as budget consumption.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
