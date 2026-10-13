# OUTBOX-016 — EF composition, mappings, options and validation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: EF DependencyInjection/OutboxConfiguration/OutboxValidation, project references, OutboxOptions.Section; core AddOutboxTransport. [Configuration](../../reference/configuration.md) and [architecture](../../reference/architecture.md) are descriptive.

Contracts: services AddOutbox<TDbContext>(callback? / IConfiguration), MessagingBuilder overload, generic/nongeneric DbContextOptionsBuilder.AddMessaging(options,services), ModelBuilder.AddOutbox; Section="Outbox" and inherited durable settings. Outbox factory/default local target/writer singleton; store/processor/dispatcher/tracker/interceptors scoped. Custom singleton dependencies must be lifetime-compatible. AddMessaging options returns unchanged if save interceptor absent; otherwise attaches save plus transaction interceptors and has no duplicate-instance guard. Validator checks relational provider, entity mapping, both interceptor kinds, processor resolution, not migrations/schema/provider guarantees. Inbox/outbox share configured context type.

## OUTBOX-016-AC-01 — Compose overloads and inherited options

- **Given** host uses service/builder registration callback or Outbox section configuration.
- **When** options resolve and mapped/intercepted context starts.
- **Then** requested durable settings bind; positive validation applies and missing/nonrelational model/interceptors or mismatched persistence context type fail.

## OUTBOX-016-AC-02 — Attach interceptors only when registered

- **Given** generic/nongeneric options extension is called with or without registered OutboxInterceptor.
- **When** AddMessaging(options,services) runs.
- **Then** absent interceptor leaves options unchanged; present registration attaches both kinds; repeated calls are not guaranteed duplicate-free.

## OUTBOX-016-AC-03 — Do not equate model validation with schema certification

- **Given** model includes OutboxEntry and both interceptors on relational context, but store schema is missing/incompatible.
- **When** startup validation and later SaveChanges execute.
- **Then** model validation can succeed while real persistence fails; target/transport mapping lengths and unique keys remain provider/schema obligations.
