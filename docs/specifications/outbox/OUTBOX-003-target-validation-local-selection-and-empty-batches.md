# OUTBOX-003 — Target validation, local selection and empty batches

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need target identities that distinguish independent obligations and survive persistence, without forcing provider-neutral routing to understand each transport's destination protocol.

## Guarantees

- Target IDs MUST be nonblank ASCII values no longer than 200 characters; transport identity MUST be nonblank.
- Duplicate target IDs within one outgoing batch MUST fail even if their transport or destination differs. Ordinal case-distinct target IDs MUST remain distinct.
- Persisted target/transport/destination MUST remain a snapshot; successful target completion MUST not depend on sibling target progress.
- Provider-specific destination interpretation MUST remain owned by the appropriate integration rather than introduce protocol coupling into neutral routing.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Persisted target identity/schema compatibility and coding standards sections 9 and 14 establish nonblank stable identities, bounded ASCII target keys and ordinal uniqueness; architecture assigns protocol mechanics to transport integrations.

## Contract boundaries

Target ID is the per-event obligation identity, not merely a route display label. This contract does not approve the current omission of transport length or destination checks, default local failure for external-only messages, or silent targetless staging. Shared persisted-field requirements and provider-specific destination semantics are different validation concerns.

## Acceptance criteria

### OUTBOX-003-AC-01 — Validate target boundary and duplicates

- **Given** a batch has a blank, non-ASCII or overlong target ID, a blank transport, duplicate ordinal target IDs across routes, or otherwise-valid case-distinct IDs.
- **When** target staging validates the batch.
- **Then** invalid identities and ordinal duplicates fail, while case-distinct target IDs remain distinct obligations.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### OUTBOX-003-AC-02 — Do not infer transport or destination schema validation

Reserved pending [GAP-INTENT-DUR-001](../GAPS.md#gap-intent-dur-001) (unapproved route-validation decision). Protocol-specific destination interpretation remains provider-owned. The original omission of comprehensive transport-name/destination checks also selects validation timing for portable persisted constraints; that permissiveness is not independently approved.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### OUTBOX-003-AC-03 — Observe external-only default local failure

Reserved pending [GAP-DUR-006](../GAPS.md#gap-dur-006). Requiring default local lookup to fail even when external targets exist is not an approved external-only routing policy. Decide whether local absence is a configuration failure or simply no local obligation.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### OUTBOX-003-AC-04 — Stage an empty custom batch without work

Reserved pending [GAP-DUR-006](../GAPS.md#gap-dur-006). Treating a custom targetless batch as a successful no-op needs an explicit validity and capture-responsibility decision; representability of an empty batch does not approve silent loss of outgoing intent.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
