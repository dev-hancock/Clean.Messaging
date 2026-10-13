# RETRY-005 — Processing settlement boundary

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §7 and §9 preserve separate failure settlement and host/claim cancellation; processors use classified policy decisions after unsuccessful attempts.

Pre: active durable processor with local unsuccessful attempt. Post: retry/dead-letter metadata can be recorded independently of rolled-back consumer work, but only by current ownership.

## RETRY-005-AC-01 — Failure rollback and retry
**Given** consumer state rolls back while claim remains valid and policy permits retry.
**When** failure settlement succeeds.
**Then** a later due attempt is recorded without committing the failed consumer changes.

## RETRY-005-AC-02 — Lost ownership
**Given** the attempt fails after loss is signalled.
**When** processor handles that failure.
**Then** it does not overwrite successor state with retry/dead-letter metadata.

## RETRY-005-AC-03 — Host cancellation
**Given** handling throws cancellation due to the host stopping token.
**When** processor exits.
**Then** the target is left recoverable by expiry rather than dead-lettered solely because of shutdown.

## RETRY-005-AC-04 — Settlement timeout
**Given** failure settlement cannot complete within its separate configured timeout.
**When** the bound elapses.
**Then** no successful retry/dead-letter settlement is claimed; durable state must be inspected/recovered later.
