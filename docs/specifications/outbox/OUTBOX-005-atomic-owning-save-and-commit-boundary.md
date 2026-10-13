# OUTBOX-005 — Atomic owning save and commit boundary

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour

**Evidence:** Normative: coding standards and repository instructions require durable consistency and scoped transaction preservation. Source evidence: save/transaction interceptors, tracker, configured-context staging. Descriptive persistence/transaction-boundary docs do not make every interceptor detail normative.

Purpose: commit application changes and outgoing entries together. Preconditions: configured relational owning context with model and save/transaction interception; correctly staged rows. Postconditions: rows do not authorize delivery before owning commit, and failed transaction does not commit intended application/outbox state. Other contexts and external publication are outside this boundary. Caller must not interpret staging or an uncommitted save as completed delivery.

## OUTBOX-005-AC-01 — Commit application state with outgoing work

- **Given** application changes and staged targets share the configured scoped context.
- **When** the owning transaction commits successfully.
- **Then** both application state and outgoing rows are durable together and available for postcommit processing.

## OUTBOX-005-AC-02 — Do not publish uncommitted work

- **Given** an explicit transaction has saved application/outbox rows but has not committed.
- **When** a worker using an independent store tries to discover work.
- **Then** uncommitted rows do not constitute committed outgoing work; acknowledgement/wake based on commit is deferred.

## OUTBOX-005-AC-03 — Roll back the owning transaction

- **Given** saved outgoing rows and application changes are in an explicit transaction.
- **When** the transaction is rolled back.
- **Then** those database changes are not durable outgoing work; in-memory rollback does not reset arbitrary application state.
