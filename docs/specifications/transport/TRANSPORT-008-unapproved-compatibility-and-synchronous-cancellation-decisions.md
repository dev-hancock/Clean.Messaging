# TRANSPORT-008 — Unapproved compatibility and synchronous cancellation decisions

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Deployment owners need compatibility commitments separate from serializer settings, and attempt owners need cancellation semantics separate from accidental payload failure policy.

Authority: Coding standards establish compatibility ownership and failure/cancellation separation, but do not select a schema support matrix or synchronous cancellation classification. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Serialized/public compatibility MUST have producer/consumer ownership and an explicit approach for breaking changes across retained data and peers.
- Failure and cancellation MUST remain distinct contractual concerns; uncertain external outcomes MUST NOT prove exactly-once effects.

## Contract boundaries

These obligations choose no upcasting, negotiation, rolling-deployment support or cancellation algorithm. No acceptance IDs were assigned to the original questions; concrete scenarios await contractual decisions.

## Acceptance criteria

No acceptance scenarios are assigned to the unresolved policy choices below.

## Open decisions

- **Retained payload compatibility:** Which earlier payload schema/contract configurations must a later deployment support? Which changes require migration or a classified rejection, and who owns the strategy? No upcasting or compatibility outcome is selected here. See [GAP-TX-003](../GAPS.md#gap-tx-003); its recommendation remains unapproved.
- **Synchronous mapping cancellation:** If a synchronous serializer/mapper throws cancellation while the attempt token is cancelled, must the boundary propagate cancellation or classify a mapping fault before admission/publication? How should user-thrown cancellation with an uncancelled attempt token differ? No classification change is prescribed here. See [GAP-TX-002](../GAPS.md#gap-tx-002); its recommendation remains unapproved.
