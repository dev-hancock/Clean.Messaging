# DOMAIN-006 — Defer acknowledgement and deduplicate multi-save capture

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications may save more than once in a transaction while facts remain pending. Those saves must accumulate new outgoing responsibilities without recapturing earlier facts or acknowledging events whose transaction could still roll back.

## Guarantees

- A captured fact MUST remain pending until the owning commit is observed or verified, and repeated successful saves in that transaction MUST suppress its duplicate capture.
- Distinct facts raised before later saves MUST join the transaction’s outgoing work without replacing or recapturing earlier facts.
- Commit acknowledgement MUST select captured identities only; a newer uncaptured fact MUST remain pending.

## Contract boundaries

These guarantees concern stable, distinct facts in one correctly observed owning transaction, with valid targets and ordinary AggregateRoot acknowledgement. They do not establish a permanent event admission ledger, cross-context capture suppression or a policy for colliding custom-source IDs. Delivery remains target-specific outbox work after commit.

## Acceptance criteria

### DOMAIN-006-AC-01 — Keep facts pending across multiple saves

- **Given** a fact was captured by the first save in an explicit transaction.
- **When** further saves succeed and then the transaction commits.
- **Then** the fact stays pending before commit, is not captured twice and is removed by normal acknowledgement only after commit.

### DOMAIN-006-AC-02 — Capture newly raised facts in later save

- **Given** an earlier fact is pending in transaction capture state and a distinct fact is raised before a later save.
- **When** the later save and final commit succeed.
- **Then** both respective target sets commit and both facts are acknowledged without recapturing the earlier fact.

### DOMAIN-006-AC-03 — Acknowledge only selected committed identities

- **Given** a new fact is raised after the last capture without disrupting enumeration.
- **When** the earlier captured work commits.
- **Then** only captured IDs are acknowledged and the newer uncaptured fact remains pending.

## Open decisions

None.
