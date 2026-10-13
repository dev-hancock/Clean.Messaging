# Message identity and execution context — Behavioural specification

**Status:** Draft; source reviewed 2026-10-13; no acceptance criterion has been executed.
**Scope:** `EntryKey`, `IMessageContext`, `MessageContext.Push` and disposable scope; `ClaimId.Create`; envelope identities and metadata are specified in inbox/outbox/domain/scheduling/saga/transport pages.

## Purpose
Separate logical operations, deliveries, targets, related operations and temporary ownership, while exposing execution-local metadata to handlers.

## Contracts, preconditions and postconditions
Pre: use identities appropriate to the initiating API and dispose metadata scopes in nesting order. Post: handlers see the selected delivery metadata and follow-on operations use the capability-specific propagation rules. Correlation and causation are optional relationships, not uniqueness constraints.

## Invariants
- Retries of stored work preserve logical, delivery and target identity.
- A target's success is independent of sibling failure.
- Ownership tokens describe attempts, not business identity.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are counted separately as gaps, not executable requirements.

## Limitations
- No exactly-once external effect, universal identifier meaning for every property called Id, automatic distributed tracing, or global ordering.
- Cross-context disposal and out-of-order scopes are unresolved; execution-local state can flow into child async work and is not durable.

## Specifications and acceptance criteria

### IDENTITY-001 — Stable identity across durable retries
**Classification:** Required behaviour
**Evidence:** Coding standards §9 explicitly requires stable logical identity, distinct EventId/MessageId, and independent target completion.

Pre: retained admitted/staged work undergoing later attempts. Post: the same logical operation and target remain identifiable even if claim ownership changes.

#### IDENTITY-001-AC-01 — Retry identity
**Given** an unresolved durable target with message, event and target IDs.
**When** it fails, becomes eligible and is reclaimed.
**Then** the same message/event/target IDs remain associated with it while a new ownership attempt is represented.

#### IDENTITY-001-AC-02 — Independent target completion
**Given** one logical operation has two targets and the first has committed completion.
**When** the second fails and retries.
**Then** the first is not reset or deliberately rerun merely to retry its sibling.

### IDENTITY-002 — Relationship metadata is not deduplication
**Classification:** Required behaviour
**Evidence:** Coding standards §9 separates correlation/causation from logical identities; repository instructions require their preservation.

Pre: two distinct operations may share correlation; optional causation describes a preceding delivery. Post: preserving these values does not coalesce distinct operations.

#### IDENTITY-002-AC-01 — Shared correlation
**Given** two distinct operations share one correlation ID but have distinct event identities.
**When** both are admitted or staged for a target.
**Then** correlation alone does not suppress either operation.

#### IDENTITY-002-AC-02 — Causation distinction
**Given** an outgoing operation is caused by a current incoming delivery.
**When** the applicable API inherits causation.
**Then** causation represents the causing delivery ID, not an inferred event ID or correlation key.

### IDENTITY-003 — Execution-local context scope
**Classification:** Observed behaviour
**Evidence:** MessageContext properties, Push and idempotent disposal; XML summary establishes ambient delivery metadata but not all scope lifetime edge cases.

Pre: scopes used in LIFO order in their originating execution flow. Post: current values are visible while active; disposal restores previous values. This is not a transaction or durable state record.

#### IDENTITY-003-AC-01 — No active scope
**Given** a context with no active pushed scope.
**When** its four public metadata properties are read.
**Then** MessageId, CorrelationId, CausationId and ConsumerId are null.

#### IDENTITY-003-AC-02 — Nested scope restoration
**Given** an outer scope supplies A and a nested scope supplies B.
**When** B is disposed normally.
**Then** A becomes visible again; disposing A restores the state preceding A.

#### IDENTITY-003-AC-03 — Repeated disposal
**Given** an already disposed nested scope.
**When** it is disposed again while another scope is current.
**Then** the current metadata does not change again.

#### IDENTITY-003-AC-04 — Parallel flow isolation
**Given** two separately started asynchronous flows push different metadata and overlap.
**When** each reads the context before and after awaiting.
**Then** each observes its own pushed state rather than the other flow's values.

#### IDENTITY-003-AC-05 — Optional metadata
**Given** a scope supplies only message ID and null optional relationships/consumer.
**When** properties are read.
**Then** the message ID is available and omitted optional values stay null.

#### IDENTITY-003-AC-06 — Exceptional handler exit
**Given** a processor pushes delivery metadata and its handler fails.
**When** the attempt exits and disposes its scope.
**Then** the prior execution context is restored even though processing did not succeed.

### IDENTITY-004 — Opaque claim token representation
**Classification:** Observed behaviour
**Evidence:** ClaimId.Create generates GUID N strings; IDurableStore consumes a string ownership token.

Pre: use the supplied claim-token factory. Post: an opaque 32-character GUID representation is returned, without encoding event identity or promising cryptographic authentication.

#### IDENTITY-004-AC-01 — Usable token
**Given** the public claim-token factory.
**When** a token is created.
**Then** it is a 32-character N-format GUID string accepted by the standard claim field representation.

### IDENTITY-005 — Scope misuse and child-flow lifetime
**Classification:** Unknown behaviour
**Evidence:** AsyncLocal flows into child work, and disposal restores the captured previous state without checking nesting or originating flow.

Unknown: whether out-of-order disposal, disposal in another flow, or fire-and-forget child handlers are supported usage. No isolation claim applies outside the normal scoped flow.

**Open decisions:** [GAP-CORE-002](GAPS.md#gap-core-002).

## Traceability and verification matrix
All rows are **Not tested**. Unit means isolated observable contract verification, not internal-call assertions. Persistence/concurrency verification requires a suitable real relational provider, not EF InMemory. The eventual TUnit suite can combine criteria while retaining their IDs.

| Specification | Criterion | Classification | Verification | Observable assertion | Status |
| --- | --- | --- | --- | --- | --- |
| IDENTITY-001 | IDENTITY-001-AC-01 | Required behaviour | Persistence integration | Retry identity | Not tested |
| IDENTITY-001 | IDENTITY-001-AC-02 | Required behaviour | Workflow integration | Independent target completion | Not tested |
| IDENTITY-002 | IDENTITY-002-AC-01 | Required behaviour | Persistence integration | Shared correlation | Not tested |
| IDENTITY-002 | IDENTITY-002-AC-02 | Required behaviour | Workflow integration | Causation distinction | Not tested |
| IDENTITY-003 | IDENTITY-003-AC-01 | Observed behaviour | Unit | No active scope | Not tested |
| IDENTITY-003 | IDENTITY-003-AC-02 | Observed behaviour | Unit | Nested scope restoration | Not tested |
| IDENTITY-003 | IDENTITY-003-AC-03 | Observed behaviour | Unit | Repeated disposal | Not tested |
| IDENTITY-003 | IDENTITY-003-AC-04 | Observed behaviour | Concurrency integration | Parallel flow isolation | Not tested |
| IDENTITY-003 | IDENTITY-003-AC-05 | Observed behaviour | Unit | Optional metadata | Not tested |
| IDENTITY-003 | IDENTITY-003-AC-06 | Observed behaviour | Workflow integration | Exceptional handler exit | Not tested |
| IDENTITY-004 | IDENTITY-004-AC-01 | Observed behaviour | Unit | Usable token | Not tested |

## Implementation trace
These locations are evidence, not mandated internal architecture. The full inspected-file/public-surface matrix is in [INSPECTION.md](INSPECTION.md).

- [src/Clean.Messaging.Abstractions/EntryKey.cs](../../src/Clean.Messaging.Abstractions/EntryKey.cs)
- [src/Clean.Messaging.Abstractions/IMessageContext.cs](../../src/Clean.Messaging.Abstractions/IMessageContext.cs)
- [src/Clean.Messaging/MessageContext.cs](../../src/Clean.Messaging/MessageContext.cs)
- [src/Clean.Messaging/ClaimId.cs](../../src/Clean.Messaging/ClaimId.cs)

## Open decisions and evidence limits
The [gap register](GAPS.md) records unresolved input domains, provider evidence and proposed changes. Source inspection is not execution proof. [INDEX.md](INDEX.md) explains classification authority and cross-capability ownership.
