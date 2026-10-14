# OUTBOX-002 — Ambient outgoing metadata and representation members

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Consumers producing outgoing work need correlation continuity and immediate causal identity, while explicit overrides and later persisted delivery must not be replaced by unrelated ambient state.

## Guarantees

- Explicit nonnull correlation and causation values MUST take precedence over ambient values.
- Absent outgoing correlation MUST inherit the current correlation when available. Absent causation MUST inherit the immediate current MessageId, not the parent's CausationId.
- Without an explicit or ambient value, metadata MUST remain absent.
- External delivery MUST carry persisted identity, contract, payload and nullable correlation/causation values, not recompute them from a later ambient context.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Public nullable outgoing metadata compatibility and coding standards sections 9 and 14 for stable persisted relationships; MessageContext defines immediate delivery identity rather than ancestor causation.

## Contract boundaries

Nullable input means inherit when context exists; it is not an explicit clearing instruction. Correlation and causation are relationships, not deduplication keys. Nongeneric OutboxMessage members describe the envelope but do not add a nongeneric Enqueue operation.

## Acceptance criteria

### OUTBOX-002-AC-01 — Inherit or override outgoing context

- **Given** outgoing metadata is either explicitly supplied or null, with an ambient delivery/correlation or no ambient context.
- **When** the outgoing envelope is staged.
- **Then** nonnull overrides win, null correlation inherits available current correlation, null causation inherits available immediate MessageId rather than ancestor causation, and unavailable values remain null.

### OUTBOX-002-AC-02 — Carry persisted metadata to external port

- **Given** retained external work has persisted identity/payload and nullable metadata while another ambient context exists.
- **When** it is delivered through the external transport port.
- **Then** the published envelope retains its delivery/event/target IDs, destination, contract, payload and nullable correlation/causation rather than adopting the other context; arbitrary headers and content type are not automatically added by the core envelope.

## Open decisions

None.
