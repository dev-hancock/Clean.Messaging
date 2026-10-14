# OUTBOX-018 — Undecided duplicate, commit and callback policies

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications need explicit policy choices for identity conflict, targetless/external-only routing, uncertain commit and postcommit acknowledgement before treating accidental implementation outcomes as durable promises.

## Guarantees

- Unapproved routing, conflict and recovery proposals MUST NOT be represented as required implemented policy.
- Owning-state capture atomicity, per-target identity, failure transparency and the external repeat limitation MUST remain established while related choices are open.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 7, 8 and 9 establish atomic capture, stable identities and truthful uncertainty; they do not independently select the unresolved content, target or callback protocols.

## Contract boundaries

This decision catalogue adds no resolved acceptance algorithm. Enqueue/save/commit/publication are separate boundaries. A unique index is not content comparison, key absence need not prove rollback, and a callback throw cannot undo committed database work.

## Acceptance criteria

No resolved acceptance criteria are assigned on this page. The unresolved choices are collected under Open decisions.

## Open decisions

- [GAP-DUR-006](../GAPS.md#gap-dur-006): decide external-only default routing, targetless validity and capture responsibility.
- [GAP-DUR-007](../GAPS.md#gap-dur-007): decide equivalent retry versus conflicting event/target content and persisted duplicate behavior.
- [GAP-DUR-008](../GAPS.md#gap-dur-008): decide uncertain commit evidence, evidence expiry and bounded verification lifetime/cancellation.
- [GAP-DUR-009](../GAPS.md#gap-dur-009): decide acknowledgement identity, callback-only participation and recovery/error reporting after callback failure.
- Outgoing route validation responsibility/timing remains unapproved in [OUTBOX-003](OUTBOX-003-target-validation-local-selection-and-empty-batches.md).

Documenting or scaling these specifications does not approve any recommendation.
