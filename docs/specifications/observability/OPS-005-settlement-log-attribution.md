# OPS-005 — Settlement log attribution

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour
**Evidence:** Processor retry/dead-letter wording is logged before conditional persistence; outcome-looking log may precede a rejected/failing update.

Unknown: whether settlement-intent and confirmed settlement should have distinct operational messages. Current metric boundary differs from logs; proposal belongs in GAPS, not silent telemetry rewrite.

**Open decisions:** [GAP-CORE-007](../GAPS.md#gap-core-007).
