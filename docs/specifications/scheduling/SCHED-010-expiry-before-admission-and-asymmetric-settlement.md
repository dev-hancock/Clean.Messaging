# SCHED-010 — Expiry before admission and asymmetric settlement

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Host operators need a deliberate lease policy for queued and long-running schedules before relying on expiry as a settlement boundary.

The temporary-ownership and stale-token standards establish fencing, but do not independently select renewal, queue leasing or exact post-expiry settlement policy. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- A replaced ownership token MUST not authorize local settlement.
- Terminal schedule state MUST not be reopened by an expired attempt.

## Contract boundaries

These invariants do not choose whether expiry alone revokes a still-current token, or whether admission permits completion after expiry. No renewal, final deadline check or exactly-once external effect is implied.

## Acceptance criteria

No acceptance criteria are assigned to the unresolved policies on this page. The cross-cutting guarantees above do not decide those policies.

## Open decisions

### Question SCHED-010-Q-01 ? Queued claim outlives lease

**Reserved — no normative acceptance scenario.** How does ownership remain usable while claimed work waits for capacity or execution outlasts a lease?

Decision: [GAP-WF-003](../GAPS.md#gap-wf-003) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SCHED-010-Q-02 ? Old unreclaimed token can record failure after expiry

**Reserved — no normative acceptance scenario.** What expiry rule governs admission, successful completion and retry/failure settlement?

Decision: [GAP-WF-004](../GAPS.md#gap-wf-004) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SCHED-010-Q-03 ? Accepted dispatch may finish past its lease

**Reserved — no normative acceptance scenario.** What expiry rule governs admission, successful completion and retry/failure settlement?

Decision: [GAP-WF-004](../GAPS.md#gap-wf-004) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### Question SCHED-010-Q-04 ? Decide renewal and queue exposure

**Reserved — no normative acceptance scenario.** How does ownership remain usable while claimed work waits for capacity or execution outlasts a lease?

Decision: [GAP-WF-003](../GAPS.md#gap-wf-003) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
