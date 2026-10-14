# DOMAIN-005 — Atomic captured facts and application commit

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications must not commit state while losing the outgoing facts that explain that state, or discard pending facts for a save that did not commit. Capture acknowledgement releases local pending work only after its participating outgoing work is durably committed.

## Guarantees

- Participating application changes and successfully captured outgoing targets MUST commit atomically in the owning transaction.
- A capture/save that fails or is cancelled before a committed result is established MUST NOT acknowledge its facts as successfully captured. Newly captured rows from that failed save MUST cease being tracked for persistence so capture can be retried.
- An observed explicit rollback MUST leave captured facts pending and MUST NOT commit their outgoing targets. Acknowledgement MUST NOT represent downstream completion.

## Contract boundaries

The context is correctly composed and the facts have valid nonempty target sets. Other contexts, external effects and uncaptured in-memory facts are outside this atomic boundary. A failed commit notification can coexist with a committed database result: uncertainty is not proof of rollback. Application entity reconciliation after rollback is not automatic.

## Acceptance criteria

### DOMAIN-005-AC-01 — Commit state and event targets atomically

- **Given** application state changes and valid tracked facts with outgoing targets participate in one owning transaction.
- **When** the transaction commits successfully.
- **Then** application changes and captured outgoing rows become durable together; source acknowledgement denotes capture commit, not consumer or broker completion.

### DOMAIN-005-AC-02 — Do not acknowledge failed or cancelled save

- **Given** new event rows were captured but save fails or observes cancellation before successful commit is established.
- **When** failed-save bookkeeping runs.
- **Then** the facts remain pending, no successful acknowledgement is invented and the newly captured outgoing rows are detached from the context for retry.

### DOMAIN-005-AC-03 — Retain pending events on explicit rollback

- **Given** one or more captures were saved inside an explicit transaction.
- **When** its rollback is observed.
- **Then** the captured outgoing state is not committed and its facts remain unacknowledged; the application still owns reconciliation of aggregate state.

## Open decisions

- [GAP-DUR-008](../GAPS.md#gap-dur-008): the related policy remains undecided; its recommendation is not approved.

- [GAP-DUR-009](../GAPS.md#gap-dur-009): the related policy remains undecided; its recommendation is not approved.
