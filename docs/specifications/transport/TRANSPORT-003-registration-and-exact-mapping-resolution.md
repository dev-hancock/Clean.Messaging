# TRANSPORT-003 — Registration and exact mapping resolution

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Host developers need custom mapping composition to retain their chosen services. Callers need the requested type pair to select one mapping rather than an accidental fallback.

Authority: Public typed AddIngress/AddDispatch extension contracts, exact IngressMap/DispatchMap keys and deliberate customization extension points; coding standards protect established registration compatibility. Construction-time conflict detection has no independent approval. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Adding ingress or dispatch mapping MUST preserve an earlier compatible serializer and concrete mapper registration.
- Registered exact type pairs MUST select their configured mapping; an absent pair MUST fail without inventing a polymorphic fallback.
- Null service collections MUST be rejected. Mapping registration alone MUST NOT imply durable admission, consumer processing or external publication readiness.
- Ambiguous mappings MUST NOT silently select a precedence winner; their validation phase remains unresolved.

## Contract boundaries

The host separately supplies durable services, contracts, stores, consumers and connectivity. Generic variance does not change registry key semantics. Ambiguity must prevent successful use; this does not choose a universal registration/startup validation phase.

## Acceptance criteria

### TRANSPORT-003-AC-01 — Custom serializer and mapper registration are retained

- **Given** earlier compatible custom serializer and concrete mapper services.
- **When** mapping services are added and used.
- **Then** the earlier services remain selected rather than being replaced by defaults.

### TRANSPORT-003-AC-02 — Exact type pair chooses mapping

- **Given** registered A-to-B and A-to-C pairs but no subclass-of-A-to-B pair.
- **When** the exact pairs and the unregistered pair are requested.
- **Then** each registered pair uses its map and the missing pair fails without variance fallback.

### TRANSPORT-003-AC-04 — Invalid composition is not durable readiness

- **Given** a null service collection or only mapping services without durable dependencies.
- **When** registration is invoked on null or dependent admission/dispatch is requested.
- **Then** null fails argument validation and mapping-only composition supplies neither admission, consumer processing nor publication.

## Open decisions

The original IDs and heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### TRANSPORT-003-AC-03 — Duplicate pair has no precedence winner

When and with which diagnostic contract are duplicate pairs rejected? The original composite also covers repeated-identical registration, so its ID is reserved intact rather than narrowed to ambiguity prevention. See [GAP-TX-008](../GAPS.md#gap-tx-008); the recommendation remains unapproved.
