# MQTT-008 — Inbox reconnect, subscription results and cleanup

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Inbox hosts need bounded, noncompeting connection recovery and truthful readiness, with explicit resource ownership through shutdown.

Authority: Coding standards on bounded owned resources/work, failure distinctions and truthful readiness. They do not select exact resubscription/result acceptance, fixed recovery sequence or final-disconnect/draining policy. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Concurrent failures MUST be coalesced without unbounded independent reconnects. Connection transitions MUST serialize and worker disconnect MUST NOT recursively create recovery signals.
- Connection/subscription failure MUST NOT become ready subscription state. Broker reconnect MUST remain separate from durable retry.
- Client and synchronization resources MUST have explicit lifetime ownership; shutdown MUST NOT manufacture receipt/consumer success.

## Contract boundaries

Delivery depends on subscription acceptance and actual broker/session state. No exact result-count/QoS-grant contract, unconditional resubscription or shutdown duration is selected here. Resource ownership is not race certification.

## Acceptance criteria

### MQTT-008-AC-03 — Concurrent failure signals do not queue unbounded reconnects

- **Given** a connected client and concurrent admission failures/unexpected disconnects.
- **When** recovery is signalled.
- **Then** requests are coalesced instead of unbounded queued reconnects, transitions serialize and worker disconnect does not recursively resignal.

## Open decisions

The original IDs and exact heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### MQTT-008-AC-01 — Each successful connection subscribes all routes again

What subscription/session-present/granted-QoS contract establishes readiness? Reserve unconditional resubscription and exact result acceptance together under the original composite ID. Decision [GAP-INTENT-TX-004](../GAPS.md#gap-intent-tx-004) remains unapproved.

### MQTT-008-AC-02 — Refused connection or rejected subscription uses fixed reconnect loop

Which setup failures require fixed-delay disconnect/reconnect and what results establish readiness? Decide together with the subscription contract. Decision [GAP-INTENT-TX-004](../GAPS.md#gap-intent-tx-004) remains unapproved.

### MQTT-008-AC-04 — Shutdown removes callbacks and attempts final disconnect

What bound and callback-draining contract applies to final disconnect/disposal? The original fixes an uncancelled disconnect and cleanup sequence without resolving resource obligations. See [GAP-TX-009](../GAPS.md#gap-tx-009); the recommendation remains unapproved.
