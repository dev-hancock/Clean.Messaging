# OUTBOX-014 — Workers, signals, bounded queue and recovery

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: OutboxSignal.WaitAsync/Wake, OutboxWorker, OutboxRecoveryWorker and shared WorkLoop. No persisted notification, global FIFO or fixed-latency guarantee.

Purpose: execute committed due work eventually under healthy host/store conditions. Preconditions: valid options and resolvable scoped services. Worker startup wakes, periodic recovery wakes, hints coalesce; bounded queued/active work with configured concurrency. Leases start before queue wait; renewal only starts when processing, so stalled queued ownership can expire. Claim errors back off; processing errors log and later work continues.

## OUTBOX-014-AC-01 — Recover lost signal and crashed queued claims

- **Given** commit hint was lost or host crashed after claiming queued entries.
- **When** restart/recovery runs and stale leases expire.
- **Then** eligible committed work becomes attemptable again, potentially increasing Attempts without prior handler completion.

## OUTBOX-014-AC-02 — Coalesce signals and cancel waits

- **Given** multiple Wake calls occur and a later wait has a cancelled token.
- **When** WaitAsync consumes or waits for an in-memory hint.
- **Then** wake counts are not persisted; available work, not signal count, drives claims and cancellation can stop a wait.

## OUTBOX-014-AC-03 — Continue after infrastructure recovery

- **Given** claim/processing infrastructure is temporarily unavailable.
- **When** the running worker logs failures and configured recovery wakes after availability returns.
- **Then** durable work is retried without being marked delivered by infrastructure-error logging.
