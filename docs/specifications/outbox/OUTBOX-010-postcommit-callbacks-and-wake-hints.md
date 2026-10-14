# OUTBOX-010 — Postcommit callbacks and wake hints

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications need to release in-memory capture responsibility only once outgoing work is durably owned, without confusing postcommit callbacks or wake hints with delivery to recipients.

## Guarantees

- When normal postcommit callbacks finish, known local commit MUST trigger capture acknowledgement and a processing wake; neither initial staging nor successful SaveChanges inside an uncommitted explicit transaction authorizes that acknowledgement.
- Capture acknowledgement MUST mean the outgoing intent is durable, not that any local consumer or external recipient completed it.
- Durable committed work MUST remain recoverable without its original wake hint. A callback failure after commit MUST NOT be represented as having rolled back that committed database result.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 7, 8 and 9 establish commit-before-acknowledgement, durable responsibility independent of hints and separation of callback failure from database truth.

## Contract boundaries

Callbacks execute outside the database commit and cannot undo it. Callback identity collisions, recovery after a callback throw, callback-only participation and error reporting during reconciliation need an explicit policy. This page does not promise exactly-once callbacks, guaranteed callback order/retry or network delivery by acknowledgement time.

## Acceptance criteria

### OUTBOX-010-AC-01 — Acknowledge capture rather than delivery

- **Given** outgoing capture has known local commit while delivery is still pending, including an explicit transaction with earlier successful saves.
- **When** the owning commit is observed and its postcommit callbacks finish normally.
- **Then** when callbacks finish normally, captured events are acknowledged and outgoing processing is signalled after known commit; acknowledgement means durable outgoing intent rather than completed recipient delivery, and lost hints do not remove retained work.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### OUTBOX-010-AC-02 — Surface callback failure after durable commit

Reserved pending [GAP-DUR-009](../GAPS.md#gap-dur-009). Interruption of later callbacks, wake and tracking cleanup by one postcommit callback failure is a current limitation. Durable committed work remains recoverable; choose callback failure propagation and recovery rather than require this interruption sequence.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### OUTBOX-010-AC-03 — Observe callback-only participation gap

Reserved pending [GAP-DUR-009](../GAPS.md#gap-dur-009). Ignoring callback-only participation when no captures or outgoing keys exist is not approved as a postcommit scheduling contract. Decide whether and how such callbacks participate, without claiming durability from registration alone.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### OUTBOX-010-AC-04 — Distinguish ordinary callback failure from commit-failure reconciliation

Reserved pending [GAP-DUR-009](../GAPS.md#gap-dur-009). Swallowing a callback exception during commit-failure reconciliation and relabeling tracker state unknown is not approved error policy. Callback failure cannot undo a committed result; choose truthful reporting and recovery of remaining acknowledgements.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
