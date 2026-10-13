# MQTT-012 — Publish result, durable completion and failure classification

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** [MqttOutboxTransport](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttOutboxTransport.cs), [MqttPublishException](../../../src/Clean.Messaging.Outbox.Dispatch.Mqtt/Publishing/MqttPublishException.cs), dependencies [external wrapper](../../../src/Clean.Messaging.Outbox/Transport/OutboxTransport.cs), [processor](../../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs), [retry policy](../../../src/Clean.Messaging/Retry/RetryPolicy.cs).

Current route lookup → durable-source deserialize/map/outgoing serialize → connected client → packet factory → awaited PublishAsync(token) → result.IsSuccess → generic wrapper's fenced completion. No adapter-level publish retry occurs. Published bytes are not a receipt from subscribers.

| Unsuccessful broker result | Code | Action |
| --- | --- | --- |
| NotAuthorized | mqtt.publish.not_authorized | Fault/permanent |
| TopicNameInvalid | mqtt.publish.topic_name_invalid | Fault/permanent |
| PayloadFormatInvalid | mqtt.publish.payload_format_invalid | Fault/permanent |
| Other unsuccessful result | mqtt.publish.rejected | Retry/transient |

## MQTT-012-AC-01 — Successful publication returns for owned settlement
**Given** a valid outgoing target and a successful broker publish result, **When** dispatch returns, **Then** the external wrapper attempts completion for that target/key/claim; the adapter did not wait for downstream application consumption, and false/rejected settlement is not changed into a successful database completion.

## MQTT-012-AC-02 — Permanent publish rejections do not receive transient retries
**Given** an owned entry and broker NotAuthorized, TopicNameInvalid or PayloadFormatInvalid result, **When** durable failure handling classifies the thrown publish failure, **Then** its code/action match the table and owned dead-letter settlement is attempted instead of scheduling a transient retry. Broker reason code/string remain in diagnostics.

## MQTT-012-AC-03 — Other rejection or network failure uses durable retry budget
**Given** another unsuccessful publish reason, or an unclassified network/connect/publish exception, **When** a durable attempt fails below its configured retry limit, **Then** shared retry handling can schedule an owned retry; exhaustion can dead-letter. There is no second adapter retry around the publish call and no universal maximum external-effect count is asserted.

## MQTT-012-AC-04 — Missing route or mapping prevents successful publication
**Given** a retained target whose current route is absent, or whose stored source/map/contract is incompatible, **When** MQTT dispatch resolves/maps it, **Then** it fails as permanent `outbox.dispatch.failed` before successful publication/completion; it does not select a different route by topic or target-ID prefix.
