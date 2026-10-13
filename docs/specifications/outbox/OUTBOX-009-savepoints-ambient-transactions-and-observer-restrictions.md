# OUTBOX-009 — Savepoints, ambient transactions and observer restrictions

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: tracker BeginSave and transaction interceptor CreatingSavepoint/RollingBackToSavepoint synchronous/asynchronous paths. [Persistence](../../reference/persistence.md) describes restrictions.

Contracts: sync/async interceptors share lifecycle semantics. Ambient transactions are unsupported for capture; explicit transaction end must be observed. User savepoint creation/rollback outside active save throws NotSupportedException; EF save-time savepoints are allowed. Raw/unobserved commit or disposing without interception is not a supported acknowledgement protocol. No snapshot of callback/key state per user savepoint.

## OUTBOX-009-AC-01 — Reject user savepoints outside active save

- **Given** outbox tracking is attached and no save is active.
- **When** user savepoint creation or rollback-to-savepoint is intercepted.
- **Then** NotSupportedException reports unsupported tracking rather than a silently partial state rewind.

## OUTBOX-009-AC-02 — Allow EF save-time savepoints

- **Given** an intercepted SaveChanges is active in an explicit relational transaction.
- **When** EF creates or rolls back its save-time savepoint.
- **Then** the active-save path is permitted and save success/failure still controls capture bookkeeping.

## OUTBOX-009-AC-03 — Expose unobserved transaction end restriction

- **Given** an integration bypasses observed transaction end with pending work.
- **When** it tries a subsequent save on the context.
- **Then** unsupported lifecycle can be rejected; caller must reconcile/discard rather than infer event acknowledgement from raw commit.
