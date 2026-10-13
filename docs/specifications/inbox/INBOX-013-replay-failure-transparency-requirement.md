# INBOX-013 — Replay failure transparency requirement

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour

**Evidence:** Normative: coding standards prohibit hiding failure behind successful/default outcomes. Descriptive counter-evidence: InboxStore.Replay catches any DbUpdateException and returns a count after reconciliation. See [GAP-DUR-003](../GAPS.md#gap-dur-003).

Precondition: an actual persistence error not positively reconciled as successful replay. Required postcondition: observable failure remains distinguishable from successful zero additions. This is a suspected implementation mismatch, not authorization to fix code.

## INBOX-013-AC-01 — Do not turn unreconciled replay failure into success

- **Given** a replay insert fails and no evidence confirms a successful operation.
- **When** the operation completes its failure path.
- **Then** required outcome: the caller can distinguish storage failure from a successful no-op; the observed zero-count path does not currently demonstrate this.
