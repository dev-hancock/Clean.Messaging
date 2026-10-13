# CONFIG-001 — Host ownership and resource lifetimes

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §3, §6 and §16 and repository instructions explicitly assign host composition and scoped-service ownership.

Pre: correctly composed host. Post: no library startup implicitly provisions resources or captures a scoped context for a singleton lifetime. Current cleanup wiring conflicts with the latter; see GAPS rather than treating observed wiring as required.

**Open decisions:** [GAP-DUR-005](../GAPS.md#gap-dur-005).

## CONFIG-001-AC-01 — Host-owned persistence setup
**Given** only core messaging services have been registered.
**When** the service collection is examined or built.
**Then** no application DbContext/provider/schema is provisioned merely by AddMessaging.

## CONFIG-001-AC-02 — Valid scoped host
**Given** a host uses scope validation with all required capability services and its scoped DbContext.
**When** the capability hosted services are resolved and started.
**Then** no singleton-to-scoped dependency capture is required for successful composition.

## CONFIG-001-AC-03 — Separate operation scopes
**Given** a correctly composed durable processing host handles successive independent attempts.
**When** consumer/context dependencies are observed through host-visible lifetimes.
**Then** attempt-scoped resources can be disposed independently and are not retained for the entire host lifetime.
