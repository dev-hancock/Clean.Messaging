# IDENTITY-003 — Execution-local context scope

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Handlers need the current delivery's metadata across asynchronous execution without leaking it into unrelated attempts. Leaving a scope must restore its caller's context even after failure.

## Guarantees

- An active delivery scope MUST expose its message ID and supplied optional relationships/consumer; absent values and an inactive context MUST remain null.
- Normal nested scope disposal MUST restore the preceding metadata, including on exceptional exit; repeated disposal MUST NOT alter context again.
- Overlapping asynchronous flows with their own pushed scopes MUST observe their own metadata across awaits rather than overwrite each other's current delivery.

## Contract boundaries

This covers awaited execution and normal nested disposal in the originating flow. Ambient metadata is not durable state, a transaction, or automatic distributed tracing. Unsupported disposal order/cross-flow lifetime and fire-and-forget children are unresolved under IDENTITY-005; this does not claim isolation outside the normal scope contract.

## Acceptance criteria

### IDENTITY-003-AC-01 — No active scope

- **Given** a context with no active pushed scope.
- **When** its four public metadata properties are read.
- **Then** MessageId, CorrelationId, CausationId and ConsumerId are null.

### IDENTITY-003-AC-02 — Nested scope restoration

- **Given** an outer scope supplies A and a nested scope supplies B.
- **When** B is disposed normally.
- **Then** A becomes visible again; disposing A restores the state preceding A.

### IDENTITY-003-AC-03 — Repeated disposal

- **Given** an already disposed nested scope.
- **When** it is disposed again while another scope is current.
- **Then** the current metadata does not change again.

### IDENTITY-003-AC-04 — Parallel flow isolation

- **Given** two overlapping asynchronous flows each push distinct delivery metadata in their own normal scopes.
- **When** each reads context before and after awaiting.
- **Then** each sees its own metadata rather than the other flow's current values.

### IDENTITY-003-AC-05 — Optional metadata

- **Given** a scope supplies only message ID and null optional relationships/consumer.
- **When** properties are read.
- **Then** the message ID is available and omitted optional values stay null.

### IDENTITY-003-AC-06 — Exceptional handler exit

- **Given** a processor pushes delivery metadata and its handler fails.
- **When** the attempt exits and disposes its scope.
- **Then** the prior execution context is restored even though processing did not succeed.

## Open decisions

[GAP-CORE-002](../GAPS.md#gap-core-002) leaves out-of-order/cross-flow disposal and child-work lifetime unresolved.
