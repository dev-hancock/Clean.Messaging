# SCHED-012 — Failure budgets, diagnostics and cancellation recovery

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need recoverable retry/permanent-failure outcomes and distinct cooperative cancellation without treating acquisition counts as proven handler calls.

FailureAction compatibility, durable retry and cancellation standards justify disposition distinctions; public retry options establish configured bounds, not an internal accounting unit or exact delay algorithm. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- A current owned permanent failure MUST become terminal when settlement succeeds; retryable unfinished work MUST remain recoverable within the selected budget.
- Observed processing-token cancellation MUST not be classified as an ordinary message failure; an unsuccessful settlement MUST not masquerade as completed work.

## Contract boundaries

Retry-budget accounting, exact backoff and diagnostic history/representation remain undecided. Fencing applies to settlement; expiry asymmetry remains GAP-WF-004. Lease recovery is not a guarantee of exact invocation count.

## Acceptance criteria

### SCHED-012-AC-01 — Permanent failure settles immediately

**Given** a currently owned active schedule fails with a Fault-classified MessageException below the retry budget.
**When** authorized failure settlement succeeds.
**Then** FailedAt records terminal failure with the failure code/message and no remaining claim, lease or retry eligibility; transient retry is not substituted.

### SCHED-012-AC-02 — Ordinary and transient failure retry within budget

**Given** a currently owned delivery has budget remaining and fails ordinarily or with Retry-classified MessageException.
**When** authorized retry settlement succeeds.
**Then** it remains nonterminal with recorded diagnostics and a next-attempt instant, without current claim/lease; it is not eligible before that retry instant.

### SCHED-012-AC-07 — Shutdown cancellation is not normal message failure

**Given** the processing token is cancelled during scheduled work.
**When** execution observes that cancellation.
**Then** the cancellation remains distinct from normal retry/permanent failure settlement; unfinished durable ownership can later expire and recover.

### SCHED-012-AC-08 — Failed settlement is recoverable unfinished work

**Given** recording retry or failure itself fails.
**When** the worker resumes recovery.
**Then** that failed settlement or its log is not successful completion; still-active persisted work becomes recoverable when its ownership expires and eligibility permits.

## Open decisions

### SCHED-012-AC-03 — Exhaustion occurs on failed attempt at boundary

**Reserved — no normative acceptance scenario.** Which unit consumes scheduling retry budget, especially lease-loss, cancelled and never-admitted claims?

Decision: [GAP-CLAIM-001](../GAPS.md#gap-claim-001) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SCHED-012-AC-04 — Capped exponential delays have no jitter

**Reserved — no normative acceptance scenario.** Is exact deterministic capped exponential retry, including no jitter, the intended scheduling policy?

Decision: [GAP-INTENT-WF-002](../GAPS.md#gap-intent-wf-002) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SCHED-012-AC-05 — Diagnostic bounds and fallback code

**Reserved — no normative acceptance scenario.** Which diagnostic fallback codes, truncation/sanitization rules and post-success/cancellation history are guaranteed?

Decision: [GAP-INTENT-WF-003](../GAPS.md#gap-intent-wf-003) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.

### SCHED-012-AC-06 — Success retains earlier retry history

**Reserved — no normative acceptance scenario.** Which diagnostic fallback codes, truncation/sanitization rules and post-success/cancellation history are guaranteed?

Decision: [GAP-INTENT-WF-003](../GAPS.md#gap-intent-wf-003) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
