# RETRY-006 — Out-of-domain public policy inputs

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour
**Evidence:** Evaluate does not explicitly reject null exceptions or negative/zero attempts; Backoff validation omits enum membership.

Unknown: public Evaluate preconditions beyond runtime-supplied positive attempts. Startup enum rejection is an unapproved CONFIG-006 proposal; do not freeze negative delay behavior as a valid requirement.

**Open decisions:** [GAP-CORE-006](../GAPS.md#gap-core-006), [GAP-CORE-003](../GAPS.md#gap-core-003).
