# INBOX-011 — Composition, mappings and public options

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Host authors need a predictable public composition contract for durable Inbox processing and clear separation between valid service/model configuration and an actually installed persistence schema.

## Guarantees

- Service and builder registration MUST preserve callback configuration and the Inbox configuration-section binding for shared durable options.
- Supported EF composition MUST require a relational configured context, both admission and target mappings, a resolvable processor and one messaging context type shared with Outbox.
- Options MUST reject nonpositive lease, renewal, concurrency, capacity, batch, settlement, recovery, cleanup and retention settings; renewal MUST be shorter than its lease and maximum recovery delay MUST not be shorter than the recovery interval.
- Model/service validation MUST NOT be presented as schema installation, migration execution or certification of every relational provider.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Coding standards sections 6 and 14 for public registration compatibility, lifetime ownership and explicit validation; provider-neutral/EF responsibilities are defined by the architecture boundary.

## Contract boundaries

The consumer host owns the scoped context, provider, model installation, database schema/migrations and lifetime-compatible custom services. Inbox composition does not install an outgoing capture transaction merely by admitting incoming work. Cleanup lifetime safety is an independent requirement in [INBOX-012](INBOX-012-scoped-cleanup-composition-requirement-and-current-mismatch.md).

## Acceptance criteria

### INBOX-011-AC-01 — Bind configure and configuration overloads

- **Given** a host uses the callback, configuration-section or builder admission registration with its context.
- **When** configured options resolve and durable composition is validated.
- **Then** the requested Inbox settings bind; a nonrelational or missing admission/entry model, missing processor, or conflicting messaging context type fails rather than suggesting complete integration.

### INBOX-011-AC-02 — Require positive valid shared settings

- **Given** shared durable settings contain a nonpositive required duration/count, renewal at least as long as its lease, or recovery maximum below its interval.
- **When** options validation runs.
- **Then** invalid settings fail, and otherwise valid positive relationships are accepted.

### INBOX-011-AC-03 — Do not infer installed schema from model validation

- **Given** the relational model and services are configured but the database schema is missing or incompatible.
- **When** composition validation succeeds and persistence is later attempted.
- **Then** model validation supplies no schema certification and actual persistence can still fail.

## Open decisions

None.
