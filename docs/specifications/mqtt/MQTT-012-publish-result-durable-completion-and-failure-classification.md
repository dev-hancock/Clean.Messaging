# MQTT-012 — Publish result, durable completion and failure classification

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Outbox operators need broker results distinguished from subscriber effects, target settlement and policy-controlled retry/permanent failure.

Authority: Coding standards on external/local separation, ownership, diagnostics and public outbox.dispatch.failed compatibility; deliberate durable-owned retry supports no second loop. Coding standards section 7 explicitly requires preserving existing error codes and transient/permanent classifications; the deliberate named broker-failure taxonomy therefore remains normative. Retry accounting remains undecided. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- A successful adapter publication result MUST precede attempted current-owner completion; rejected local settlement MUST NOT become successful completion.
- Rejection/network failure MUST NOT become successful publication/completion. The adapter MUST NOT add a second retry loop around durable attempts.
- Existing broker failure codes/actions MUST preserve their meaning and diagnostic reasons.
- Missing route and incompatible stored source/map/contract MUST fail permanent construction without fallback route selection or successful publication.

## Contract boundaries

A successful PublishAsync result satisfies the adapter's publication-result boundary, not application consumption. At QoS 0, return does not prove broker acknowledgement or receipt; QoS 1/2 protocol acknowledgement still does not prove subscriber effects. Completion can fail afterward and publication can repeat. Missing unchanged configuration fails permanent construction; this does not choose a route migration policy. The deliberate failure taxonomy below is preserved; retry budget and synchronous cancellation remain distinct decisions.

| Broker rejection | Stable failure code | Action |
| --- | --- | --- |
| NotAuthorized | mqtt.publish.not_authorized | Fault |
| TopicNameInvalid | mqtt.publish.topic_name_invalid | Fault |
| PayloadFormatInvalid | mqtt.publish.payload_format_invalid | Fault |
| Other rejected publish result | mqtt.publish.rejected | Retry |

The action classifies an entry attempt, not an assurance that operator reconfiguration cannot remedy the cause. Failure-time retry exhaustion is a separate accounting question.

## Acceptance criteria

### MQTT-012-AC-01 — Successful publication returns for owned settlement

- **Given** valid owned work and a successful publish result.
- **When** dispatch returns to the external wrapper.
- **Then** target/key/current-claim completion is attempted without waiting for recipient effects, and false settlement is not manufactured as database success.

### MQTT-012-AC-02 — Permanent publish rejections do not receive transient retries

- **Given** owned work rejected by the broker with NotAuthorized, TopicNameInvalid or PayloadFormatInvalid.
- **When** durable failure handling classifies the publication failure.
- **Then** its stable code/action follows the declared failure table and current-owner dead-letter settlement is attempted instead of transient retry; broker reason code/string remain diagnostic context.

### MQTT-012-AC-04 — Missing route or mapping prevents successful publication

- **Given** retained work with missing current route or incompatible source/map/contract.
- **When** resolution/mapping is requested.
- **Then** permanent outbox.dispatch.failed prevents successful publication/completion without topic or target-prefix fallback.

## Open decisions

The original IDs and exact heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### MQTT-012-AC-03 — Other rejection or network failure uses durable retry budget

Which attempt accounting/budget governs durable retry/exhaustion? Reserve the composite rather than silently narrow its ID to the established failure taxonomy/no-second-retry guarantees. See [GAP-CLAIM-001](../GAPS.md#gap-claim-001); the recommendation remains unapproved.
