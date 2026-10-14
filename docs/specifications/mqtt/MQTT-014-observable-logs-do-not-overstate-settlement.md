# MQTT-014 — Observable logs do not overstate settlement

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Operators need setup, admission, ACK and cleanup diagnostics that distinguish attempted transitions from confirmed protocol or consumer success.

Authority: Coding standards on meaningful truthful diagnostics and delivery/settlement boundaries establish outcomes. Exact event IDs, levels/fields and misleading pre-ACK wording are not independently approved promises. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Diagnostics MUST distinguish attempted acknowledgement from confirmed success. Admission/broker publication MUST NOT be presented as consumer/subscriber completion.
- Setup, receipt and cleanup failures MUST remain attributable to their operational boundaries with useful causes.

## Contract boundaries

Diagnostic text is not broker/database evidence. Attempt logs cannot establish rejected/failed ACK or settlement success. Detailed event contracts need explicit authority before acceptance IDs encode them.

## Acceptance criteria

No acceptance scenarios are assigned to unresolved choices below.

## Open decisions

The original IDs and exact heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### MQTT-014-AC-01 — Poison logging can precede a failed acknowledgement

How must ACK attempt/result logs remain truthful? Reserve original event-order/misleading-wording composite, not an ID silently narrowed to new truthful-result scenarios. See [GAP-TX-007](../GAPS.md#gap-tx-007); the recommendation remains unapproved.

### MQTT-014-AC-02 — Connection and admission diagnostics retain their operational boundary

Which levels/fields are deliberate operator compatibility contracts, and how must setup/receipt/cleanup wording avoid false completion? Reserve the exact-inventory composite. See [GAP-TX-007](../GAPS.md#gap-tx-007); the recommendation remains unapproved.
