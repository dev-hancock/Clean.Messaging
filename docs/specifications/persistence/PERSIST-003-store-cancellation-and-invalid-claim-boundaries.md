# PERSIST-003 — Store cancellation and invalid claim boundaries

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** Shared durable store methods forward I/O tokens and validate positive limit/lease; time arithmetic uses caller DateTime.

Pre: explicit operation token, positive claim limit/lease and representable supplied instants. Post: invalid values do not successfully claim/delete; pre-I/O cancellation is surfaced without a fabricated success. Cancellation after an uncertain commit requires subsequent observation.

**Open decisions:** [GAP-CORE-004](../GAPS.md#gap-core-004).

## PERSIST-003-AC-01 — Invalid claim
**Given** limit <=0 or lease <=0.
**When** Claim is requested.
**Then** ArgumentOutOfRangeException is raised and no successful claim is returned.

## PERSIST-003-AC-02 — Invalid cleanup limit
**Given** cleanup limit <=0.
**When** Cleanup is requested.
**Then** ArgumentOutOfRangeException is raised rather than an unrestricted deletion.

## PERSIST-003-AC-03 — Pre-cancelled database operation
**Given** a cancelled token before a supported asynchronous store operation reaches I/O.
**When** the operation is requested.
**Then** the operation does not report successful settlement/deletion as a replacement for cancellation.
