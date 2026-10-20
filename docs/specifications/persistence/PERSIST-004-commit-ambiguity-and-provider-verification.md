# PERSIST-004 — Commit ambiguity and provider boundaries

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Callers need to distinguish established commit outcomes from uncertainty so recovery does not assume either success or rollback. Hosts also need an explicit provider-support boundary for the local consistency guarantees they rely on.

## Guarantees

- A failed commit notification MUST NOT be interpreted as proof of rollback or proof of success without adequate outcome evidence.
- Persistence guarantees MUST remain scoped to their actual local participants and provider boundary, not be presented as universal multi-host or cross-database certification.

## Contract boundaries

The host supplies the relational provider and schema and owns reconciliation after uncertainty. Retained identity in [PERSIST-001](PERSIST-001-retained-logical-target-uniqueness.md) and local completion atomicity in [PERSIST-002](PERSIST-002-local-completion-atomicity.md) remain binding within their stated boundaries. An unresolved provider-support policy does not suspend those requirements.

Outcome evidence must concern the intended operation: observing a compatible stored state is not automatically proof that a particular racing attempt committed. The adequacy of that evidence and the supported provider/concurrency envelope remain unresolved; no new verification protocol is selected here.

The [persistence reference](../../reference/persistence.md) describes current transaction integration and limitations. Provider certification is not implied by registration, relational API availability or this specification.

## Acceptance criteria

No acceptance-criterion IDs are assigned to the unresolved policies on this page.

## Open decisions

- [GAP-CORE-005](../GAPS.md#gap-core-005): which providers and commit-loss/race outcomes are supported, and what evidence can attribute success to the intended operation? Recommendations remain unapproved.
