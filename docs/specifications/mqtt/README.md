# MQTT ingress and dispatch

Hosts use MQTT subscriptions to transfer valid receipts into durable inbox responsibility, and publish committed outbox targets through MQTT without leaking broker mechanics into consumer contracts.

## Capability boundaries

Successful valid-receipt ACK follows admission, not consumer completion; explicit poison discard remains separate. Stored destination and application identity survive retry. Broker publication success is not subscriber effects or durable settlement; QoS 2 is not exactly-once application processing. Session retention, TLS deployment and client race/drain support are not inferred.

These specifications separate independently supported guarantees from reserved original criteria. Open recommendations remain unapproved; source observations do not settle policy.

## Capability catalogue

- [MQTT-001 — Registration and public fluent routes](MQTT-001-registration-and-public-fluent-routes.md)
- [MQTT-002 — Startup options and MQTT connection configuration](MQTT-002-startup-options-and-mqtt-connection-configuration.md)
- [MQTT-003 — Incoming filters choose exactly one route](MQTT-003-incoming-filters-choose-exactly-one-route.md)
- [MQTT-004 — Incoming metadata, serialization and contract compatibility](MQTT-004-incoming-metadata-serialization-and-contract-compatibility.md)
- [MQTT-005 — Acknowledgement follows admission, not consumer effects](MQTT-005-acknowledgement-follows-admission-not-consumer-effects.md)
- [MQTT-006 — Receive failures, poison acknowledgement and shutdown](MQTT-006-receive-failures-poison-acknowledgement-and-shutdown.md)
- [MQTT-007 — Reset-on-start public session contract](MQTT-007-reset-on-start-public-session-contract.md)
- [MQTT-008 — Inbox reconnect, subscription results and cleanup](MQTT-008-inbox-reconnect-subscription-results-and-cleanup.md)
- [MQTT-009 — Outgoing target and destination staging](MQTT-009-outgoing-target-and-destination-staging.md)
- [MQTT-010 — Outgoing packet metadata and retain semantics](MQTT-010-outgoing-packet-metadata-and-retain-semantics.md)
- [MQTT-011 — Outgoing connection ownership, concurrency and disposal](MQTT-011-outgoing-connection-ownership-concurrency-and-disposal.md)
- [MQTT-012 — Publish result, durable completion and failure classification](MQTT-012-publish-result-durable-completion-and-failure-classification.md)
- [MQTT-013 — External repeat, cancellation and independently successful targets](MQTT-013-external-repeat-cancellation-and-independently-successful-targets.md)
- [MQTT-014 — Observable logs do not overstate settlement](MQTT-014-observable-logs-do-not-overstate-settlement.md)
- [MQTT-015 — Pending interoperability and lifecycle policies](MQTT-015-pending-interoperability-and-lifecycle-policies.md)

## Related contracts and decisions

- [GAP-TX-001](../GAPS.md#gap-tx-001)
- [GAP-TX-002](../GAPS.md#gap-tx-002)
- [GAP-TX-003](../GAPS.md#gap-tx-003)
- [GAP-TX-004](../GAPS.md#gap-tx-004)
- [GAP-TX-005](../GAPS.md#gap-tx-005)
- [GAP-TX-006](../GAPS.md#gap-tx-006)
- [GAP-TX-007](../GAPS.md#gap-tx-007)
- [GAP-TX-008](../GAPS.md#gap-tx-008)
- [GAP-TX-009](../GAPS.md#gap-tx-009)

- [Transport](../transport/README.md)
- [repository instructions](../../../.github/copilot-instructions.md)
- [coding standards](../../CODING_STANDARDS.md)
- [package contract and source map](../../reference/contract-map.md)
- [gap register](../GAPS.md)
