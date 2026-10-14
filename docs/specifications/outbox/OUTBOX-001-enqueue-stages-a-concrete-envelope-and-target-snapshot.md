# OUTBOX-001 — Enqueue stages a concrete envelope and target snapshot

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications need to stage the intent to send one logical operation to independent targets while preserving its concrete envelope and route snapshot. Staging must be distinguishable from durable capture and later delivery.

## Guarantees

- Enqueue MUST stage work without implying save, commit, transport publication or target completion.
- The caller's nonempty Id MUST remain EventId; each staging call MUST receive a fresh nonempty delivery MessageId shared by its targets.
- Targets staged for one call MUST share the concrete registered payload snapshot and UTC creation time.
- Invalid null envelope/payload or empty logical identity MUST fail before valid work is staged; contract lookup or serialization failure MUST be visible.
- Routing MUST select from the original concrete message and preserve the selected target, transport and destination snapshot for later dispatch.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Public Enqueue/envelope compatibility and coding standards sections 9 and 14 establish stable logical identity, distinct delivery batches, concrete persisted contracts and per-target routing identity.

## Contract boundaries

The caller owns stable logical identity and the owning save/transaction; Enqueue is a synchronous staging boundary, not autonomous persistence. Target policy is separately specified and targetless behavior is unresolved. Dispatch of retained work does not recompute routing from later configuration. Atomic capture requires [OUTBOX-005](OUTBOX-005-atomic-owning-save-and-commit-boundary.md).

## Acceptance criteria

### OUTBOX-001-AC-01 — Do not confuse staging with persistence or delivery

- **Given** a valid outgoing message has selected targets.
- **When** Enqueue returns before any owning save.
- **Then** target work is staged, without a promise of database commit, publication or completion.

### OUTBOX-001-AC-02 — Preserve logical identity with new delivery identity

- **Given** a valid logical Id has multiple selected targets from a concrete registered payload.
- **When** the work is staged and its configured owning save commits, and another staging call is made for the same logical operation.
- **Then** the committed targets share a fresh nonempty MessageId distinct from the caller EventId, the concrete serialized contract/payload and one UTC creation time; another staging call has a different delivery ID without guaranteeing that a duplicate owning save succeeds.

### OUTBOX-001-AC-03 — Reject invalid envelope and serialization

- **Given** the envelope/payload is null, logical Id is empty, contract is missing or serialization fails.
- **When** staging is requested.
- **Then** invalid envelope and serialization/contract failures are observable without a successfully staged batch; constructing the envelope representation alone is not boundary validation.

### OUTBOX-001-AC-04 — Snapshot targets before storage

- **Given** routing is selected from an original concrete message and configuration later changes.
- **When** its staged target is persisted and dispatched.
- **Then** the persisted target/transport/destination snapshot remains the delivery instruction rather than rerouting the operation.

## Open decisions

None.
