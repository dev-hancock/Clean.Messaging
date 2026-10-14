# INBOX-014 — Admission and replay policy boundaries

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications need explicit durable-admission, replay and retention policy decisions before relying on consumerless acceptance, historical target creation or an unbounded duplicate-protection horizon.

## Guarantees

- Established stable-envelope, per-target completion, local atomicity and failure-transparency guarantees MUST remain in force while admission, replay and retention policy is unresolved.

## Contract boundaries

Admission, local consumer completion and management replay are distinct contracts. Incoming adapters, consumer hosts and operators must not infer one boundary's result from another. In particular, representing a targetless envelope does not establish a policy for accepting it.

This page collects unresolved consumer-facing decisions, not a new admission or replay algorithm. Current behavior is described in the [Inbox reference](../../reference/inbox.md); it does not approve the policy alternatives below.

## Acceptance criteria

No resolved acceptance criteria are assigned on this page. The unresolved choices are collected under Open decisions.

## Open decisions

- [GAP-DUR-001](../GAPS.md#gap-dur-001): decide new targetless admission and retained empty-duplicate validation.
- [GAP-DUR-007](../GAPS.md#gap-dur-007): decide conflicting incoming identity/content reuse without silently replacing the original or adding consumers on redelivery.
- [GAP-DUR-002](../GAPS.md#gap-dur-002): decide replay target syntax and registered contract compatibility.
- [GAP-DUR-003](../GAPS.md#gap-dur-003): decide page progress, concurrent counts and evidence-backed failure reconciliation.
- [GAP-DUR-004](../GAPS.md#gap-dur-004): decide admission/payload retention and its relationship to target completion deletion and same-target replay.

All recommendations in those decisions remain unapproved.
