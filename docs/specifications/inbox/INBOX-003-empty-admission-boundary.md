# INBOX-003 — Empty admission boundary

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Consumers need an explicit answer about accepting history for later processing when no initial target exists. Duplicate recognition must not accidentally decide the validity of a new targetless admission.

## Guarantees

- Admission outcomes MUST distinguish newly accepted work from recognition of retained identity; neither implies consumer completion.
- A retained admission MUST NOT be rewritten solely to resolve a targetless request.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Typed durable admission and immutable retained identity are established; no independent decision establishes whether an empty admission is a valid consumer capability.

## Contract boundaries

No normative empty-batch admission policy is selected here. The ability to represent an envelope without entries does not prove that the public admission boundary accepts it. Admission-only support must be decided together with replay and payload retention.

## Acceptance criteria

No resolved acceptance criteria are assigned on this page. The original identifiers remain reserved under Open decisions.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### INBOX-003-AC-01 — Reject a new empty batch

Reserved pending [GAP-DUR-001](../GAPS.md#gap-dur-001). Rejection of every new empty batch is not independently approved. Decide whether an envelope can be admitted for later target assignment rather than turning the current first-entry dependency into a permanent capability restriction.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### INBOX-003-AC-02 — Recognize retained empty duplicate

Reserved pending [GAP-DUR-001](../GAPS.md#gap-dur-001). Recognition of a retained duplicate before validating an otherwise empty batch selects a caller-visible validation order. Decide that order with new targetless admission; retained identity does not alone approve the empty-input policy.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
