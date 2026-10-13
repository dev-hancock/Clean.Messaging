# INBOX-004 — Durable entry state and fenced claims

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: `InboxEntry` and inherited `DurableStore.Claim/Renew/Complete/Retry/DeadLetter`, `DurableEntryConfiguration`. Shared paths are traced below. [Processing](../../reference/processing.md) is descriptive.

Contracts: IInboxStore inherited durable operations and InboxEntry identity, payload, metadata, lifecycle and diagnostic properties; Key is (MessageId, TargetId). Preconditions: Claim limit and lease positive; mappings installed. Postconditions: claims increment Attempts and clear NextAttemptAt; settlement returns a bool for ownership. Invariants: terminal rows are not eligible; target rows complete independently. ClaimedUntil equals now is expired, never valid ownership. Store ordering is creation/message/target order, not a guarantee of handler completion order.

## INBOX-004-AC-01 — Claim only available due work

- **Given** pending entries include future-due, due-at-now, actively claimed, expired-at-now and terminal rows.
- **When** Claim(now, positive limit, positive lease) completes.
- **Then** only unresolved due work with absent/expired leases is returned, at most the limit, with a claim token, incremented Attempts and a new expiry.

## INBOX-004-AC-02 — Fence stale and concurrent owners

- **Given** an entry has been reclaimed with a new token or its lease equals/exceeds expiry at settlement time.
- **When** the stale owner attempts Renew, Complete, Retry or DeadLetter.
- **Then** false is returned and no state is overwritten; only a matching unexpired owner can settle.

## INBOX-004-AC-03 — Keep independent target progress

- **Given** one logical event has several target entries.
- **When** one owned target completes and another schedules retry.
- **Then** only the first has ProcessedAt; the retry target retains identities/payload and diagnostics with its next due time.

## INBOX-004-AC-04 — Validate claim limits and renew successful ownership

- **Given** invalid nonpositive limit/lease, or a matching unresolved unexpired owned row.
- **When** Claim is requested for invalid inputs or Renew extends the owned lease.
- **Then** invalid Claim fails; valid Renew updates the expiry and returns true without changing identities.
