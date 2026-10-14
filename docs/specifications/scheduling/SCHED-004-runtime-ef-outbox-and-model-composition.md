# SCHED-004 — Runtime, EF, outbox and model composition

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Host integrators need explicit runtime, persistence and delivery composition so startup cannot be confused with installing a database or completing a delivery.

The separate public runtime/EF/Outbox registration surfaces and the standards on scoped composition, provider isolation and host-owned schema establish this boundary. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- A durable EF composition MUST have a relational scoped context, schedule mapping, required hooks and resolvable persistence/execution services.
- Delivery registration MUST identify one delivery per exact target; equivalent options overloads MUST install equivalent integration without duplicating the same hook instance.

## Contract boundaries

Runtime and Outbox bridge registration do not create persistence or schema. Hosts own migrations and same-context composition. Unknown retained target failure classification is undecided, but a missing delivery cannot demonstrate successful dispatch.

## Acceptance criteria

### SCHED-004-AC-01 — Runtime registration is not durable installation

**Given** runtime-only scheduling registration.
**When** the host assesses durable installation.
**Then** registration alone does not supply the durable provider, executor/worker or IMessageScheduler; those capability choices remain explicit.

### SCHED-004-AC-02 — Outbox adapter does not install outbox persistence

**Given** Outbox scheduling bridge registration lacks its outgoing writer, scheduler or delivery dependencies.
**When** the host validates a usable bridge composition.
**Then** the missing dependency prevents valid startup rather than silently installing persistence or treating registration as ready durable publication.

### SCHED-004-AC-03 — EF composition requires relational configured context

**Given** an EF scheduling composition lacks relational use, mapped schedules or required save/transaction integration.
**When** startup validation runs.
**Then** the composition is rejected; passing validation does not create schema or validate a deployment provider.

### SCHED-004-AC-04 — Typed and untyped options install equivalent integration

**Given** the same services are applied through typed or untyped EF options overloads.
**When** the host installs scheduling integration, including repeating attachment of the same hook instances.
**Then** both overloads provide equivalent behavior and the identical hook instance is not attached twice.

### SCHED-004-AC-05 — Target registration is exact and unique

**Given** deliveries claiming equal or differently cased target strings.
**When** the delivery composition is validated.
**Then** equal exact targets cannot ambiguously select two deliveries; differently cased valid targets remain distinct.

## Open decisions

### SCHED-004-AC-06 — Unknown retained target faults when encountered

**Reserved — no normative acceptance scenario.** Is scheduling.delivery.target_not_registered/Fault the deliberate public recovery outcome for a retained unknown target?

Decision: [GAP-INTENT-WF-007](../GAPS.md#gap-intent-wf-007) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
