# MQTT-015 — Pending interoperability and lifecycle policies

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour
**Evidence:** [stale outbox README](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/README.md), current registration/builders/connection/factories and inbox lifecycle. No decision approving the stale SPI, full shared-client concurrency claim, optional empty-ID interoperability or bounded shutdown/resource policy was found.
**Pending decisions:** [GAP-TX-001](../GAPS.md#gap-tx-001), [GAP-TX-005](../GAPS.md#gap-tx-005), [GAP-TX-006](../GAPS.md#gap-tx-006), [GAP-TX-009](../GAPS.md#gap-tx-009). These numbered questions are not executable acceptance criteria. An owner decision must select definite observable outcomes before criteria can be authored.

1. **Empty optional IDs between peers:** Must empty correlation/causation identities be omitted, rejected before publication or interpreted as absent by ingress? Which representation must both sides support? No alternative is selected here; current asymmetric behaviour remains MQTT-010.
2. **Supported concurrency and shutdown:** Which overlaps among publication, reconnect and resource disposal must be supported? Must shutdown drain in-flight callbacks/publications, within what bound, and with which allowed outcomes? Which component owns synchronization disposal? No MQTTnet safety or bounded termination guarantee is selected here.
3. **Authoritative registration documentation:** Should the historical README SPI be retired or replaced with the current public API, and what migration guidance is required? Who approves that API/documentation decision? No production or reference-documentation repair is performed in this scoped specification task.
