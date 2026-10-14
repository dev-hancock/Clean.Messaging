# OUTBOX-016 — EF composition, mappings, options and validation

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Host authors need stable public registration and persistence integration while retaining responsibility for provider deployment, schema installation and valid service lifetimes.

## Guarantees

- Callback, Outbox configuration-section and builder registration MUST preserve requested shared durable settings and reject invalid positive/relationship constraints.
- Supported EF integration MUST require a relational configured context, outgoing model, both save and transaction interceptor kinds and a resolvable processor; Inbox/Outbox MUST share one configured messaging context type.
- Generic and nongeneric options integration MUST leave options unchanged when capture services are absent and attach both interceptor kinds when registered; repeated attachment MUST not be represented as an idempotent guarantee.
- Successful model/service validation MUST NOT imply migrations were applied, schema exists, every provider is certified or arbitrary custom singleton dependencies are lifetime-safe.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 6 and 14 establish public composition compatibility and explicit validation; architecture separates provider-neutral staging from EF capture and consumer host ownership.

## Contract boundaries

Consumer hosts install model/options integration, scoped context and actual schema/migrations. Factory/provider/writer singleton dependencies must be lifetime-compatible; cleanup safety is specified in [OUTBOX-017](OUTBOX-017-scoped-cleanup-lifetime-requirement.md). Model constraints and unique identities require real provider/schema support beyond startup configuration.

## Acceptance criteria

### OUTBOX-016-AC-01 — Compose overloads and inherited options

- **Given** a host uses callback, Outbox section or builder registration and its configured context.
- **When** options and durable composition validate.
- **Then** requested settings bind, invalid nonpositive or lease/renewal/recovery relationships fail, and nonrelational/missing model, missing interceptor/processor or mismatched messaging context type is rejected.

### OUTBOX-016-AC-02 — Attach interceptors only when registered

- **Given** generic or nongeneric options integration is invoked with or without registered capture interception.
- **When** integration options are produced.
- **Then** absent capture registration leaves options unchanged; present registration attaches save and transaction kinds, without promising duplicate-free repeated invocation.

### OUTBOX-016-AC-03 — Do not equate model validation with schema certification

- **Given** a relational outgoing model and both interceptors exist but deployed schema is missing or incompatible.
- **When** startup validation succeeds and persistence is later attempted.
- **Then** the successful configuration check is not schema/provider certification, and persistence can fail under actual mapping lengths, unique keys or missing tables.

## Open decisions

None.
