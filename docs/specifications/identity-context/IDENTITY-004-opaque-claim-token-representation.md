# IDENTITY-004 — Opaque claim token representation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** ClaimId.Create generates GUID N strings; IDurableStore consumes a string ownership token.

Pre: use the supplied claim-token factory. Post: an opaque 32-character GUID representation is returned, without encoding event identity or promising cryptographic authentication.

## IDENTITY-004-AC-01 — Usable token
**Given** the public claim-token factory.
**When** a token is created.
**Then** it is a 32-character N-format GUID string accepted by the standard claim field representation.
