# INBOX-010 — Terminal cleanup and admission-retention boundary

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need bounded removal of old terminal processing evidence without deleting unresolved work. Applications must be able to understand what duplicate and replay protection remains after evidence expires.

## Guarantees

- Cleanup MUST remove at most the positive requested limit of terminal entries whose applicable terminal timestamp is strictly before the supplied cutoff.
- Pending work and terminal evidence at or after the cutoff MUST survive that cleanup call.
- Periodic cleanup failure MUST be reported and allow a later tick to retry; failure or shutdown MUST NOT be reported as successful deletion.
- Duplicate and replay protection MUST be described in terms of the evidence still retained, not as unlimited protection after its deletion.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Public cleanup(before, limit) and configured retention compatibility, with coding standards sections 7 and 9 for bounded cleanup, pending-work preservation and explicit retention-limited duplicate protection.

## Contract boundaries

Target entries and original admissions have different lifecycles. This contract does not approve indefinite admission payload storage or a new same-target replay policy. Once completion evidence is deleted it cannot by itself prevent repeated effects; removing admission evidence also changes admission-level recognition and historical availability. There is no guaranteed completion/cleanup order under concurrency.

## Acceptance criteria

### INBOX-010-AC-01 — Respect terminal and strict cutoff retention

- **Given** pending rows and terminal rows older than, equal to or newer than the cutoff exist.
- **When** cleanup completes with a positive limit.
- **Then** at most the limit of terminal rows strictly older than the cutoff are deleted, while pending and equality/newer rows remain.

### INBOX-010-AC-03 — Retry periodic cleanup without successful deletion claim

- **Given** periodic cleanup fails while the host is running, or host shutdown cancels cleanup.
- **When** the cleanup cycle ends.
- **Then** ordinary failure is warned and later ticks can retry, while shutdown ends cooperatively and neither path claims successful deletion.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### INBOX-010-AC-02 — Retain admission while replay evidence expires

Reserved pending [GAP-DUR-004](../GAPS.md#gap-dur-004). The original obligation combines indefinitely retained admissions with recreation of cleaned same-target evidence. Decide admission/payload retention and replay responsibility explicitly; deleting completion evidence cannot by itself prevent repeats.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
