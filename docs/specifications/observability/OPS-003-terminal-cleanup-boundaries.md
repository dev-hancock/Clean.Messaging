# OPS-003 — Terminal cleanup boundaries

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Retention must remove only eligible terminal evidence within a bounded pass, without losing unresolved work simply because it is old. Operators also need to distinguish deleted target-completion evidence from independently retained inbox admission.

## Guarantees

- Cleanup MUST preserve unresolved pending, retrying or claimed entries regardless of creation age.
- A cleanup pass MUST delete no more than its positive limit and MUST NOT delete terminal entries whose relevant terminal timestamps are at or after its before cutoff.
- Removing inbox target entries MUST NOT itself remove their separately retained admission; duplicate acceptance and explicit missing-target replay MUST remain distinct operations.

## Contract boundaries

These guarantees concern standard durable-entry cleanup and retained admissions, not an admission expiry policy. Retention removes per-target duplicate/completion evidence and cannot undo external effects. This specification does not choose which terminal timestamp governs a multiply terminal discarded row; that retention-clock policy is reserved below.

## Acceptance criteria

### OPS-003-AC-01 — Strict cutoff

- **Given** terminal entries have terminal timestamps older than, equal to and newer than the cutoff.
- **When** Cleanup runs.
- **Then** only strictly older qualifying terminal entries can be deleted; equality and later timestamps remain protected.

### OPS-003-AC-02 — Pending protection

- **Given** an unresolved pending, retrying or claimed entry has very old CreatedAt.
- **When** Cleanup runs.
- **Then** creation age alone does not authorize deleting that entry.

### OPS-003-AC-03 — Bounded deletion

- **Given** more terminal entries qualify than the positive cleanup limit.
- **When** one cleanup pass runs.
- **Then** its reported deletion count is at most the limit, without promising to drain all eligible entries.

### OPS-003-AC-05 — Evidence removal

- **Given** a completed inbox target is cleaned while its admission remains retained.
- **When** ordinary duplicate Accept and explicit replay are attempted.
- **Then** retained admission still protects duplicate acceptance while replay can recreate missing target work; target cleanup did not imply admission deletion.

## Open decisions

These original identifiers and heading anchors remain reserved, not normative scenarios. No replacement IDs are assigned before the underlying policy and identity migration are decided.

### OPS-003-AC-04 — Discard age

Reserved — [GAP-INTENT-LIFE-001](../GAPS.md#gap-intent-life-001). Which terminal timestamp governs retention after discard is undecided. The original earlier-dead-letter deletion permission remains reserved.

- [GAP-DUR-004](../GAPS.md#gap-dur-004): the related policy remains undecided; its recommendation is not approved.
