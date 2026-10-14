# SCHED-003 — Input descriptions, serialization and UTC boundaries

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Schedule producers and provider adapters need durable descriptions that preserve identity, payload and instants without inventing contextual metadata.

The public schedule/writer descriptions, ScheduleTarget construction contract and the standards on valid inputs, stable contracts, explicit metadata and UTC instants establish the supported boundary. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Accepted descriptions MUST preserve supplied identity, target, grouping and metadata and represent the same due instant in UTC.
- Writer commands MUST reject empty operation/group identities where supplied, and scheduling MUST not accept null or unrepresentable durable payloads.

## Contract boundaries

Past due is eligibility, not expiry. Valid constructor values do not establish validation for default structs. Schedule metadata is supplied explicitly; outgoing fallback is a later contract.

## Acceptance criteria

### SCHED-003-AC-01 — Valid descriptions preserve values

**Given** a valid payload, identity, target, optional group/metadata and an offset due instant.
**When** the description is durably saved.
**Then** the returned ScheduleId equals the supplied identity, the stored target/group/metadata are preserved and the due time represents the same instant in UTC.

### SCHED-003-AC-02 — Due time is eligibility not rejection

**Given** valid descriptions due in the past, now or in the future.
**When** they are durably accepted.
**Then** past/current due alone does not reject them and future work is not eligible before its due instant; persistence and ownership/retry conditions still apply, without an exact firing guarantee.

### SCHED-003-AC-03 — Identity guards cover all writer commands

**Given** Schedule, Cancel or CancelGroup with an empty required ID or an empty supplied group/exclusion ID.
**When** the writer admits the operation.
**Then** the invalid identity is rejected before it can become durable scheduled work.

### SCHED-003-AC-04 — Target constructor character and length boundary

**Given** ScheduleTarget inputs that are null, blank, non-ASCII, or 100/101 characters long.
**When** the target is constructed and used for delivery selection.
**Then** a nonblank 100-character ASCII value is accepted unchanged, invalid inputs are rejected, and differently cased targets are not interchangeable.

### SCHED-003-AC-05 — Null and unregistered payloads are not durable schedules

**Given** a null payload or a payload with no registered durable contract.
**When** scheduling admission is attempted.
**Then** null is rejected or the missing contract exposes contract.type.not_registered/Fault; no schedule is staged and no outgoing row is created for that payload.

### SCHED-003-AC-06 — Serializer rejects unsupported durable representation

**Given** a registered payload that cannot be represented by the configured serializer.
**When** durable scheduling is attempted.
**Then** serialization.serialize.failed is exposed as a permanent message failure and no schedule is staged.

### SCHED-003-AC-08 — Writer does not inherit execution metadata

**Given** execution metadata exists but the schedule description omits correlation and causation.
**When** the schedule is durably stored.
**Then** those schedule metadata values remain absent; any later outgoing fallback belongs to delivery, not scheduling admission.

## Open decisions

### SCHED-003-AC-07 — Identity value objects are not admission validation

**Reserved — no normative acceptance scenario.** Is acceptance/formatting of directly constructed empty identity values and empty SagaCancel an intentional compatibility contract?

Decision: [GAP-INTENT-WF-006](../GAPS.md#gap-intent-wf-006) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
