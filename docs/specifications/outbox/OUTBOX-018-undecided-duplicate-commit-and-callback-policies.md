# OUTBOX-018 — Undecided duplicate, commit and callback policies

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour

**Evidence:** Evidence: tracked writer, target provider, tracker/commit verifier and [GAP-DUR-006](../GAPS.md#gap-dur-006), [GAP-DUR-007](../GAPS.md#gap-dur-007), [GAP-DUR-008](../GAPS.md#gap-dur-008), [GAP-DUR-009](../GAPS.md#gap-dur-009). No normative resolution found.

The numbered questions below remain undecided. Preconditions: explicit product policy selection; no implicit repair, guarantee of durable callbacks or idempotent save is approved. Limitations include commit evidence deletion and callback-only/custom capture integrations.

**Undecided questions (not acceptance criteria):**

1. **external-only and empty targeting contract:** What approved policy should apply when a valid event has only external targets or no targets? Decision to resolve: specify empty/local-miss handling without relying on current Get exception or silently acknowledging zero outgoing work.

2. **persisted duplicate and content conflict contract:** What approved policy should apply when the logical pair already exists with equal or conflicting payload? Decision to resolve: choose duplicate suppression, conflict failure or documented uniqueness failure and define tracked versus persisted equivalence/retention scope.

3. **uncertain commit recovery and callback safety:** What approved policy should apply when keys are absent/cleaned, verifier unavailable, or postcommit callback throws? Decision to resolve: define context disposal/recovery, evidence horizon, callback isolation/error reporting and zero-key outcome without declaring commit/rollback from uncertainty.
