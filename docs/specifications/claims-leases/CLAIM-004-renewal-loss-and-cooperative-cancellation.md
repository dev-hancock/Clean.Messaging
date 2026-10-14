# CLAIM-004 — Renewal loss and cooperative cancellation

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

A processor needs to stop asserting ownership when renewal no longer establishes it. Hosts need intentional shutdown to remain distinguishable from a message fault.

## Guarantees

- Rejected renewal or unexpected renewal I/O failure MUST request cooperative claim-loss cancellation for the active handling attempt.
- Unexpected renewal failure MUST remain diagnostically observable; intentional renewal shutdown MUST NOT be reported as an ordinary renewal fault or successful settlement.
- Ownership loss MUST NOT be represented as forced handler termination or reversal of external effects.

## Contract boundaries

The processor links its handling token to renewal-loss signalling and the host stopping token; handlers and transports are responsible for observing cancellation. This does not guarantee that an uncooperative handler stops before another worker recovers the target. Durable settlement remains fenced independently of cancellation.

## Acceptance criteria

### CLAIM-004-AC-01 — Renewal rejected

- **Given** an active attempt's next renewal is rejected.
- **When** the renewal loop observes rejection.
- **Then** the handling attempt receives claim-loss cancellation instead of continued asserted ownership.

### CLAIM-004-AC-02 — Renewal I/O failure

- **Given** renewal throws an unexpected store exception.
- **When** the loop handles it.
- **Then** claim-loss cancellation is requested and the failure is observable diagnostically.

### CLAIM-004-AC-03 — Shutdown renewal

- **Given** host cancellation stops the renewal loop.
- **When** its wait or I/O is cancelled.
- **Then** no ordinary renewal fault or new successful settlement is fabricated from intentional shutdown.

### CLAIM-004-AC-04 — Uncooperative handler

- **Given** an old handler ignores cancellation and issues an external request after lease loss.
- **When** a successor acquires the target.
- **Then** the persistence fence does not promise to undo or prevent the old external effect.

## Open decisions

None.
