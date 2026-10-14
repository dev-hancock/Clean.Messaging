# SAGA-001 — Workflow identity and local outgoing consistency

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workflow authors need durable local state and effect intentions that can recover together while logical effect identities survive repeated execution.

The coding standards explicitly require saga identity, same-context state/outgoing atomicity and application-owned external/compensation policy. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Repeated logical sends/timers MUST preserve their effect identities; distinct effects MUST use distinct identities.
- Participating saga state, outgoing intentions, timer changes and owning delivery completion MUST commit atomically; cancellation MUST remain distinct from a business Fail result.

## Contract boundaries

The host supplies one scoped context/transaction. Direct I/O, another context and distributed compensation are outside rollback; effects may execute again externally.

## Acceptance criteria

### SAGA-001-AC-01 — Effect identity survives retry

**Given** a logical send or timer effect is reproduced during owner recovery.
**When** the workflow describes that effect again.
**Then** its nonempty logical identity is retained; a genuinely distinct effect receives a distinct identity.

### SAGA-001-AC-02 — Local state and durable intentions are atomic

**Given** saga state, outgoing/timer changes and owner completion share one scoped transaction.
**When** the local workflow transition commits.
**Then** all those participating changes become durable together; a known rollback leaves none as a successful local transition.

### SAGA-001-AC-03 — External effects remain outside rollback guarantee

**Given** a handler performs direct external work outside the owning database transaction.
**When** local persistence later fails or replays.
**Then** rollback does not revoke that external work and the library does not guarantee exactly-once effects or automatic compensation.

### SAGA-001-AC-04 — Handler cancellation has distinct owner meaning

**Given** handler execution observes cancellation of the owner token.
**When** the attempt aborts.
**Then** cancellation is not converted to a business Fail outcome; no rolled-back candidate state/effects are represented as committed.

## Open decisions

None.
