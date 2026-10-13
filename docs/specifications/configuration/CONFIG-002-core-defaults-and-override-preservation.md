# CONFIG-002 — Core defaults and override preservation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** Core registrations use TryAdd/standard options and register consumer startup checks; persistence builder does not register context.

Pre: optional prior service overrides. Post: repeated core calls do not replace prior compatible services; registration of explicit contract/consumer entries is separate.

## CONFIG-002-AC-01 — Service override
**Given** a custom clock, serializer, registry, context or consumer service is registered before core registration.
**When** the relevant AddMessaging/AddConsumer path runs.
**Then** the prior compatible service registration is preserved.

## CONFIG-002-AC-02 — Repeated core registration
**Given** AddMessaging is called more than once without adding duplicate explicit contracts.
**When** core services are resolved.
**Then** the same default singleton services remain available without duplicate-registry entries introduced by core registration alone.

## CONFIG-002-AC-03 — Persistence builder forwarding
**Given** a builder callback configures a contract, consumer and retry options.
**When** AddMessaging<TContext> invokes it.
**Then** the callback can access Services and its registrations/configuration become available; it does not register the host context.

## CONFIG-002-AC-04 — Null builder callbacks
**Given** the required persistence callback or ConfigureRetry callback is null.
**When** the corresponding public API is invoked.
**Then** ArgumentNullException is raised rather than accepting a successful no-op.
