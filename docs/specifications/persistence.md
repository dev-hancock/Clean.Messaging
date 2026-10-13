# Persistence and transaction consistency — Behavioural specification

**Status:** Draft; source reviewed 2026-10-13; no acceptance criterion has been executed.
**Scope:** `IDurableEntry`, `IDurableStore<T>`, `OwnedEntry<T>`, public `DurableStore<TContext,TEntry>` operations/protected provider extension boundary, `DurableEntryConfiguration.Configure`; model/transaction helpers and store variants in capability pages.

## Purpose
Preserve durable local effects, retained identities and conditional ownership within explicit database boundaries without asserting distributed atomicity.

## Contracts, preconditions and postconditions
Pre: suitable relational provider, configured models/schema and standard store lifecycle; host-owned arbitrary model writes may bypass safeguards. Post: successful supported operations are observable through independent database reads. External transport effects and another context are outside local transaction participation.

## Invariants
- Persisted MessageId/TargetId identifies delivery work and EventId/TargetId protects retained logical target uniqueness.
- Configured-context effects and owned local completion must commit together where local dispatcher integration applies.
- A database claim cannot retract an issued remote effect.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are counted separately as gaps, not executable requirements.

## Limitations
- No tested provider/locking/isolation matrix, SKIP LOCKED promise, universal DateTime.Kind enforcement or cross-database transaction.
- Bulk store mutations do not guarantee refresh of caller-tracked entities.
- Schema/index details are compatibility lookup in reference/persistence.md, not a mandate for every future internal storage design.

## Specifications and acceptance criteria

### PERSIST-001 — Retained logical target uniqueness
**Classification:** Required behaviour
**Evidence:** Coding standards §9 requires duplicate protection and per-target progress; standard model implements these boundaries.

Pre: standard mapped durable stores retain the relevant identity. Post: no two persisted outstanding/completed representations of one logical event/target are introduced by competing normal writes.

#### PERSIST-001-AC-01 — Same target logical duplicate
**Given** two candidate entries share EventId/TargetId but have different MessageIds.
**When** both are saved against the standard retained model.
**Then** they cannot both persist as distinct representations for the same logical target.

#### PERSIST-001-AC-02 — Distinct targets
**Given** one event has different target IDs.
**When** valid entries are saved.
**Then** each target can persist independently without the event ID globally suppressing siblings.

#### PERSIST-001-AC-03 — Delivery key duplicate
**Given** two entries share MessageId/TargetId.
**When** both are saved.
**Then** the retained key does not identify two independent work records.

### PERSIST-002 — Local completion atomicity
**Classification:** Required behaviour
**Evidence:** Coding standards §8/§9 and architecture execution boundaries explicitly preserve same-context consumer changes and completion.

Pre: local inbox/outbox dispatcher, same configured scoped context and supported relational transaction. Post: committed participating effects and owned completion agree; failed completion/consumer attempts leave neither committed. Invocation itself can repeat.

#### PERSIST-002-AC-01 — Commit effect and completion
**Given** a local consumer modifies configured-context state while owning its target.
**When** processing succeeds and commits.
**Then** an independent reader observes both the business change and completed target.

#### PERSIST-002-AC-02 — Handler fails before commit
**Given** a local consumer modifies configured-context state then fails.
**When** its local attempt rolls back.
**Then** neither that business change nor successful target completion is persisted.

#### PERSIST-002-AC-03 — Stale completion
**Given** a local consumer has changed configured-context state but its ownership is no longer valid.
**When** completion is rejected.
**Then** the local changes do not commit as a successful attempt.

#### PERSIST-002-AC-04 — External effect excluded
**Given** a local consumer performs an independently committed external operation then its local save fails.
**When** the local transaction rolls back.
**Then** no claim is made that the external operation was undone; local state/completion remain unsuccessful.

### PERSIST-003 — Store cancellation and invalid claim boundaries
**Classification:** Observed behaviour
**Evidence:** Shared durable store methods forward I/O tokens and validate positive limit/lease; time arithmetic uses caller DateTime.

Pre: explicit operation token, positive claim limit/lease and representable supplied instants. Post: invalid values do not successfully claim/delete; pre-I/O cancellation is surfaced without a fabricated success. Cancellation after an uncertain commit requires subsequent observation.

**Open decisions:** [GAP-CORE-004](GAPS.md#gap-core-004).

#### PERSIST-003-AC-01 — Invalid claim
**Given** limit <=0 or lease <=0.
**When** Claim is requested.
**Then** ArgumentOutOfRangeException is raised and no successful claim is returned.

#### PERSIST-003-AC-02 — Invalid cleanup limit
**Given** cleanup limit <=0.
**When** Cleanup is requested.
**Then** ArgumentOutOfRangeException is raised rather than an unrestricted deletion.

#### PERSIST-003-AC-03 — Pre-cancelled database operation
**Given** a cancelled token before a supported asynchronous store operation reaches I/O.
**When** the operation is requested.
**Then** the operation does not report successful settlement/deletion as a replacement for cancellation.

### PERSIST-004 — Commit ambiguity and provider verification
**Classification:** Unknown behaviour
**Evidence:** Shared stores use execution-strategy verification predicates on claim presence/dead-letter state; actual provider exceptions and racing changes were not executed.

Unknown: supported providers, locking guarantees, and whether verification predicates attribute a commit to the intended attempt under races. No unconditional atomic multi-host certification can be derived from source inspection.

**Open decisions:** [GAP-CORE-005](GAPS.md#gap-core-005).

## Traceability and verification matrix
All rows are **Not tested**. Unit means isolated observable contract verification, not internal-call assertions. Persistence/concurrency verification requires a suitable real relational provider, not EF InMemory. The eventual TUnit suite can combine criteria while retaining their IDs.

| Specification | Criterion | Classification | Verification | Observable assertion | Status |
| --- | --- | --- | --- | --- | --- |
| PERSIST-001 | PERSIST-001-AC-01 | Required behaviour | Persistence integration | Same target logical duplicate | Not tested |
| PERSIST-001 | PERSIST-001-AC-02 | Required behaviour | Persistence integration | Distinct targets | Not tested |
| PERSIST-001 | PERSIST-001-AC-03 | Required behaviour | Persistence integration | Delivery key duplicate | Not tested |
| PERSIST-002 | PERSIST-002-AC-01 | Required behaviour | Persistence integration | Commit effect and completion | Not tested |
| PERSIST-002 | PERSIST-002-AC-02 | Required behaviour | Persistence integration | Handler fails before commit | Not tested |
| PERSIST-002 | PERSIST-002-AC-03 | Required behaviour | Concurrency integration | Stale completion | Not tested |
| PERSIST-002 | PERSIST-002-AC-04 | Required behaviour | Workflow integration | External effect excluded | Not tested |
| PERSIST-003 | PERSIST-003-AC-01 | Observed behaviour | Unit | Invalid claim | Not tested |
| PERSIST-003 | PERSIST-003-AC-02 | Observed behaviour | Unit | Invalid cleanup limit | Not tested |
| PERSIST-003 | PERSIST-003-AC-03 | Observed behaviour | Persistence integration | Pre-cancelled database operation | Not tested |

## Implementation trace
These locations are evidence, not mandated internal architecture. The full inspected-file/public-surface matrix is in [INSPECTION.md](INSPECTION.md).

- [src/Clean.Messaging/Persistence/IDurableEntry.cs](../../src/Clean.Messaging/Persistence/IDurableEntry.cs)
- [src/Clean.Messaging/Persistence/IDurableStore.cs](../../src/Clean.Messaging/Persistence/IDurableStore.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableEntryConfiguration.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs](../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs)

## Open decisions and evidence limits
The [gap register](GAPS.md) records unresolved input domains, provider evidence and proposed changes. Source inspection is not execution proof. [INDEX.md](INDEX.md) explains classification authority and cross-capability ownership.
