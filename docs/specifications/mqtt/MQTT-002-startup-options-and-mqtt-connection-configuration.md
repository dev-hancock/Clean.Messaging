# MQTT-002 — Startup options and MQTT connection configuration

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Deployment owners need adapter-specific connection defaults and valid configuration without confusing validator acceptance with broker compatibility or secure deployment certification.

Authority: Deliberate public option defaults and selected MQTT 5 protocol are compatibility surfaces. Coding standards require validation and truthful operational/security boundaries, not accidental numeric limits or ignored password-only configuration. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Connections MUST preserve the explicitly selected MQTT 5 contract and deliberate public defaults unless configured otherwise. Outgoing settings MUST NOT acquire inbox-only session/reset/reconnect options.
- Invalid required configuration MUST NOT be presented as ready connectivity. Option acceptance MUST NOT certify broker/network acceptance or security.
- TLS selection MUST NOT silently change port or certify host certificates.

## Contract boundaries

Public defaults: both use port 1883, QoS 1 and TLS off; username/password are optional and Host/ClientId required. Inbox additionally defaults to uint.MaxValue session expiry seconds, five-second reconnect delay and ResetSessionOnStart=false. Outgoing exposes none of these inbox-only settings. QoS 1/2 concerns protocol delivery, not exactly-once application effects.

## Acceptance criteria

### MQTT-002-AC-03 — Defaults remain adapter-specific

- **Given** Host/ClientId are supplied and other public options untouched.
- **When** connection/publication settings are produced.
- **Then** the stated adapter-specific defaults apply, both clients select MQTT 5 and outgoing settings acquire no inbox-only expiry/reset/delay.

## Open decisions

The original IDs and exact heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### MQTT-002-AC-01 — Invalid operational options prevent valid startup configuration

Which numeric/operational limits and diagnostics are deliberate? The composite includes exact reconnect bounds and multiple validation messages. Decision [GAP-INTENT-TX-002](../GAPS.md#gap-intent-tx-002) remains unapproved.

### MQTT-002-AC-02 — Validator boundary is distinct from broker validity

Should out-of-network-range positive ports be accepted, and what reconnect-delay boundary is supported? Port 65536 acceptance is not an operational guarantee. Decision [GAP-INTENT-TX-002](../GAPS.md#gap-intent-tx-002) remains unapproved.

### MQTT-002-AC-04 — Credentials and TLS are configured without implicit security claims

Should password-only configuration be ignored, rejected or supported? Reserve the complete credentials/TLS criterion, not an ID narrowed to the established no-implicit-port/security guarantee. Decision [GAP-INTENT-TX-002](../GAPS.md#gap-intent-tx-002) remains unapproved.
