# INBOX-007 — Signals, workers and restart recovery

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: `InboxSignal.WaitAsync/Wake`, `InboxWorker`, `InboxRecoveryWorker`, shared WorkLoop and AsyncAutoResetEvent. The signal is a coalesced in-memory hint, not persisted work.

Contracts: public signal and BackgroundService lifecycle inherited by workers. Options bound concurrency, capacity, batch, lease, settlement, recovery and cleanup. Preconditions: host running with resolvable services; no exact latency/order/fairness guarantee. Postconditions: startup/recovery wakes enable available work to be claimed, including work whose admission hint was lost. Claim leases begin before queueing; renewal begins during processing.

## INBOX-007-AC-01 — Coalesce wake hints and cancel a wait

- **Given** no signal is pending, then several Wake calls occur.
- **When** a WaitAsync consumes a hint and a later waiting token is cancelled.
- **Then** one wake can satisfy the wait without persisting a count of calls; cancelled waiting propagates cancellation.

## INBOX-007-AC-02 — Recover after crash without a hint

- **Given** durable unresolved work remains after restart or queue loss and any old lease expires.
- **When** startup or periodic recovery wakes a running healthy worker.
- **Then** eligible work is eventually attempted subject to host/database availability; crash claims may increase Attempts without a completed handler.

## INBOX-007-AC-03 — Recover after claim or processing infrastructure failures

- **Given** a claim fails or processing cannot settle and a database/service later becomes available.
- **When** the worker logs failure and continues with configured recovery.
- **Then** durable work remains recoverable, with bounded in-process execution rather than silently deleted or marked completed.
