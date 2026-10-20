# DOMAIN-010 — Event-source and acknowledgement contract

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Application authors implementing custom event sources need to know which facts capture may read, how identities distinguish those facts, and what acknowledgement means. Collision handling, source stability and targetless capture require explicit policy rather than dependence on traversal order or callback behavior.

## Guarantees

- The source extension MUST leave business-event meaning with the application. Capture acknowledgement MUST NOT be presented as downstream consumer or transport completion.

## Contract boundaries

The ordinary `AggregateRoot`, valid-source capture and local commit contracts remain specified in [DOMAIN-001 through DOMAIN-009](README.md#specifications), with their own open decisions. This page identifies unresolved extension policies; it does not select collision handling, payload-mutation rules, callback error handling or targetless acknowledgement.

Source mechanics and the current capture lifecycle belong in the [domain-events reference](../../reference/domain-events.md). Neither that implementation nor recommendations in the linked decisions establish additional guarantees.

## Acceptance criteria

No acceptance-criterion IDs are assigned to the unresolved policies on this page.

## Open decisions

1. **Custom source contract:** Which identity, collection/payload stability and acknowledgement obligations apply to custom sources, and how are conflicts or callback errors exposed? See [GAP-DUR-010](../GAPS.md#gap-dur-010) and [GAP-DUR-009](../GAPS.md#gap-dur-009).
2. **Targetless acknowledgement:** What happens to a captured fact with no outgoing targets? See [GAP-DUR-011](../GAPS.md#gap-dur-011).

The linked decisions retain the alternatives and consequences. Neither policy is approved.
