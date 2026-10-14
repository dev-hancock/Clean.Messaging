# MQTT-009 — Outgoing target and destination staging

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Outgoing hosts need target identity and destination fixed before durable commit, with payload mapping performed by later publication attempts.

Authority: Deliberate public On<TMessage>(targetId)/Publish original-message destination callback and durable target representation. Historical documentation deliberately promises stored destinations; current APIs independently agree. Deliberate target identity/storage constraints (<=200 ASCII) are protected by coding standards on public compatibility; the standards do not themselves prescribe that numeric boundary. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Staging MUST select destination from the original message and retain destination, transport mqtt and target ID for later mapped publication.
- Invalid target identities, null callbacks and duplicate target identities MUST prevent ambiguous work.
- A base-type route MUST NOT silently contribute a MQTT target for an unregistered concrete subtype.

## Contract boundaries

Target IDs are ordinal, nonblank ASCII, at most 200 characters and unique across local/external targets in a batch. Destination is not target identity. Staging does not publish or prove global fan-out success. Topic validation and retained map/retain/route migration remain decisions.

## Acceptance criteria

### MQTT-009-AC-01 — Destination is selected before persistence, mapping later

- **Given** an A-to-B map and destination callback based on A.
- **When** A is staged then dispatched after commit.
- **Then** stored work carries the destination evaluated from A, transport mqtt and route ID; publication later carries mapped B.

### MQTT-009-AC-02 — Target identity validation and duplicates prevent ambiguous work

- **Given** blank/non-ASCII/201-character target ID, duplicate completed route ID, null callback or local-target collision.
- **When** registration/staging reaches validation.
- **Then** argument/configuration failure prevents ambiguous work; valid 200-character ASCII identity remains allowed.

### MQTT-009-AC-05 — Exact-type target matching does not imply polymorphic publication

- **Given** a base-A route and subclass B without a MQTT route.
- **When** targets for B are enumerated.
- **Then** the A route contributes no MQTT target; other providers/consumers still determine overall staging success.

## Open decisions

The original IDs and exact heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### MQTT-009-AC-03 — Blank destination fails staging; topic grammar has another boundary

Where is topic grammar validated and which staging errors are public contracts? The original combines blank/callback failure with acceptance of other invalid topics; reserve it intact. Decision [GAP-INTENT-TX-002](../GAPS.md#gap-intent-tx-002) remains unapproved.

### MQTT-009-AC-04 — Retries preserve destination but current route can change payload or retain

Which current-route/map/retain changes remain compatible with retained work? Stored destination authority does not approve arbitrary payload/retain changes or target-ID migration. See [GAP-TX-003](../GAPS.md#gap-tx-003); the recommendation remains unapproved.
