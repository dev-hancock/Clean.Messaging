# INBOX-010 — Entry cleanup and indefinite admission retention

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: `InboxCleanupWorker`, inherited `DurableStore.Cleanup`; no owned admission cleanup operation exists. [GAP-DUR-004](../GAPS.md#gap-dur-004) records the retention decision.

Contracts: IInboxStore.Cleanup inherited; cleanup worker starts immediately then periodically with UTC cutoff now minus retention and one batch per tick. Positive batch limit required. Only entry terminal timestamps strictly older than cutoff qualify; pending rows survive. Ordering is CreatedAt. Admissions and original payloads are retained indefinitely by this capability. Cleanup removes per-target completion/dedup evidence, enabling later replay of the deleted pair.

## INBOX-010-AC-01 — Respect terminal and strict cutoff retention

- **Given** pending and terminal rows exist with terminal timestamps before, equal to and after cutoff.
- **When** Cleanup(before, positive limit) runs.
- **Then** only terminal rows strictly older are deleted, up to the limit; pending and equality-boundary rows remain.

## INBOX-010-AC-02 — Retain admission while replay evidence expires

- **Given** an old completed target has been cleaned but its original admission remains.
- **When** duplicate Accept and explicit Replay for that target run.
- **Then** Accept stays duplicate; Replay can create that target again, so cleaned completion no longer prevents repeated local effects.

## INBOX-010-AC-03 — Retry periodic cleanup without successful deletion claim

- **Given** cleanup throws an ordinary exception, or host cancellation occurs.
- **When** a running correctly resolved cleanup worker handles it.
- **Then** ordinary failure is warning-logged and a later tick can retry; host cancellation stops without implying deleted rows.
