# PERSIST-003 — Store cancellation and invalid claim boundaries

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Store callers need bounded operations to fail clearly on invalid budgets and to preserve cancellation instead of receiving fabricated database success. Invalid limits must not become unlimited acquisition or deletion.

## Guarantees

- Claim MUST reject a nonpositive limit or lease with ArgumentOutOfRangeException rather than return a successful claim.
- Cleanup MUST reject a nonpositive limit with ArgumentOutOfRangeException rather than perform unrestricted deletion.
- Asynchronous store I/O MUST honor supplied cancellation; cancellation before I/O MUST NOT be substituted with successful settlement or deletion.

## Contract boundaries

These invalid-input outcomes belong to the standard public EF durable store. Callers provide positive limits/leases and representable instants on a consistent time basis. This page does not choose UTC-kind enforcement, claim-string validation or extension-only renewal policy. Cancellation racing a database commit can leave its outcome uncertain; it is not proof that storage was unchanged.

## Acceptance criteria

### PERSIST-003-AC-01 — Invalid claim

- **Given** the requested claim limit or lease is nonpositive.
- **When** Claim is requested.
- **Then** ArgumentOutOfRangeException is raised and no successful claim is returned.

### PERSIST-003-AC-02 — Invalid cleanup limit

- **Given** the requested cleanup limit is nonpositive.
- **When** Cleanup is requested.
- **Then** ArgumentOutOfRangeException is raised instead of unrestricted deletion.

### PERSIST-003-AC-03 — Pre-cancelled database operation

- **Given** the supplied token is cancelled before a supported asynchronous store operation reaches I/O.
- **When** the operation is requested.
- **Then** it does not replace cancellation with a successful settlement or deletion result.

## Open decisions

- [GAP-CORE-004](../GAPS.md#gap-core-004): the related policy remains undecided; its recommendation is not approved.
