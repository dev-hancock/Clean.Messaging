# CONFIG-004 — Startup validation and configuration binding

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Hosts need incomplete registrations and invalid operational settings detected before ordinary processing. Default configuration and section binding must be predictable without claiming operational readiness.

## Guarantees

- Consumer startup validation MUST reject a missing message contract or concrete consumer-service registration.
- AddMessaging(configuration) MUST bind core retry configuration from Messaging:Retry without implicitly binding unrelated capability options.
- For nonnull common option groups, validation MUST reject nonpositive lease/processing/recovery/cleanup settings, renewal not shorter than duration, and failure maximum below recovery interval, and MUST return all applicable failures.
- Unmodified public MessageOptions and RetryOptions defaults MUST pass their standard validation and preserve the default values stated in AC-06.

## Contract boundaries

Startup registration checks do not execute consumers or probe resources. The host selects feature registration/binding separately. Null nested groups and undefined retry modes remain unresolved under CONFIG-006; this page does not approve their proposed handling. Public defaults are configuration compatibility values, not recommended universal sizing.

## Acceptance criteria

### CONFIG-004-AC-01 — Missing consumer contract

- **Given** a consumer is registered but its message contract is absent.
- **When** consumer startup validation runs.
- **Then** startup fails before successful normal consumer processing.

### CONFIG-004-AC-02 — Missing consumer service

- **Given** a provider-created invoker references a service absent from the container.
- **When** consumer startup validation runs.
- **Then** startup fails with the missing service registration condition.

### CONFIG-004-AC-03 — Core section binding

- **Given** IConfiguration contains Messaging:Retry and unrelated capability sections.
- **When** core AddMessaging(configuration) is used.
- **Then** retry values are bound from Messaging:Retry and unrelated feature options are not implicitly bound.

### CONFIG-004-AC-04 — Common invalid options

- **Given** nonpositive lease/processing/recovery/cleanup values, renewal >= duration or failure maximum < recovery interval.
- **When** common MessageOptions validation runs.
- **Then** validation reports the offending constraints instead of declaring options valid.

### CONFIG-004-AC-05 — Aggregated option failures

- **Given** several independently invalid nonnull common option groups.
- **When** validation runs.
- **Then** all applicable validation failures are returned, not only the first.

### CONFIG-004-AC-06 — Valid defaults

- **Given** new common MessageOptions and RetryOptions.
- **When** their standard validation runs.
- **Then** the documented defaults pass; common defaults are lease 30s/renewal 10s, concurrency 4/capacity 1024/batch 100/settlement 5s, recovery 10s/max 1m, cleanup 1h/retention 7d/batch 500; retry defaults are 5 attempts/2s initial/1m cap/exponential/full jitter.

## Open decisions

[GAP-CORE-003](../GAPS.md#gap-core-003) covers null nested options and undefined retry modes.
