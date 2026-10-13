# TRANSPORT-008 — Unapproved compatibility and synchronous cancellation decisions

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Unknown behaviour
**Evidence:** [transport serializer](../../../src/Clean.Messaging.Transport/TransportSerializer.cs), both mapping implementations, and coding standards §§7, 9, 14. No approved schema migration or mapper-cancellation contract was found in scoped authored files.
**Pending decisions:** [GAP-TX-002](../GAPS.md#gap-tx-002) and [GAP-TX-003](../GAPS.md#gap-tx-003). These numbered questions are not executable acceptance criteria, requirements or pass/fail promises. An owner decision must select definite observable outcomes before criteria can be authored.

1. **Retained payload compatibility:** Which earlier payload schema/contract configurations must a later deployment support? Which changes require migration or a classified rejection, and who owns the strategy? No upcasting or compatibility outcome is selected here.
2. **Synchronous mapping cancellation:** If a synchronous serializer/mapper throws cancellation while the attempt token is cancelled, must the boundary propagate cancellation or classify a mapping fault before admission/publication? How should user-thrown cancellation with an uncancelled attempt token differ? No classification change is prescribed here.
