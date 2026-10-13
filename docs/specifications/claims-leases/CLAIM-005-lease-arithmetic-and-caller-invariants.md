# CLAIM-005 — Lease arithmetic and caller invariants

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour
**Evidence:** Renew accepts an arbitrary claimedUntil; store DateTime.Kind and overflow are not explicitly validated.

Unknown: whether shrinking leases, past expiry, non-UTC instants, overflowing times or empty claim strings are valid provider-level inputs. Normal runtime supplies UTC/later expiry, but a public port contract needs a deliberate decision.

**Open decisions:** [GAP-CORE-004](../GAPS.md#gap-core-004).
