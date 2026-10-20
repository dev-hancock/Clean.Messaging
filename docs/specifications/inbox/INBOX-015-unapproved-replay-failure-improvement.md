# INBOX-015 — Replay failure reconciliation policy

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Callers need truthful replay outcomes when concurrent requests or persistence failures make insertion results uncertain. Recovery must not conceal an unreconciled failure; deciding how to recognize a successful concurrent outcome is a separate policy choice.

## Guarantees

- Replay MUST preserve the required failure-transparency contract in [INBOX-013](INBOX-013-replay-failure-transparency-requirement.md).

## Contract boundaries

Failure transparency is required independently of the reconciliation policy. The proposed duplicate-only rule does not establish target suitability, continuation behavior, count attribution or all-or-nothing replay. An uncertain partial outcome remains possible.

The [Inbox reference](../../reference/inbox.md) describes the current persistence behavior. The decision below separates that behavior and the suspected failure-transparency discrepancy from the unapproved reconciliation proposal.

## Acceptance criteria

The existing criterion is reserved under Open decisions, not an approved reconciliation rule. Required failure transparency is specified separately in INBOX-013.

## Open decisions

[GAP-DUR-003](../GAPS.md#gap-dur-003) owns the reconciliation, result-attribution and continuation decisions. A suspected violation of required failure transparency is distinct from those policy choices.

### INBOX-015-AC-01 — Proposal reconcile only confirmed duplicate outcomes

Reserved pending [GAP-DUR-003](../GAPS.md#gap-dur-003). The proposed reconcile-only-confirmed-duplicates algorithm is unapproved. Required failure transparency remains in force, but the exact evidence, concurrent count attribution and partial-outcome recovery protocol need a decision.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
