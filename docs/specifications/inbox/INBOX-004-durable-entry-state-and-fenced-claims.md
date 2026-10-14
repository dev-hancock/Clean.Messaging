# INBOX-004 — Durable entry state and fenced claims

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Workers need temporary exclusive settlement authority over each unresolved target so crashes permit recovery and an obsolete worker cannot overwrite a newer attempt or a completed sibling.

## Guarantees

- A claim MUST be bounded by the requested positive limit and lease and MUST exclude terminal work, future-due work and work still validly owned elsewhere.
- Settlement or renewal by a mismatched or no-longer-authorized owner MUST be rejected without overwriting target state.
- Completion and retry MUST remain independent per event/target; retry MUST preserve identity, payload and failure diagnosis.
- Nonpositive claim limits or leases MUST fail. Successful renewal by the current unresolved owner MUST extend authority without changing identity.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 7 and 9 establish bounded ownership, stale-owner fencing, failure preservation and per-target progress; public durable-store inputs and identity compatibility establish validation and renewal boundaries.

## Contract boundaries

Claims fence database transitions, not arbitrary handler execution or external effects. Due work is eligible rather than promised an execution time; selection does not imply FIFO completion. Provider concurrency and uncertain acquisition remain in [GAP-CORE-005](../GAPS.md#gap-core-005). Exact equality and abandoned-attempt accounting remain undecided.

## Acceptance criteria

### INBOX-004-AC-03 — Keep independent target progress

- **Given** one logical event has multiple target entries and both attempts hold valid authority.
- **When** one target completes while another records a retry.
- **Then** only the successful target is completed; the retry target preserves identities, payload and diagnostics with its next due time.

### INBOX-004-AC-04 — Validate claim limits and renew successful ownership

- **Given** a claim has a nonpositive limit or lease, or an unresolved owner has a matching still-valid claim.
- **When** invalid acquisition is requested or valid renewal is requested.
- **Then** invalid acquisition fails; successful renewal returns true with the extended expiry and unchanged identities.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### INBOX-004-AC-01 — Claim only available due work

Reserved pending [GAP-CLAIM-001](../GAPS.md#gap-claim-001), [GAP-CLAIM-002](../GAPS.md#gap-claim-002). The original composite includes exact due/lease equality and one attempt increment per acquisition. Bounded acquisition, terminal/future-due exclusion and existing-owner protection remain established, but temporal equality and attempt accounting need approval.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.

### INBOX-004-AC-02 — Fence stale and concurrent owners

Reserved pending [GAP-CLAIM-002](../GAPS.md#gap-claim-002). The original stale-owner criterion also chooses rejection at exact lease expiry. Stale or mismatched ownership remains fenced; exact equality requires a consistent eligibility/settlement decision rather than reassignment of this composite.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
