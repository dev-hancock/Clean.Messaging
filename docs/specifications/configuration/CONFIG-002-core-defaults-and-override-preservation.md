# CONFIG-002 — Public registration defaults and validation

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Host developers need predictable public registration: compatible service choices remain effective, repeated default registration is safe, and invalid required callbacks fail visibly. Persistence-builder configuration must reach the host's service collection without taking ownership of its context.

## Guarantees

- Core/consumer registration MUST preserve prior compatible registrations for the default clock, context, registries, serializer and consumer services.
- Repeated core registration alone MUST leave the default services available without introducing duplicate default services or explicit contract/consumer entries.
- The public persistence builder MUST expose the host service collection through `Services` and make its supported contract, consumer and retry configuration available there. It MUST NOT implicitly register the host `DbContext`.
- A null required persistence-builder or `ConfigureRetry` callback MUST raise `ArgumentNullException`, not be accepted as a successful no-op.

## Contract boundaries

Overrides are registered before the relevant default registration and remain responsible for the substituted contract and lifetime. Explicit duplicate contracts/consumer identities are still rejected under [CORE-002](../core-contracts/CORE-002-registry-uniqueness-and-selection.md); safe repetition of core defaults does not make explicit registrations idempotent.

The callback failure type and exposed `Services` collection are public compatibility outcomes retained by the existing criteria. No particular guard helper, internal forwarding sequence, validation order or exception message is required. Registration does not establish database provisioning or successful startup.

This contract covers configuration before service use. Mutable options do not imply support for live mutation or reload.

## Acceptance criteria

### CONFIG-002-AC-01 — Service override

- **Given** a custom clock, serializer, registry, context or consumer service is registered before core registration.
- **When** the relevant AddMessaging/AddConsumer path runs.
- **Then** the prior compatible service registration is preserved.

### CONFIG-002-AC-02 — Repeated core registration

- **Given** AddMessaging is called more than once without adding duplicate explicit contracts.
- **When** core services are resolved.
- **Then** the same default singleton services remain available without duplicate-registry entries introduced by core registration alone.

### CONFIG-002-AC-03 — Persistence builder forwarding

- **Given** a host uses AddMessaging<TContext> with a callback registering contracts, consumers and retry options.
- **When** the callback configures the exposed Services/builder.
- **Then** those registrations and settings are available from the host collection without an implicitly registered host context.

### CONFIG-002-AC-04 — Null builder callbacks

- **Given** the required persistence callback or ConfigureRetry callback is null.
- **When** the corresponding public API is invoked.
- **Then** ArgumentNullException is raised rather than accepting a successful no-op.

## Open decisions

[GAP-CORE-009](../GAPS.md#gap-core-009) leaves live options/serializer reconfiguration unresolved.
