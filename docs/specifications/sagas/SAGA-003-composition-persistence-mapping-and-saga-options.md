# SAGA-003 — Composition, persistence mapping and saga options

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need explicit saga persistence/effect/timer composition and predictable options without assuming runtime registration provisions storage.

Public Saga/EF registration and SagaOptions compatibility plus the standards on provider separation, explicit host binding and positive resource bounds establish this boundary. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- A usable saga composition MUST supply its store, outgoing writer, scheduling integration and configured definitions.
- EF options/model integration MUST include saga and timer requirements; default options and positive cleanup bounds MUST remain compatible.

## Contract boundaries

Hosts own relational schema, migrations and same-context Outbox composition. Section constants are not automatic binding. Null serializer options/extreme values remain open and no saga retry/concurrency policy is introduced.

## Acceptance criteria

### SAGA-003-AC-01 — Runtime composition is incomplete without stores/effects

**Given** runtime saga registration lacks durable stores or outgoing/timer dependencies.
**When** a usable host composition starts.
**Then** missing dependencies are exposed rather than treating registration as complete durable installation.

### SAGA-003-AC-02 — EF model and interceptors include timers

**Given** typed or untyped EF model/options saga integration is installed.
**When** the host prepares the scoped relational context.
**Then** saga mapping/hooks and scheduling timer integration are included equivalently, without duplicating the same hook instances; deficient model/interceptors or nonrelational use fail validation. Outbox persistence remains separately composed.

### SAGA-003-AC-03 — Defaults and explicit binding

**Given** fresh SagaOptions and a configuration section without explicit binding.
**When** initial settings are read.
**Then** state JSON uses Web defaults separately from message serialization; retention is 30 days, cleanup interval 1 hour and batch 500; Messaging:Sagas does not bind itself.

### SAGA-003-AC-04 — Cleanup numeric boundaries

**Given** cleanup retention, interval or batch is zero or negative.
**When** saga options are validated.
**Then** each invalid positive-only setting is rejected and supported positive values are accepted; no saga-specific retry, lease or execution-concurrency option is inferred from these cleanup settings.

## Open decisions

Separate boundary questions remain unapproved in the [decision register](../GAPS.md): `GAP-WF-013`. These do not suspend the independently justified guarantees above.
