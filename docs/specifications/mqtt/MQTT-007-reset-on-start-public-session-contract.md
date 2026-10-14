# MQTT-007 — Reset-on-start public session contract

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Deployment owners need a deliberate one-time session reset without unintentionally discarding persistent broker state on every reconnect.

Authority: Public ResetSessionOnStart XML explicitly promises first-success reset and later persistence. MQTT clean-start semantics define a request, not broker retention guarantees. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- ResetSessionOnStart=true MUST request clean start until the first successful connection after process start, then persistent reconnects. Failed connections MUST NOT consume reset intent.
- ResetSessionOnStart=false MUST request no startup reset and preserve configured client identity/expiry on reconnect.

## Contract boundaries

First successful connect consumes reset even if subscriptions then fail. Default false means no requested reset, not proof prior state exists. Broker expiry/configuration and client identity determine actual persistence.

## Acceptance criteria

### MQTT-007-AC-01 — Reset happens on first successful connection only

- **Given** reset enabled and refused first connection followed by success.
- **When** initial attempts and later reconnects occur.
- **Then** refused attempts retain clean-start intent, first success requests it and later reconnects request persistence even after subscription failure.

### MQTT-007-AC-02 — Default startup does not request a session reset

- **Given** reset disabled.
- **When** startup/reconnect options are sent.
- **Then** clean start remains false with configured expiry/client identity, without inferring retained state.

## Open decisions

None.
