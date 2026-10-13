# INBOX-009 — Replay retained admissions into missing targets

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: `IInboxManager.Replay<TMessage>`, `InboxManager.Replay`, `InboxStore.Replay`; `ReplayResult.Matched/Replayed`. [GAP-DUR-002](../GAPS.md#gap-dur-002) covers target validation/compatibility, [GAP-DUR-003](../GAPS.md#gap-dur-003) covers count/error ambiguity.

Purpose: explicitly introduce a target for retained history, not resubmit an envelope or reset an existing target. Preconditions at manager: consumerId nonblank, limit positive (default 1000), from <= to when both supplied, registered TMessage contract. No consumer registration/type compatibility check or ASCII/length validation occurs here. Inclusive CreatedAt filtering; sort CreatedAt then MessageId; limit applied before checking existing event/target pairs of every state. Result Matched means selected admissions, not total history; Replayed means inserted/reconciled pair count. Empty selection returns zero/zero; wake occurs when Replayed > 0.

## INBOX-009-AC-01 — Preserve original envelope in replay

- **Given** retained admissions of the registered contract lie inside inclusive from/to boundaries and the target is absent.
- **When** Replay selects them.
- **Then** new unresolved targets preserve original MessageId, EventId, contract, payload, metadata and CreatedAt, and counts reflect selected/added entries.

## INBOX-009-AC-02 — Do not reset existing targets of any state

- **Given** the selected history has existing pending, processed, dead-lettered or discarded event/target pairs.
- **When** Replay is requested for the same target.
- **Then** those entries retain state and are excluded from additions; matched entries still count toward the selection limit.

## INBOX-009-AC-03 — Handle invalid and empty replay selection

- **Given** blank target, nonpositive limit, inverted dates or missing contract, versus a valid query with no matching admissions.
- **When** Replay is requested.
- **Then** invalid manager arguments/contracts fail; an empty valid selection reports Matched=0 and Replayed=0.

## INBOX-009-AC-04 — Bound before exclusion without cursor advancement

- **Given** the oldest limited history window already has every requested target, and later history lacks it.
- **When** the same limited Replay is repeated.
- **Then** Matched remains the selected window size, Replayed stays zero, and later admissions are not automatically visited.

## INBOX-009-AC-05 — Allow incompatible target names at management boundary

- **Given** a nonblank but unregistered, incompatible, non-ASCII or overlong target ID.
- **When** Replay is requested.
- **Then** manager-level compatibility/syntax rejection is not supplied; provider storage or later consumer resolution can fail, rather than proving a viable replay target.

## INBOX-009-AC-06 — Reconcile concurrent replay inserts

- **Given** independent Replay requests select the same missing logical event/target pairs.
- **When** their insertions race.
- **Then** unique keys prevent duplicate rows; a DbUpdateException path reports the increase in persisted pairs, which can include another caller's additions.

## INBOX-009-AC-07 — Report current swallowed update failure

- **Given** a replay SaveChanges throws DbUpdateException unrelated to a duplicate and reconciliation finds no new pairs.
- **When** Replay returns from its current EF catch path.
- **Then** it reports selected Matched and Replayed=0 instead of rethrowing that update error; this is a suspected mismatch, not approved behaviour to require.
