# CONFIG-003 — Registration identifier boundaries

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Producers and hosts need identities that fit the supported durable contract and target representations. Invalid names must fail at registration rather than appearing as usable message routes.

## Guarantees

- AddContract MUST accept nonblank names up to 500 characters and reject overlong or blank names before successful registration.
- AddConsumer MUST accept nonblank ASCII identities up to 200 characters and reject overlong, non-ASCII or blank identities before successful registration.

## Contract boundaries

These are the supported extension-registration inputs, not a general internationalization policy for every identifier. Duplicate identity selection is CORE-002. Whether direct MessageContract/custom invoker registration independently validates the same domain is unresolved in [GAP-CORE-001](../GAPS.md#gap-core-001).

## Acceptance criteria

### CONFIG-003-AC-01 — Contract boundary

- **Given** nonblank contract names of length 500 and 501.
- **When** each is registered separately.
- **Then** 500 is accepted by the extension and 501 is rejected before a successful registration.

### CONFIG-003-AC-02 — Consumer boundary

- **Given** nonblank ASCII IDs of length 200/201 and an ID containing a character above ASCII 127.
- **When** each is registered separately.
- **Then** 200 ASCII is accepted; 201 or non-ASCII is rejected.

### CONFIG-003-AC-03 — Blank identities

- **Given** a null, empty or whitespace-only name or consumer ID.
- **When** registration is requested.
- **Then** argument validation fails rather than registering an empty identity.

## Open decisions

[GAP-CORE-001](../GAPS.md#gap-core-001) covers validation at direct registry construction; extension limits are not broadened here.
