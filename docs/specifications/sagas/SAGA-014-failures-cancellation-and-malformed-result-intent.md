# SAGA-014 — Failures, cancellation and malformed result intent

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workflow authors and hosts need to distinguish business Fail diagnostics and cancellation precedence from the already-compatible public saga failure surface.

The exception-compatibility and explicit outcome standards protect public missing/conflict/closed/serialization behavior; they do not establish malformed-result validation or lookup cancellation precedence. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Business Fail outcomes MUST remain distinguishable from processing exceptions and cancellation.
- Public saga recovery classifications MUST not be changed merely to settle malformed diagnostic or collision-lookup policy.

## Contract boundaries

Valid business Fail is covered by SAGA-006 and public exceptions by SAGA-013. This page reserves the raw-diagnostic disparity and competing failure/cancellation outcome; neither proposal is approved.

## Acceptance criteria

No acceptance criteria are assigned to the unresolved policies on this page. The cross-cutting guarantees above do not decide those policies.

## Open decisions

### Question SAGA-014-Q-01 ? Validated diagnostic value versus raw Fail result

**Reserved — no normative acceptance scenario.** What admission behavior applies to null, blank and overlong SagaResult.Fail diagnostics?

Decision: [GAP-WF-008](../GAPS.md#gap-wf-008) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SAGA-014-Q-02 ? Cancelled conflict lookup preserves save error today

**Reserved — no normative acceptance scenario.** Should cancellation during winner lookup replace or preserve the original database save failure?

Decision: [GAP-WF-010](../GAPS.md#gap-wf-010) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
