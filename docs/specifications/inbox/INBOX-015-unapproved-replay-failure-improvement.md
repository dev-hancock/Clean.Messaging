# INBOX-015 — Unapproved replay failure improvement

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Proposed behaviour

**Evidence:** Recommendation only, not approved: [GAP-DUR-003](../GAPS.md#gap-dur-003). Motivated by normative failure transparency and descriptive replay reconciliation evidence; no implementation change was made.

Purpose: candidate reconciliation semantics, subject to approval. Preconditions: failed replay save; postcondition proposal: duplicate conflicts may reconcile but unrelated unreconciled storage errors stay visible. Does not decide count attribution or pagination.

## INBOX-015-AC-01 — Proposal reconcile only confirmed duplicate outcomes

- **Given** a replay save fails with either a confirmed concurrent uniqueness conflict or an unrelated update failure.
- **When** an approved implementation of the proposal is evaluated.
- **Then** unapproved proposal: return reconciled results only for confirmed successful/duplicate outcomes and preserve the other error; this criterion is not yet binding.
