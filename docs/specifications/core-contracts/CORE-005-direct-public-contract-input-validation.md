# CORE-005 — Direct public contract input validation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour
**Evidence:** Public MessageContract and custom IConsumerInvoker values can bypass AddContract/AddConsumer checks; malformed-data behavior is not uniformly guarded.

Unknown: whether public registration records and provider-created invokers must independently enforce the extension APIs' constraints. Current validation bypass must not become an approved alternate contract.

**Open decisions:** [GAP-CORE-001](../GAPS.md#gap-core-001).
