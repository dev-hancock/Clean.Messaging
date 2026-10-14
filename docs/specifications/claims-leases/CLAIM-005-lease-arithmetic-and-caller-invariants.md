# CLAIM-005 — Lease arithmetic and caller invariants

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Provider callers need a defined domain for lease times and tokens so that invalid input cannot ambiguously grant or shorten processing authority.

## Guarantees

- For valid current ownership, renewal and settlement MUST preserve the identity and stale-owner protections of [CLAIM-002](CLAIM-002-stale-settlement-rejection.md) and [CLAIM-003](CLAIM-003-owned-state-and-expiry-predicates.md).

## Contract boundaries

Normal processing supplies UTC, representable times, a claim token and a later renewal expiry. This page does not establish outcomes for shortening/past expiry, non-UTC instants, overflowing arithmetic or empty tokens. Normal caller practice is not approval of a direct-provider input policy.

## Acceptance criteria

No additional acceptance outcomes are assigned while the policy below remains unresolved.

## Open decisions

[GAP-CORE-004](../GAPS.md#gap-core-004): decide supported direct ownership inputs and failure outcomes. Recommendation remains unapproved: define representable UTC inputs and usable tokens, and reject unusable expiry without inventing extension-only semantics.
