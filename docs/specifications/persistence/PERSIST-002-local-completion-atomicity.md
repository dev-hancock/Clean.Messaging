# PERSIST-002 — Local completion atomicity

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §8/§9 and architecture execution boundaries explicitly preserve same-context consumer changes and completion.

Pre: local inbox/outbox dispatcher, same configured scoped context and supported relational transaction. Post: committed participating effects and owned completion agree; failed completion/consumer attempts leave neither committed. Invocation itself can repeat.

## PERSIST-002-AC-01 — Commit effect and completion
**Given** a local consumer modifies configured-context state while owning its target.
**When** processing succeeds and commits.
**Then** an independent reader observes both the business change and completed target.

## PERSIST-002-AC-02 — Handler fails before commit
**Given** a local consumer modifies configured-context state then fails.
**When** its local attempt rolls back.
**Then** neither that business change nor successful target completion is persisted.

## PERSIST-002-AC-03 — Stale completion
**Given** a local consumer has changed configured-context state but its ownership is no longer valid.
**When** completion is rejected.
**Then** the local changes do not commit as a successful attempt.

## PERSIST-002-AC-04 — External effect excluded
**Given** a local consumer performs an independently committed external operation then its local save fails.
**When** the local transaction rolls back.
**Then** no claim is made that the external operation was undone; local state/completion remain unsuccessful.
