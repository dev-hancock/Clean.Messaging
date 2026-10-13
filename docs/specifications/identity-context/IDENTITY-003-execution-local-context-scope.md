# IDENTITY-003 — Execution-local context scope

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** MessageContext properties, Push and idempotent disposal; XML summary establishes ambient delivery metadata but not all scope lifetime edge cases.

Pre: scopes used in LIFO order in their originating execution flow. Post: current values are visible while active; disposal restores previous values. This is not a transaction or durable state record.

## IDENTITY-003-AC-01 — No active scope
**Given** a context with no active pushed scope.
**When** its four public metadata properties are read.
**Then** MessageId, CorrelationId, CausationId and ConsumerId are null.

## IDENTITY-003-AC-02 — Nested scope restoration
**Given** an outer scope supplies A and a nested scope supplies B.
**When** B is disposed normally.
**Then** A becomes visible again; disposing A restores the state preceding A.

## IDENTITY-003-AC-03 — Repeated disposal
**Given** an already disposed nested scope.
**When** it is disposed again while another scope is current.
**Then** the current metadata does not change again.

## IDENTITY-003-AC-04 — Parallel flow isolation
**Given** two separately started asynchronous flows push different metadata and overlap.
**When** each reads the context before and after awaiting.
**Then** each observes its own pushed state rather than the other flow's values.

## IDENTITY-003-AC-05 — Optional metadata
**Given** a scope supplies only message ID and null optional relationships/consumer.
**When** properties are read.
**Then** the message ID is available and omitted optional values stay null.

## IDENTITY-003-AC-06 — Exceptional handler exit
**Given** a processor pushes delivery metadata and its handler fails.
**When** the attempt exits and disposes its scope.
**Then** the prior execution context is restored even though processing did not succeed.
