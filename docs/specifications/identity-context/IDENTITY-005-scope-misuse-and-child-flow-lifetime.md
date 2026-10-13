# IDENTITY-005 — Scope misuse and child-flow lifetime

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour
**Evidence:** AsyncLocal flows into child work, and disposal restores the captured previous state without checking nesting or originating flow.

Unknown: whether out-of-order disposal, disposal in another flow, or fire-and-forget child handlers are supported usage. No isolation claim applies outside the normal scoped flow.

**Open decisions:** [GAP-CORE-002](../GAPS.md#gap-core-002).
