# MQTT-003 — Incoming filters choose exactly one route

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Ingress hosts need a topic to choose one typed payload route; consumer fan-out belongs after durable admission rather than ambiguous transport routing.

Authority: MQTT topic-filter protocol semantics and deliberate single-route typed ingress, plus coding standards on validation and preserving poison classification. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- MQTT whole-level +/# semantics, terminal #, empty levels and system-topic wildcard distinction MUST be preserved.
- Invalid/overlapping routes MUST fail configuration without an arbitrary precedence winner.
- Routable receipts MUST select one typed map, not broadcast. No route or runtime ambiguity MUST reject admission as poison.

## Contract boundaries

Filter selection is separate from durable consumer fan-out. A non-system wildcard does not claim $SYS topics. Routing poison does not itself acknowledge a receipt.

## Acceptance criteria

### MQTT-003-AC-01 — Wildcards and topic levels select one payload map

- **Given** nonoverlapping devices/+/state and events/# filters.
- **When** devices/a/state or events/a/b arrives.
- **Then** only the matching route supplies the map, without broadcast; durable fan-out remains after admission.

### MQTT-003-AC-02 — Invalid wildcard placement and overlap reject configuration

- **Given** a/#/b, a+b, a#, duplicate/overlapping filters or a blank filter.
- **When** configuration reaches On/Subscribe validation or router construction.
- **Then** invalid syntax, overlap and blank filters fail instead of choosing a winner.

### MQTT-003-AC-03 — System-topic distinction and level boundaries remain explicit

- **Given** filters # and $SYS/#, or a//b and a/b.
- **When** overlap validation and matching occur.
- **Then** system topics are not captured solely by non-system # and empty levels remain distinct; runtime matching follows MQTTnet topic-filter semantics, not global ordering.

### MQTT-003-AC-04 — Unroutable incoming work is poison

- **Given** a receipt with no route or multiple matches despite configuration validation.
- **When** routing resolves it.
- **Then** poison prevents durable admission or admission twice.

## Open decisions

None.
