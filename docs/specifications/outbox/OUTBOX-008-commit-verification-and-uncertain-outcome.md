# OUTBOX-008 — Commit verification and uncertain outcome

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: OutboxTransactionInterceptor.TransactionFailed/Async, OutboxCommitVerifier, OutboxTracker.GetPendingKeys/TransactionCommitUnknown. [GAP-DUR-008](../GAPS.md#gap-dur-008) covers recovery/verification horizon.

Purpose: reconcile failed commit notification using retained outgoing entry keys in a fresh context. Preconditions: marked commit with pending keys. All keys present implies observed committed handling; not all present selects rollback handling; query error or zero keys leaves unknown. Verification is source-derived inference, not certification of other writes or external effects; async verification ignores the caller cancellation token (None). Unknown state does not acknowledge or claim successful rollback. Cleanup/removal and callback-only captures complicate inference.

## OUTBOX-008-AC-01 — Verify a lost successful commit response

- **Given** transaction commit notification fails after database commit and every pending outgoing key remains queryable.
- **When** fresh-context verification succeeds.
- **Then** tracker acknowledges commit and can wake outgoing processing while original commit-failure handling remains subject to EF behaviour.

## OUTBOX-008-AC-02 — Handle missing keys versus unavailable verification

- **Given** commit fails during marked commit with some expected keys absent, or verifier throws.
- **When** transaction failure interception runs.
- **Then** absent-key evidence selects rollback tracking; verification error selects unknown state and no definite acknowledgement.

## OUTBOX-008-AC-03 — Keep zero-key outcome unknown

- **Given** a failed commit has no outgoing keys suitable for verification.
- **When** failure interception attempts reconciliation.
- **Then** no committed outcome is inferred from empty keys; acknowledgement and application state certainty are not proved.

## OUTBOX-008-AC-04 — Do not cancel verification with failed commit token

- **Given** the commit token is cancelled and a marked commit requires async verification.
- **When** the failure hook verifies.
- **Then** verification uses an independent None token; its success/error, not the cancelled commit token alone, controls tracker reconciliation.
