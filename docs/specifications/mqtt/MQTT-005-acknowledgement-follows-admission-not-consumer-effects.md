# MQTT-005 — Acknowledgement follows admission, not consumer effects

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Operators need acknowledgement to transfer responsibility to durable work, not falsely imply consumer completion. Explicit poison discard must stay distinct from persistence failure.

Authority: Coding standards explicitly require admission-before-ACK, preservation of poison policy, persistence failure distinction and retained per-target completion. None follows merely from QoS. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Successful valid-receipt acknowledgement MUST follow awaited admission and MUST NOT depend on consumer completion. Automatic acknowledgement MUST NOT bypass admission.
- Ordinary admission failure MUST NOT become successful acknowledgement or poison discard; recovery MUST be requested.
- Legitimate repeats recognized within retained duplicate evidence MUST NOT reset or duplicate retained obligations.
- Already-classified poison MUST follow existing logged acknowledgement-attempt policy without inbox poison/dead-letter archival.

## Contract boundaries

Admission transfers local processing responsibility, not completed business effects. ACK may fail after commit; redelivery depends on retained broker/session state. Already-classified poison includes ordinary mapping poison, not approval of wrapping attempt cancellation. No archive or broker retention horizon is promised.

## Acceptance criteria

### MQTT-005-AC-01 — Successful admission transfers processing responsibility

- **Given** valid input and durable inbox with consumers paused.
- **When** admission commits and acknowledgement succeeds.
- **Then** ACK follows admission and can precede consumer completion; local work remains outstanding.

### MQTT-005-AC-02 — Admission failure is not acknowledged as success

- **Given** valid payload/metadata but unavailable/failing admission.
- **When** admission fails.
- **Then** no successful ACK, consumer success or poison discard hides the fault, and recovery is requested.

### MQTT-005-AC-03 — Duplicate receipt after lost acknowledgement does not reset work

- **Given** committed admission, lost ACK and broker redelivery of the same IDs with local evidence retained.
- **When** relational inbox recognizes and acknowledges the legitimate duplicate.
- **Then** one admission/obligation set remains without resetting completed targets; broker retention is not inferred.

### MQTT-005-AC-04 — Explicit poison policy stays distinct from admission failure

- **Given** already-classified routing, metadata or ordinary ingress-mapping poison.
- **When** receipt follows poison policy.
- **Then** rejection is logged and ACK attempted without an inbox poison/dead-letter record; ordinary admission errors do not use discard.

## Open decisions

None.
