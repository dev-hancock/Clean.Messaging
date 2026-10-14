# OPS-005 — Settlement log attribution

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Operators must not confuse a diagnostic statement about intended settlement with confirmed durable state. The choice of phase-accurate log wording needs a deliberate contract while database outcomes and metric observations retain their own meaning.

## Guarantees

- Operational log text MUST NOT be treated as durable confirmation of retry or dead-letter settlement. A failed or rejected store transition MUST NOT acquire successful durable status merely because a log was emitted.

## Contract boundaries

This specification does not mandate new log templates, event IDs, message order or post-success logs. The existing metric contract is in OPS-004. Whether settlement intent and confirmed outcome should have distinct operational messages remains an unapproved policy decision.

## Acceptance criteria

No acceptance-criterion IDs are assigned to the unresolved policies on this page.

## Open decisions

- [GAP-CORE-007](../GAPS.md#gap-core-007): the related policy remains undecided; its recommendation is not approved.
