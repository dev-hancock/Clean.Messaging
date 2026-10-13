# MQTT-003 — Incoming filters choose exactly one route

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** [RouteFilter](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/RouteFilter.cs), [router](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Routing/MqttInboxRouter.cs), [route contract](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Routing/IMqttInboxRoute.cs), [poison exception](../../../src/Clean.Messaging.Inbox.Ingress.Mqtt/Routing/MqttPoisonMessageException.cs).

## MQTT-003-AC-01 — Wildcards and topic levels select one payload map
**Given** non-overlapping filters `devices/+/state` and `events/#`, **When** `devices/a/state` or `events/a/b` arrives, **Then** exactly its matching route supplies the payload/message map, not broadcast fan-out across routes. Durable consumer fan-out occurs after admission, not MQTT route selection.

## MQTT-003-AC-02 — Invalid wildcard placement and overlap reject configuration
**Given** `a/#/b`, `a+b`, `a#`, duplicate filters, or overlapping `a/+` and `a/b`, **When** the router is constructed, **Then** invalid syntax or overlap fails configuration instead of selecting an arbitrary winner. A blank filter is already rejected by `On`/Subscribe.

## MQTT-003-AC-03 — System-topic distinction and level boundaries remain explicit
**Given** `#` and `$SYS/#`, or distinct filters containing empty levels such as `a//b` and `a/b`, **When** overlap validation runs, **Then** the system/non-system filters are not deemed overlapping solely through `#`, and distinct empty/nonempty level paths remain distinct. Runtime route matching follows MQTTnet's topic-filter comparer, not a custom global ordering rule.

## MQTT-003-AC-04 — Unroutable incoming work is poison
**Given** a received topic with no matching configured route, **When** routing resolves it, **Then** it raises MQTT poison classification and no durable admission occurs. If multiple runtime matches occur despite configuration validation, the same poison path rejects ambiguity rather than admitting twice.
