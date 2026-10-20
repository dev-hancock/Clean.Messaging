# CONFIG-005 — Persistence-context composition constraints

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts combining durable capabilities need predictable persistence composition. A capability must not silently change the messaging context selection established by another capability in the same service collection.

## Guarantees

- Standard persistence registration MUST accept reuse of the selected messaging context type by another capability.
- Selecting a different messaging context type in the same service collection MUST fail clearly instead of silently mixing persistence contexts.

## Contract boundaries

The existing single-type constraint applies to standard messaging composition in one service collection. Applications may use other `DbContext` types for unrelated work; this is not a universal single-context architecture rule.

Matching context types does not by itself establish shared instances, transaction participation or atomicity, particularly for custom overrides or writes through unrelated contexts. The [persistence reference](../../reference/persistence.md) describes the current integration requirements.

The single-type rule defines the supported standard composition contract, not a requirement that every future integration use one context type. Supporting independently selected messaging contexts within one service collection would require a separate design and compatibility decision; it is not part of this contract.

## Acceptance criteria

### CONFIG-005-AC-01 — Consistent context registration

- **Given** a collection already selects context A.
- **When** another capability selects context A.
- **Then** the persistence type check accepts it.

### CONFIG-005-AC-02 — Conflicting context registration

- **Given** a collection already selects context A.
- **When** a capability selects different context B.
- **Then** registration fails clearly rather than silently mixing standard messaging contexts.

## Open decisions

None.
