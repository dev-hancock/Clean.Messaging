# MQTT-015 — Pending interoperability and lifecycle policies

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Peer/host owners need unresolved interoperability and lifecycle choices visible rather than silently settled by source-derived specifications or stale registration examples.

Authority: Coding standards on metadata/public compatibility, ownership and truthful outcomes establish boundaries but do not choose optional-empty-ID representation, race/draining support or historical README migration. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Metadata compatibility MUST preserve identity roles and explicit absence/invalid-input distinctions.
- Resource ownership MUST NOT imply exactly-once effects, active-publication draining or supported race outcomes without deliberate contracts.
- Public APIs MUST remain distinct from historical documentation; compatibility/migration changes MUST be explicitly owned.

## Contract boundaries

No acceptance IDs were previously assigned to these numbered questions. No executable policy scenarios or replacement IDs are invented. Current fluent capability is catalogued in MQTT-001; retained-route compatibility remains a deployment question.

## Acceptance criteria

No acceptance scenarios are assigned to unresolved choices below.

## Open decisions

- **Empty optional IDs between peers:** Must empty correlation/causation identities be omitted, rejected before publication or interpreted as absent by ingress? Which representation must both sides support? No alternative is selected here; current asymmetric behaviour remains MQTT-010. See [GAP-TX-005](../GAPS.md#gap-tx-005); its recommendation remains unapproved.
- **Supported concurrency and shutdown:** Which overlaps among publication, reconnect and resource disposal must be supported? Must shutdown drain in-flight callbacks/publications, within what bound, and with which allowed outcomes? Which component owns synchronization disposal? No MQTTnet safety or bounded termination guarantee is selected here. See [GAP-TX-006](../GAPS.md#gap-tx-006) and [GAP-TX-009](../GAPS.md#gap-tx-009); their recommendations remain unapproved.
- **Authoritative registration documentation:** Should the historical README SPI be retired or replaced with the current public API, and what migration guidance is required? Who approves that API/documentation decision? No production or reference-documentation repair is performed in this scoped specification task. See [GAP-TX-001](../GAPS.md#gap-tx-001); its recommendation remains unapproved.

Inbox draining/resource questions also belong to [GAP-TX-009](../GAPS.md#gap-tx-009); no policy is approved.
