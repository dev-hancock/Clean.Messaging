# CONFIG-003 — Registration identifier boundaries

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** AddContract and AddConsumer argument checks. The exact size/ASCII limits are source-observed constraints, not inferred internationalization policy.

Pre: extension registration receives an identity. Post: accepted contract names are nonblank <=500 chars; consumer IDs are nonblank <=200 ASCII chars. Duplicate resolution is CORE-002.

## CONFIG-003-AC-01 — Contract boundary
**Given** nonblank contract names of length 500 and 501.
**When** each is registered separately.
**Then** 500 is accepted by the extension and 501 is rejected before a successful registration.

## CONFIG-003-AC-02 — Consumer boundary
**Given** nonblank ASCII IDs of length 200/201 and an ID containing a character above ASCII 127.
**When** each is registered separately.
**Then** 200 ASCII is accepted; 201 or non-ASCII is rejected.

## CONFIG-003-AC-03 — Blank identities
**Given** a null, empty or whitespace-only name or consumer ID.
**When** registration is requested.
**Then** argument validation fails rather than registering an empty identity.
