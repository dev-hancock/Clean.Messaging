# WORK-003 — Bounded processing and failure recovery

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need bounded local processing that survives recoverable infrastructure failures without falsely completing targets. Shutdown must release owned operation scopes while leaving unfinished durable work to its recovery lifecycle.

## Guarantees

- Local processing MUST respect configured concurrency, even when more targets are queued.
- A recoverable claim outage MUST allow subsequent claim retry when infrastructure is available; unavailable work MUST NOT be reported completed.
- An escaped processor failure MUST remain a diagnostic failure and MUST NOT automatically complete its target or prevent subsequent work from continuing.
- With cooperative dependencies, stopping the host MUST terminate the loop and dispose owned operation scopes without reporting abandoned targets successful.

## Contract boundaries

The host supplies valid capacity/concurrency, operational scopes and cooperative cancellation. No strict recovery deadline through outages or forced cancellation of user code is promised. Escaped failures and claim infrastructure retries are not themselves per-message retry/dead-letter settlement; those require the durable processing policy.

## Acceptance criteria

### WORK-003-AC-01 — Concurrency bound

- **Given** more targets are queued than configured Concurrency and handlers are held.
- **When** the work loop runs.
- **Then** no more than the configured number of local processing calls overlap.

### WORK-003-AC-02 — Claim outage recovery

- **Given** claiming first fails and the store later becomes available.
- **When** the live loop continues past its recovery delay.
- **Then** claiming is retried without representing unavailable work as already completed.

### WORK-003-AC-03 — Escaped processor failure

- **Given** one processor call throws unexpectedly while other durable work remains.
- **When** the loop handles the escape.
- **Then** the failure is diagnostic, subsequent work can continue and the failed target is not automatically marked completed.

### WORK-003-AC-04 — Cooperative shutdown

- **Given** waiting and processing dependencies cooperate with cancellation.
- **When** the host stops.
- **Then** the loop terminates and disposes owned operation scopes without claiming abandoned targets succeeded.

## Open decisions

None.
