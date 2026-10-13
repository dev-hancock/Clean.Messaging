# WORK-002 — Signal observation and cancellation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** AsyncAutoResetEvent has a coalescing one-slot wake; WorkLoop emits initial wake.

Pre: signal can be awaited. Post: a signal permits an observation without promising a counter per Wake.

## WORK-002-AC-01 — Wake before wait
**Given** one wake has been posted with no active waiter.
**When** WaitAsync runs.
**Then** the pending wake permits the wait to complete.

## WORK-002-AC-02 — Wake coalescing
**Given** many Set calls occur before a waiter consumes one.
**When** waits are observed.
**Then** the signal is a hint and does not require one completion per Set; no work identity is stored by the signal.

## WORK-002-AC-03 — Cancelled empty wait
**Given** no wake is pending and the wait token is cancelled.
**When** WaitAsync waits.
**Then** it ends with cancellation rather than reporting that durable work was processed.

## WORK-002-AC-04 — Initial discovery
**Given** eligible stored work exists at host start with no producer notification.
**When** WorkLoop.Run starts normally.
**Then** it requests initial discovery instead of requiring a new external enqueue.
