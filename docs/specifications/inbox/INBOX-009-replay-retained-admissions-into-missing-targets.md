# INBOX-009 — Replay retained admissions into missing targets

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications need to assign missing consumer obligations to retained incoming history without replacing its envelope or resetting any already represented target. Operators also need bounded selection and honest replay outcomes.

## Guarantees

- Replay MUST preserve the original delivery/event identity, contract, payload, nullable metadata and creation time when creating a missing unresolved target.
- An existing event/target pair in any lifecycle state MUST remain unchanged and MUST NOT receive a duplicate target entry.
- Selection MUST use the registered message contract, honor inclusive supplied creation-time bounds and a positive limit, and reject blank target IDs, inverted bounds or missing contracts.
- A valid empty selection MUST report zero matched and replayed entries. For an uncontended successful selection, counts MUST describe the selected admissions and actually added targets.
- Replay persistence failure MUST remain a failure unless its intended outcome is reliably reconciled; a small or zero addition count is not proof of success.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 7, 9 and 14 establish stable historical envelope/target identity, no reset of retained completion, bounded work and failure transparency; public date-range and result compatibility establish inclusive selection and empty-result behavior.

## Contract boundaries

Replay assigns a target; it is not re-admission or an API to reset a retained completed, pending, dead-lettered or discarded row. It requires retained admissions and resolvable historical contracts. Viable target validation, page progress and concurrent result attribution need explicit decisions. Deleting target evidence can permit repeated local/external effects; replay does not promise exactly-once processing.

## Acceptance criteria

### INBOX-009-AC-01 — Preserve original envelope in replay

- **Given** retained admissions of the registered contract are inside inclusive creation-time bounds and their requested targets are absent.
- **When** an uncontended replay succeeds.
- **Then** new unresolved targets preserve MessageId, EventId, contract, payload, metadata and CreatedAt, and Matched/Replayed describe selected admissions and actually added targets.

### INBOX-009-AC-03 — Handle invalid and empty replay selection

- **Given** target is blank, limit nonpositive, dates inverted or contract missing, compared with a valid query having no matches.
- **When** replay is requested.
- **Then** invalid input or contract fails visibly, while the empty valid selection returns Matched=0 and Replayed=0.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### INBOX-009-AC-02 — Do not reset existing targets of any state

Reserved pending [GAP-DUR-003](../GAPS.md#gap-dur-003). Existing targets of every state remain protected from reset and duplication. The original composite also makes already-represented admissions consume the selected replay window; that page-progress policy is unresolved.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### INBOX-009-AC-04 — Bound before exclusion without cursor advancement

Reserved pending [GAP-DUR-003](../GAPS.md#gap-dur-003). Repeatedly revisiting the oldest fully represented page without reaching later missing targets is a current limitation, not an approved progress contract. Decide bounded traversal/resumption and its result semantics.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### INBOX-009-AC-05 — Allow incompatible target names at management boundary

Reserved pending [GAP-DUR-002](../GAPS.md#gap-dur-002). Allowing unregistered, incompatible, non-ASCII or overlong replay targets without early suitability checks is not approved. Decide valid identity and compatibility requirements together with any deliberate delayed-registration support.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### INBOX-009-AC-06 — Reconcile concurrent replay inserts

Reserved pending [GAP-DUR-003](../GAPS.md#gap-dur-003). Per-event/target uniqueness remains established. Reporting a concurrent increase that can include another caller's additions requires an explicit result-attribution and duplicate-reconciliation policy.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### INBOX-009-AC-07 — Report current swallowed update failure

Reserved pending [GAP-DUR-003](../GAPS.md#gap-dur-003). Returning normal replay counts after an unrelated, unreconciled update failure is not an approved requirement and conflicts with established failure transparency. The original swallowed-error assertion is retained only for an explicit disposition, not as permitted success.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
