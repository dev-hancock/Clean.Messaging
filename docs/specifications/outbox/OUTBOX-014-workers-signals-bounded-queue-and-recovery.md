# OUTBOX-014 — Workers, signals, bounded queue and recovery

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Outgoing work must survive missed hints and abandoned workers while bounded queueing and scoped processing keep durable recovery from exhausting host resources.

## Guarantees

- Startup and periodic recovery MUST allow eligible retained outgoing work to be rediscovered without the original enqueue/commit signal.
- Wake hints MUST be coalescible and pending waits MUST honor cancellation.
- Queue capacity and concurrent execution MUST remain bounded by configuration with operation-scoped claiming and processing.
- Claim/processing infrastructure failures MUST be reported without inventing completion, and later recovery MUST remain possible with bounded claim retry backoff.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 6, 7 and 9 establish bounded resource/lifetime ownership, restart recovery and truthful infrastructure failure; hints are not the durable work ledger.

## Contract boundaries

Recovery depends on host progress, eligible ownership and available persistence; it is not FIFO completion, immediate wake delivery or success despite permanent destination failure. Queued claims can expire before handling. No abandoned-acquisition budget is selected here.

## Acceptance criteria

### OUTBOX-014-AC-02 — Coalesce signals and cancel waits

- **Given** multiple commit/recovery hints accumulate, or a signal wait is pending during host cancellation.
- **When** the waiter resumes or is cancelled.
- **Then** hints coalesce without becoming a counted durable ledger and cancellation terminates the wait.

### OUTBOX-014-AC-03 — Continue after infrastructure recovery

- **Given** claim/processing infrastructure fails while retained unresolved targets and a running host remain.
- **When** workers recover and continue.
- **Then** failures are logged, repeated claim retries use configured bounded backoff, and later eligible work runs within queue/concurrency bounds and valid operation scopes.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### OUTBOX-014-AC-01 — Recover lost signal and crashed queued claims

Reserved pending [GAP-CLAIM-001](../GAPS.md#gap-claim-001). Recovery after lost signals or queued-worker crashes remains required. The original composite additionally counts an abandoned acquisition without completed handling; decide budget accounting/exhaustion before restoring it.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
