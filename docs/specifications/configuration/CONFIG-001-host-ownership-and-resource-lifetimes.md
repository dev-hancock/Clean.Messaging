# CONFIG-001 — Host ownership and resource lifetimes

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Applications must compose messaging with their own persistence and resource lifetime. A library registration must not take over infrastructure provisioning or keep an attempt's scoped state alive for the host lifetime.

## Guarantees

- Core messaging registration MUST leave DbContext/provider selection, schema provisioning and deployment to the host.
- Library singleton and hosted services MUST NOT capture scoped consumers, stores or contexts for the host lifetime.
- Independent processing attempts MUST have independently disposable operation-scoped resources.

## Contract boundaries

The host supplies the scoped persistence context, provider, mappings/interceptors, deployed schema and capability services. Successful composition is not proof of database or broker readiness. These lifetime obligations apply to the library's registrations, not arbitrary host overrides that violate scope rules.

## Acceptance criteria

### CONFIG-001-AC-01 — Host-owned persistence setup

- **Given** only core messaging services have been registered.
- **When** the service collection is examined or built.
- **Then** no application DbContext/provider/schema is provisioned merely by AddMessaging.

### CONFIG-001-AC-02 — Valid scoped host

- **Given** a host uses scope validation with all required capability services and its scoped DbContext.
- **When** the capability hosted services are resolved and started.
- **Then** no singleton-to-scoped dependency capture is required for successful composition.

### CONFIG-001-AC-03 — Separate operation scopes

- **Given** a correctly composed durable processing host handles successive independent attempts.
- **When** consumer/context dependencies are observed through host-visible lifetimes.
- **Then** attempt-scoped resources can be disposed independently and are not retained for the entire host lifetime.

## Open decisions

[GAP-DUR-005](../GAPS.md#gap-dur-005) records the cleanup lifetime mismatch. The no-capture requirement remains established despite the current implementation mismatch.
