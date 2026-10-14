# MQTT-006 — Receive failures, poison acknowledgement and shutdown

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Receipt owners need ACK failure/shutdown to preserve committed work without manufactured protocol success or accidental poison classification.

Authority: Coding standards on cancellation/failure distinctions, truthful diagnostics and separate admission/ACK boundaries. No-consumer and synchronous wrapping policies remain unresolved. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- ACK failure MUST preserve committed admission, identify the appropriate receipt/poison ACK failure and request recovery without claiming confirmed ACK success.
- Direct stopping cancellation MUST remain cancellation without ordinary admission-fault/reconnect classification or invented ACK success. Committed work MUST NOT be presented as rolled back by shutdown.

## Contract boundaries

An in-flight ACK can have uncertain remote outcome; no confirmed ACK is not proof the broker received nothing. Consumer progress is independent. No-consumer disposition belongs to admission, not automatic MQTT discard.

## Acceptance criteria

### MQTT-006-AC-01 — Acknowledgement failure requests recovery without erasing admission

- **Given** committed admission or already-classified poison.
- **When** explicit ACK throws a non-shutdown exception.
- **Then** the appropriate ACK failure is logged and reconnect requested without erasing admission or reporting confirmed success.

### MQTT-006-AC-02 — Shutdown cancellation leaves unsettled delivery unacknowledged

- **Given** admission/ACK is awaiting cancellable work and the host stopping token is cancelled.
- **When** asynchronous receipt work observes shutdown cancellation.
- **Then** ordinary admission-fault logging/reconnect classification is suppressed without manufactured ACK; committed work is not automatically rolled back.

## Open decisions

The original IDs and exact heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### MQTT-006-AC-03 — No consumer is an admission failure rather than automatic poison discard

Should valid no-consumer messages be rejected, retained for replay or have another admission policy? Reserve the full classification/ACK composite until decided. See [GAP-DUR-001](../GAPS.md#gap-dur-001); the recommendation remains unapproved.

### MQTT-006-AC-04 — Wrapped mapper cancellation currently reaches poison policy

Should wrapped mapper cancellation reach poison discard or preserve attempt cancellation? Recording current behavior approves no change. See [GAP-TX-002](../GAPS.md#gap-tx-002); the recommendation remains unapproved.
