# WORK-002 — Signal observation and cancellation

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Workers need a cancellable hint that prompts durable discovery, including when work predates the waiter or host startup. Signals are not a counted message queue and must not imply that a target was processed.

## Guarantees

- A wake posted before a waiter MUST permit a subsequent wait to complete.
- Multiple wakes MAY coalesce; signal observation MUST NOT imply one completion per wake or retain durable work identity.
- An empty wait MUST honor cancellation rather than report processing success.
- Starting the work loop MUST request initial durable discovery without requiring a new producer enqueue.

## Contract boundaries

This contract concerns the supplied process-local signal and normal WorkLoop.Run startup with valid options. Store state remains authoritative. No fairness among waiters, wake count, durable queueing or forced interruption of handlers is established.

## Acceptance criteria

### WORK-002-AC-01 — Wake before wait

- **Given** a wake was posted with no active waiter.
- **When** the next WaitAsync is requested.
- **Then** the pending wake permits that wait to complete.

### WORK-002-AC-02 — Wake coalescing

- **Given** many Set calls occur before a waiter consumes a hint.
- **When** subsequent waits are observed.
- **Then** one completion per Set is not required and the signal stores no work identity.

### WORK-002-AC-03 — Cancelled empty wait

- **Given** no wake is pending and the wait token is cancelled.
- **When** WaitAsync waits.
- **Then** the wait ends with cancellation rather than reporting durable processing.

### WORK-002-AC-04 — Initial discovery

- **Given** eligible stored work exists at startup without a producer notification.
- **When** WorkLoop.Run starts normally.
- **Then** initial discovery is requested without requiring another external enqueue.

## Open decisions

None.
