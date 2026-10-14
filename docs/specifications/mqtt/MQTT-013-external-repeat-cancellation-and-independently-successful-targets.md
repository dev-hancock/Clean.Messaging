# MQTT-013 — External repeat, cancellation and independently successful targets

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Applications need MQTT fan-out recovery to preserve identity and completed siblings even when publication succeeds before settlement or ownership is lost.

Authority: Coding standards explicitly require stable identity, external repetition, fencing, cancellation distinctions and independent progress. Standards section 7 explicitly distinguishes propagated handling cancellation from separately bounded settlement after processing failure; neither is replaced with the other. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Unconfirmed local completion MUST permit repeat publication with stored destination/message/event IDs. QoS 2/local uniqueness MUST NOT imply one subscriber effect.
- Stale ownership MUST NOT settle another attempt or be represented as revoking issued publication.
- Observed attempt cancellation MUST remain distinct from broker poison/permanent rejection without invented success or reversed accepted publication.
- Failed unresolved targets MUST NOT reset/reexecute independently completed retained siblings.

## Contract boundaries

Connection/publication and owned successful completion receive handling cancellation; postfailure settlement has a separate bounded consistency boundary. Do not infer universal host-cancellation propagation through recovery settlement. Broker acceptance may precede cancellation; retention bounds duplicate protection and no global completion order is promised.

## Acceptance criteria

### MQTT-013-AC-01 — Post-publish crash can produce the same logical publication again

- **Given** broker-accepted publication followed by process loss before durable completion.
- **When** unresolved work is recovered.
- **Then** publication may repeat with stored IDs/destination; QoS 2/local uniqueness does not prove one recipient effect.

### MQTT-013-AC-02 — Lease fencing limits settlement, not an issued publish

- **Given** a publish in flight loses ownership and a new owner starts.
- **When** the stale publish finishes and attempts completion.
- **Then** its claim cannot complete new-owner work although external effects may overlap/already exist.

### MQTT-013-AC-03 — Shutdown and claim-loss cancellation are not ordinary poison

- **Given** a cancellable outgoing attempt.
- **When** shutdown or claim loss cancels handling.
- **Then** cancellation reaches that attempt's cancellable connection, publication and owned settlement boundaries without broker poison/permanent rejection or manufactured completion. Accepted publication is not claimed reversed; postfailure recovery preserves its separate bounded consistency boundary.

### MQTT-013-AC-04 — Successful sibling target remains settled during another failure

- **Given** a retained completed MQTT/local target and another failed MQTT target.
- **When** recovery processes failed work.
- **Then** the sibling is not reset/reexecuted solely for that failure; no global completion order is promised.

## Open decisions

None.
