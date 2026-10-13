# CONFIG-005 — One messaging persistence context type

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** PersistenceServices.EnsurePersistence rejects a different type in the same collection; same type is accepted.

Pre: standard persistence registrations in one collection. Post: same type can be shared by capabilities; a different type conflicts. This check does not verify same instance/transaction for arbitrary contexts.

## CONFIG-005-AC-01 — Consistent context registration
**Given** a collection already selects context A.
**When** another capability selects context A.
**Then** the persistence type check accepts it.

## CONFIG-005-AC-02 — Conflicting context registration
**Given** a collection already selects context A.
**When** a capability selects different context B.
**Then** registration fails clearly rather than silently mixing standard messaging contexts.
