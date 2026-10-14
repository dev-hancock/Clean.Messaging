# SAGA-002 — Definitions, handler overloads and registration validation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workflow hosts need stable definitions and explicit ordinary-versus-instance-directed message mappings so retained work reaches a valid scoped handler.

Public SagaBuilder/On/ISagaHandler contracts and the standards on stable type identities, contracts and scoped composition establish this registration contract. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Definitions MUST have valid stable type identity, configuration and at least one unique message mapping.
- Start/continuation mappings MUST use explicit correlation and scoped configured handlers; instance-directed mappings MUST not invent ordinary correlation.

## Contract boundaries

The application owns state, correlation, message meaning and consumer identity. All mapped messages, including timers, require durable contracts. Registration does not create ingress or cross-context consistency.

## Acceptance criteria

### SAGA-002-AC-01 — Definitions require valid type and callbacks

**Given** a definition has a null callback, blank/non-ASCII/over-200-character type, duplicate type/state registration, or no mapped messages.
**When** definitions are registered.
**Then** invalid/incomplete definitions are rejected; a valid configured unique definition with a nonblank ASCII type of 200 characters is accepted unchanged, while 201 characters is rejected.

### SAGA-002-AC-02 — Message mapping is unique per definition

**Given** one definition attempts two mappings for the same message CLR type.
**When** mapping is registered.
**Then** the duplicate is rejected rather than ambiguously choosing a handler; a distinct valid message type remains registrable.

### SAGA-002-AC-03 — Start and continuation overloads install consumers

**Given** valid start and continuation mappings with consumer identity and correlation, and creation for the start.
**When** ordinary durable delivery reaches those consumers.
**Then** only the start mapping can create initial state and the continuation targets existing state; required delegates reject null and consumer IDs obey global nonblank ASCII, 200-character and uniqueness rules. Both use the normal consumer delivery boundary.

### SAGA-002-AC-04 — Timer-only mapping is instance-directed

**Given** a no-argument On mapping.
**When** an instance-directed timer reaches its handler.
**Then** the supplied instance selects the workflow without ordinary correlation or a new consumer registration for that mapping.

### SAGA-002-AC-05 — All mappings require retained contracts

**Given** an ordinary or timer-only mapping has no registered durable message contract.
**When** the runtime composition is validated.
**Then** the missing contract is rejected rather than accepted as interpretable retained work.

### SAGA-002-AC-06 — Registered handler is scope-resolved

**Given** a configured saga handler has scoped dependencies.
**When** durable processing invokes its mapping.
**Then** the configured handler service is resolved in the processing scope, not captured as a host-global stateful handler; registration does not make shared handler/context state thread-safe.

## Open decisions

None.
