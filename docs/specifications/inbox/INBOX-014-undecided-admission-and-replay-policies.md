# INBOX-014 — Undecided admission and replay policies

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour

**Evidence:** No normative policy located for conflicting identity payloads, empty admission, replay target suitability, pagination, count attribution or admission expiry. Evidence: InboxStore/InboxManager and [GAP-DUR-001](../GAPS.md#gap-dur-001), [GAP-DUR-002](../GAPS.md#gap-dur-002), [GAP-DUR-003](../GAPS.md#gap-dur-003), [GAP-DUR-004](../GAPS.md#gap-dur-004).

These are numbered undecided questions, not acceptance requirements. Purpose: make consumer-visible decision gaps explicit. Proposed resolution must define invariants and failure/recovery boundaries before implementation or tests.

**Undecided questions (not acceptance criteria):**

1. **empty or conflicting admission policy:** What approved policy should apply when a new no-target delivery or reused identity with changed payload? Decision to resolve: choose rejection, retain-without-targets, or explicit conflict handling and define durable/cancellation outcomes; current observations alone do not choose.

2. **replay suitability and progress contract:** What approved policy should apply when history exceeds a page or a requested target is incompatible? Decision to resolve: choose target validation and continuation/count semantics, distinguishing this request's additions from concurrent additions.

3. **admission retention and replay horizon:** What approved policy should apply when entry completion has expired but admissions retain payload indefinitely? Decision to resolve: define payload deletion and dedup/replay horizons, including any permitted repeated effects; no existing admission cleanup guarantee is inferred.
