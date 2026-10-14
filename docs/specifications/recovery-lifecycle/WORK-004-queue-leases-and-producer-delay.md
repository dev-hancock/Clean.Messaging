# WORK-004 — Queue, leases and producer delay

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

A local queue cannot give a delayed worker permanent settlement authority or promise ordered completion. Hosts need invalid resource settings rejected, while queue lease protection and infrastructure backoff need deliberate policy rather than accidental algorithm mandates.

## Guarantees

- Queued processing MUST NOT authorize settlement without valid current ownership. Queue position is not a lease extension.
- WorkLoopOptions.Validate and normal Run MUST reject nonpositive capacity, concurrency, batch size or loop time values before normal processing starts.
- Concurrent target completion MUST NOT be represented as FIFO merely because acquisition selected targets in an order.

## Contract boundaries

The host supplies positive budgets and loop times. Handlers can complete at different speeds; retries/recovery can alter order. This page does not select queued-lease renewal/revalidation or an exact infrastructure delay algorithm. Those choices do not weaken settlement fencing, bounded resources or recoverable durable work.

## Acceptance criteria

### WORK-004-AC-03 — Invalid loop options

- **Given** Capacity, Concurrency or BatchSize is nonpositive, or any loop time value is nonpositive.
- **When** Validate or normal Run is requested.
- **Then** validation fails before normal processing begins.

### WORK-004-AC-04 — No FIFO completion guarantee

- **Given** targets were acquired in selection order and concurrent handlers finish at different speeds.
- **When** processing completes.
- **Then** completion is not required to follow acquisition order.

## Open decisions

These original identifiers and heading anchors remain reserved, not normative scenarios. No replacement IDs are assigned before the underlying policy and identity migration are decided.

### WORK-004-AC-01 — Queued lease expiry

Reserved — [GAP-WF-003](../GAPS.md#gap-wf-003). The original composite requires no independent queued renewal as well as current settlement authority. Fencing remains a guarantee; the whole ID is reserved rather than silently narrowing it. Shared-worker renewal during processing does not establish ownership protection before invocation.

### WORK-004-AC-02 — Producer delay budget

Reserved — [GAP-INTENT-LIFE-003](../GAPS.md#gap-intent-life-003). Exact doubling/reset behavior is an observed infrastructure algorithm, not an approved recovery schedule.
