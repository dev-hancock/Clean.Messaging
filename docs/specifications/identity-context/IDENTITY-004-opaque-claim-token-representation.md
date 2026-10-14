# IDENTITY-004 — Opaque claim token representation

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Workers need a usable ownership token that is independent of message meaning. Consumers should not have to derive business identity or authorization credentials from its text.

## Guarantees

- The supplied claim-token factory MUST produce a usable token compatible with the supported claim field; ownership use MUST NOT replace logical message or event identity.

## Contract boundaries

The token is opaque ownership data, not cryptographic authentication. The exact GUID text shape has not been established as a consumer obligation; preserving the full original criterion avoids silently reducing it to only token usability.

## Acceptance criteria

No additional acceptance outcomes are assigned while the policy below remains unresolved.

## Open decisions

[GAP-INTENT-CORE-001](../GAPS.md#gap-intent-core-001) — Is GUID N-format a compatibility promise or an incidental encoding of an opaque claim token? Recommendation (unapproved): retain usable opaque tokens within the supported field capacity without requiring a GUID algorithm. If that narrower contract is selected, migrate the composite criterion explicitly rather than reusing its ID.

The following original identifiers are explicitly reserved, not normative acceptance scenarios. The original obligations remain associated with the decisions below; no narrower or opposite guarantee is assigned to those IDs.

### IDENTITY-004-AC-01 — Usable token

Reserved for [GAP-INTENT-CORE-001](../GAPS.md#gap-intent-core-001). The original ID combines usability with exact 32-character GUID N-format output. Usability remains a guarantee; the encoding clause lacks independent authority. The full ID remains reserved rather than narrowed.
