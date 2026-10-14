# OUTBOX-008 — Commit verification and uncertain outcome

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications need an honest answer after a commit response is lost: local state may have committed, rolled back or remained unknown. Recovery must not release capture responsibility on evidence that does not establish the outcome.

## Guarantees

- A failed commit response MUST NOT by itself establish rollback or erase the possibility that the complete local result committed.
- Acknowledgement of capture as committed MUST depend on a known commit outcome rather than the failed response alone.
- An outcome with no meaningful durable evidence, including zero outgoing keys when no other commit proof exists, MUST remain unknown rather than be called verified success.
- Verification unavailability MUST NOT manufacture successful capture or proof that no external/local effect occurred.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 7 and 8 require preserving uncertain commit and original failure distinctions; atomic capture does not imply immediate knowledge of its outcome.

## Contract boundaries

Verification is not a distributed transaction or an eventual-delivery guarantee. Evidence can expire through cleanup; key absence is not universally proof of rollback, and a response failure is not proof of absence. Exact all-key evidence rules, verification cancellation/lifetime and recovery actions for unknown outcome require an explicit decision.

## Acceptance criteria

### OUTBOX-008-AC-03 — Keep zero-key outcome unknown

- **Given** a commit response failed and the pending operation has zero outgoing verification keys and no independent commit proof.
- **When** outcome verification is considered.
- **Then** the outcome remains unknown and is not acknowledged merely as verified successful commit.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### OUTBOX-008-AC-01 — Verify a lost successful commit response

Reserved pending [GAP-DUR-008](../GAPS.md#gap-dur-008). Treating presence of every pending outgoing key as sufficient proof of a lost successful commit selects a specific evidence protocol and acknowledgement release. Known-commit recovery remains required, but this evidence rule needs explicit authority.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### OUTBOX-008-AC-02 — Handle missing keys versus unavailable verification

Reserved pending [GAP-DUR-008](../GAPS.md#gap-dur-008). The original composite infers rollback from missing keys but leaves verifier unavailability unknown. Cleanup can remove commit evidence, so absent rows do not universally establish rollback; decide reliable proof and its recovery consequences.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### OUTBOX-008-AC-04 — Do not cancel verification with failed commit token

Reserved pending [GAP-DUR-008](../GAPS.md#gap-dur-008). Verification independent of the failed request token is a lifetime/cancellation policy, not approval of unlimited uncancellable verification. Decide a bounded independent verification lifetime while preserving database outcome uncertainty.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
