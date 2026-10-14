# SCHED-005 — Options defaults, boundaries and binding

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need predictable initial settings and valid resource bounds while retaining control of host configuration.

Public SchedulingOptions and its section/default values provide deliberate configuration compatibility; bounded-resource and explicit host-composition standards justify validation. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Default options MUST expose the documented initial lease, concurrency, recovery, retry and cleanup settings.
- Configured positive-only bounds MUST reject zero/negative values; retry delay MUST be nonnegative with a cap at least the initial delay.

## Contract boundaries

Section names do not bind configuration. These are initial settings, not a promise of hot reload or a hard handler-invocation budget. Null subobjects and extreme values remain undecided.

## Acceptance criteria

### SCHED-005-AC-01 — Default policy is observable without binding

**Given** fresh options without host configuration binding.
**When** their values are read.
**Then** lease is 1 minute; concurrency 4; recovery interval 30 seconds and batch 32; retry maximum 5, delay 2 seconds and cap 1 minute; cleanup retention 30 days, interval 1 hour, batch 500 and run cap 20.

### SCHED-005-AC-02 — Positive-only bounds reject zero and negatives

**Given** zero or negative lease duration, concurrency, recovery interval/batch, retry maximum, or cleanup retention/interval/batch/run cap.
**When** configuration is validated.
**Then** each invalid positive-only setting is rejected rather than admitting unbounded or nonprogressing work; valid positive boundary settings are accepted.

### SCHED-005-AC-03 — Retry delay zero and equal cap are valid

**Given** retry delay/cap values at zero, equal, negative or cap below initial delay.
**When** configuration is validated.
**Then** zero initial delay and an equal cap are valid; a negative delay or smaller cap is rejected.

### SCHED-005-AC-04 — Configuration section name does not bind itself

**Given** configuration exists under Messaging:Scheduling but no binding is supplied.
**When** the host registers scheduling.
**Then** the section constant alone does not bind those values; explicit host configuration determines overrides.

## Open decisions

Separate boundary questions remain unapproved in the [decision register](../GAPS.md): `GAP-WF-013`. These do not suspend the independently justified guarantees above.
