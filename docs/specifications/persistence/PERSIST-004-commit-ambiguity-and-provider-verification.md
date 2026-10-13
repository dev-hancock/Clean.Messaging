# PERSIST-004 — Commit ambiguity and provider verification

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour
**Evidence:** Shared stores use execution-strategy verification predicates on claim presence/dead-letter state; actual provider exceptions and racing changes were not executed.

Unknown: supported providers, locking guarantees, and whether verification predicates attribute a commit to the intended attempt under races. No unconditional atomic multi-host certification can be derived from source inspection.

**Open decisions:** [GAP-CORE-005](../GAPS.md#gap-core-005).
