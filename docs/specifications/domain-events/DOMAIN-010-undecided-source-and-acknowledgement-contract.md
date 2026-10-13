# DOMAIN-010 — Undecided source and acknowledgement contract

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour

**Evidence:** No normative rule found for custom-source globally unique IDs, collection/payload stability, nonthrowing acknowledgement, duplicate-source conflict or empty-target handling. Evidence: IDomainEventSource/DomainEventCapture and [GAP-DUR-010](../GAPS.md#gap-dur-010), [GAP-DUR-011](../GAPS.md#gap-dur-011), [GAP-DUR-009](../GAPS.md#gap-dur-009).

All numbered questions are undecided, not acceptance requirements. Consumer owns domain policy; library extension contracts need a deliberate decision before imposing new validations or acknowledging zero-target facts.

**Undecided questions (not acceptance criteria):**

1. **valid custom source contract:** What approved policy should apply when custom sources can collide, mutate or throw during acknowledgment? Decision to resolve: select uniqueness/stability/acknowledgement guarantees and conflict/error visibility without silently imposing AggregateRoot-only usage.

2. **targetless commit acknowledgement:** What approved policy should apply when captured domain fact has zero outgoing targets? Decision to resolve: choose reject, retain pending, or acknowledge committed targetless capture and explicitly distinguish it from delivered downstream work.
