# SCHED-015 — Undecided integration hardening

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Integrators need explicit decisions for default values, malformed configuration, hung wake I/O, savepoints and empty outgoing target sets before relying on hardening promises.

Valid-input, transaction-ownership and provider-boundary standards establish the need to expose limitations; they do not select the competing integration policies listed here. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Scheduling descriptions MUST retain their operation identity and supported owner transaction boundary.
- Wake assistance MUST not be represented as durable payload completion.

## Contract boundaries

This page records unapproved integration policy questions, not newly promised validation, bounded Quartz latency, savepoint support or rejection of empty custom fan-out.

## Acceptance criteria

No acceptance criteria are assigned to the unresolved policies on this page. The cross-cutting guarantees above do not decide those policies.

## Open decisions

### Question SCHED-015-Q-01 ? — Default target admission policy

**Reserved — no normative acceptance scenario.** Where are default ScheduleTarget and SagaKey values rejected, and what failure is exposed?

Decision: [GAP-WF-001](../GAPS.md#gap-wf-001) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SCHED-015-Q-02 ? — Hung Quartz operation policy

**Reserved — no normative acceptance scenario.** Does a hung Quartz operation need a bounded or concurrent local fallback?

Decision: [GAP-WF-005](../GAPS.md#gap-wf-005) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SCHED-015-Q-03 ? — User savepoint policy

**Reserved — no normative acceptance scenario.** Which caller-managed savepoints and uncertain commit recovery patterns are supported across saga, schedule and outbox effects?

Decision: [GAP-WF-006](../GAPS.md#gap-wf-006) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SCHED-015-Q-04 ? — Malformed options policy

**Reserved — no normative acceptance scenario.** What failures cover null option subobjects, null serializer options and extreme otherwise-positive settings?

Decision: [GAP-WF-013](../GAPS.md#gap-wf-013) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SCHED-015-Q-05 ? — Empty custom outgoing targets policy

**Reserved — no normative acceptance scenario.** Does successful scheduling delivery require at least one outgoing target when custom providers select none?

Decision: [GAP-WF-014](../GAPS.md#gap-wf-014) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
