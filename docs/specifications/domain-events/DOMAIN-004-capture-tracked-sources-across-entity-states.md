# DOMAIN-004 — Capture tracked sources across entity states

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications need capture to follow their selected pending facts on tracked event sources, not an incidental entity-dirty flag or a requirement to inherit the supplied aggregate base. Recipients need the concrete message contract and logical event identity to survive that boundary.

## Guarantees

- Valid pending facts from tracked IDomainEventSource entities MUST participate in capture regardless of Added, Modified, Unchanged or Deleted state. Untracked sources MUST NOT be discovered by this context’s capture.
- Capture MUST preserve the fact’s ID as EventId and serialize its concrete payload contract. The selected targets MUST share a new delivery identity for that capture batch.
- An empty pending collection MUST NOT create outgoing work or acknowledgements merely because its source is tracked.

## Contract boundaries

The application supplies a stable, valid pending collection and payload during save. The context and sources have one owner; this integration does not supply concurrent collection synchronization or an atomic domain snapshot. Source-ID collisions and additional custom-source guarantees are not resolved by this contract.

## Acceptance criteria

### DOMAIN-004-AC-01 — Capture unchanged and custom tracked sources

- **Given** valid pending facts exist on custom and aggregate sources tracked as Added, Modified, Unchanged or Deleted, and on other untracked sources.
- **When** the intercepted owning save captures facts.
- **Then** all tracked sources contribute their facts regardless of entity state and untracked sources contribute none.

### DOMAIN-004-AC-02 — Serialize concrete event payload

- **Given** a pending RaisedEvent contains a registered concrete payload through object.
- **When** capture is staged and the owning save commits.
- **Then** outgoing work carries the concrete contract/payload, the original ID as EventId and a new delivery identity shared by its selected targets.

### DOMAIN-004-AC-03 — Keep empty collections inert

- **Given** tracked sources have empty pending collections.
- **When** the owning save commits.
- **Then** tracking alone creates neither domain outgoing entries nor domain acknowledgements.

### DOMAIN-004-AC-04 — Do not authorize concurrent source mutation

- **Given** a caller mutates a live pending collection or payload concurrently with save capture.
- **When** the caller observes the capture attempt.
- **Then** no synchronized or atomic domain snapshot is guaranteed; enumeration or serialization may fail or reflect the source’s mutable view.

## Open decisions

- [GAP-DUR-010](../GAPS.md#gap-dur-010): the related policy remains undecided; its recommendation is not approved.
