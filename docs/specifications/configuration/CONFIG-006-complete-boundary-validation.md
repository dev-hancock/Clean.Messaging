# CONFIG-006 — Complete boundary validation

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need a clear decision on whether missing option groups and undefined retry modes are reported as startup configuration failures, rather than being discovered incidentally during processing.

## Guarantees

- The established nonnull option-domain validation of [CONFIG-004](CONFIG-004-startup-validation-and-configuration-binding.md) MUST remain intact; this page does not approve additional invalid-input outcomes.

## Contract boundaries

Clear boundary validation is established guidance, but the exact outcome and timing for null groups and undefined enums are an existing unapproved proposal. The original criterion identifiers are retained for that proposal, not converted into executable requirements.

## Acceptance criteria

No additional acceptance outcomes are assigned while the policy below remains unresolved.

## Open decisions

[GAP-CORE-003](../GAPS.md#gap-core-003) retains the proposed startup-validation policy. Recommendation remains unapproved: reject missing groups with named options failures and reject undefined backoff before attempts begin.

The following original identifiers are explicitly reserved, not normative acceptance scenarios. The original obligations remain associated with the decisions below; no narrower or opposite guarantee is assigned to those IDs.

### CONFIG-006-AC-01 — Missing nested options

Reserved for [GAP-CORE-003](../GAPS.md#gap-core-003). A named startup options-validation failure for a missing Lease/Processing/Recovery/Cleanup group remains an unapproved proposal.

### CONFIG-006-AC-02 — Undefined retry mode

Reserved for [GAP-CORE-003](../GAPS.md#gap-core-003). Startup rejection of an undefined RetryBackoff mode before any attempt remains an unapproved proposal.
