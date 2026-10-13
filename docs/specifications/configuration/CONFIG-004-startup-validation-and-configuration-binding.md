# CONFIG-004 — Startup validation and configuration binding

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** Core and shared persistence options validators and ConsumerValidation startup checks.

Pre: complete feature registrations and configured options. Post: specified invalid values/consumer registrations fail startup; binding is section-specific rather than universal.

## CONFIG-004-AC-01 — Missing consumer contract
**Given** a consumer is registered but its message contract is absent.
**When** consumer startup validation runs.
**Then** startup fails before successful normal consumer processing.

## CONFIG-004-AC-02 — Missing consumer service
**Given** a provider-created invoker references a service absent from the container.
**When** consumer startup validation runs.
**Then** startup fails with the missing service registration condition.

## CONFIG-004-AC-03 — Core section binding
**Given** IConfiguration contains Messaging:Retry and unrelated capability sections.
**When** core AddMessaging(configuration) is used.
**Then** retry values are bound from Messaging:Retry and unrelated feature options are not implicitly bound.

## CONFIG-004-AC-04 — Common invalid options
**Given** nonpositive lease/processing/recovery/cleanup values, renewal >= duration or failure maximum < recovery interval.
**When** common MessageOptions validation runs.
**Then** validation reports the offending constraints instead of declaring options valid.

## CONFIG-004-AC-05 — Aggregated option failures
**Given** several independently invalid nonnull common option groups.
**When** validation runs.
**Then** all applicable validation failures are returned, not only the first.

## CONFIG-004-AC-06 — Valid defaults
**Given** new common MessageOptions and RetryOptions.
**When** their standard validation runs.
**Then** the documented defaults pass; common defaults are lease 30s/renewal 10s, concurrency 4/capacity 1024/batch 100/settlement 5s, recovery 10s/max 1m, cleanup 1h/retention 7d/batch 500; retry defaults are 5 attempts/2s initial/1m cap/exponential/full jitter.
