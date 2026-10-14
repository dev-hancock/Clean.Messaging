# SAGA-015 — Undecided identity, schema and diagnostic hardening

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need explicit policy for default keys, persistent equality, diagnostics, collision proof, unsaved starts and retained schema evolution before relying on hardening guarantees.

The standards on valid inputs, retained identity and schema compatibility establish the obligation to expose these unresolved boundaries, not a particular proposed implementation. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Retained workflow identity MUST not be silently replaced by a different logical start.
- Concurrency Version MUST not be represented as automatic schema migration or proof of compatibility.

## Contract boundaries

The questions below do not approve ordinal persisted collation, default-key rejection, Fail validation, new collision translation or same-scope replay handling.

## Acceptance criteria

No acceptance criteria are assigned to the unresolved policies on this page. The cross-cutting guarantees above do not decide those policies.

## Open decisions

### Question SAGA-015-Q-01 ? — Default key admission policy

**Reserved — no normative acceptance scenario.** Where are default ScheduleTarget and SagaKey values rejected, and what failure is exposed?

Decision: [GAP-WF-001](../GAPS.md#gap-wf-001) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SAGA-015-Q-02 ? — Persisted equality policy

**Reserved — no normative acceptance scenario.** Is equality of persisted type/key provider-independent or part of supported database collation?

Decision: [GAP-WF-007](../GAPS.md#gap-wf-007) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SAGA-015-Q-03 ? — Fail diagnostic consistency policy

**Reserved — no normative acceptance scenario.** What admission behavior applies to null, blank and overlong SagaResult.Fail diagnostics?

Decision: [GAP-WF-008](../GAPS.md#gap-wf-008) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SAGA-015-Q-04 ? — Narrow collision translation policy

**Reserved — no normative acceptance scenario.** What evidence permits translating a save failure into same-start race, conflict or closed outcomes?

Decision: [GAP-WF-009](../GAPS.md#gap-wf-009) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SAGA-015-Q-05 ? — Unsaved repeated-start policy

**Reserved — no normative acceptance scenario.** What result applies to repeated starts for one type/key before the owning save?

Decision: [GAP-WF-012](../GAPS.md#gap-wf-012) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SAGA-015-Q-06 ? — State evolution policy

**Reserved — no normative acceptance scenario.** Which retained state/message evolution scenarios are supported, and who migrates or rejects incompatible data?

Decision: [GAP-WF-015](../GAPS.md#gap-wf-015) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
