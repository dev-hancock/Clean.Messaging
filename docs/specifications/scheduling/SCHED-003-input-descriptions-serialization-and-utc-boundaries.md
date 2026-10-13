# SCHED-003 — Input descriptions, serialization and UTC boundaries

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** IMessageScheduler.Schedule returns the caller ScheduleId. IScheduleWriter.Schedule adds target, nullable group and metadata; cancellation ports take explicit target/group/except identity. ScheduleId and ScheduleGroupId expose Value, New and N-format text; ScheduleTarget exposes Value, Message and preserved text.

**Evidence:** ScheduleId.cs; ScheduleGroupId.cs; ScheduleTarget.cs; Runtime/ScheduleWriter.cs; OutboxMessageScheduler.cs; shared IMessageSerializer.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SCHED-003-AC-01 — Valid descriptions preserve values

**Given** nonempty IDs, an ASCII target, a registered concrete payload and an offset due time.
**When** the writer stages the operation.
**Then** the returned identity matches the supplied identity, stored due time is the equivalent UTC instant, and group, target and supplied metadata are preserved.

## SCHED-003-AC-02 — Due time is eligibility not rejection

**Given** valid payloads with past, current and future due times.
**When** each is staged.
**Then** none is rejected merely for being past/current; future work remains ineligible before its instant.

## SCHED-003-AC-03 — Identity guards cover all writer commands

**Given** an empty schedule ID, empty supplied group ID, or empty supplied except ID.
**When** the corresponding Schedule, Cancel or CancelGroup command is used.
**Then** invalid identity is rejected before a durable schedule mutation.

## SCHED-003-AC-04 — Target constructor character and length boundary

**Given** blank/null strings, 100 ASCII characters, 101 ASCII characters and a non-ASCII character.
**When** ScheduleTarget is constructed.
**Then** 100 nonblank ASCII characters are accepted unchanged; blank/null, overlong and non-ASCII inputs are rejected; lookup remains case-sensitive.

## SCHED-003-AC-05 — Null and unregistered payloads are not durable schedules

**Given** a null payload or a payload with no durable contract.
**When** Schedule is used.
**Then** null is rejected or contract.type.not_registered faults; no outgoing row or durable schedule is created.

## SCHED-003-AC-06 — Serializer rejects unsupported durable representation

**Given** a registered payload whose JSON serializer throws JsonException or NotSupportedException.
**When** Schedule serializes it.
**Then** serialization.serialize.failed is a permanent message failure and no schedule is staged.

## SCHED-003-AC-07 — Identity value objects are not admission validation

**Given** Guid.Empty and newly generated GUID values.
**When** identity objects are constructed and formatted.
**Then** constructors retain even empty values, New produces a nonempty identity, and schedule/group text is the GUID N representation; writer guards remain a separate boundary.

## SCHED-003-AC-08 — Writer does not inherit execution metadata

**Given** a message context with metadata and a Schedule call omitting metadata.
**When** the schedule is saved.
**Then** stored schedule correlation and causation remain null; eventual outgoing fallback belongs to the delivery/outbox boundary.
