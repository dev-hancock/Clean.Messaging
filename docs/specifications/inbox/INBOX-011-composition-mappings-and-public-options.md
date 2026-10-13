# INBOX-011 — Composition, mappings and public options

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: Inbox EF `DependencyInjection`, `InboxConfiguration`, `InboxValidation`, project references; `InboxOptions.Section`. [Configuration](../../reference/configuration.md) and [processing defaults](../../reference/processing.md) are descriptive.

Contracts: services AddInbox<TDbContext>(Action<InboxOptions>?), IConfiguration overload, MessagingBuilder overload, ModelBuilder.AddInbox; InboxOptions inherits Lease/Processing/Recovery/Cleanup with Section="Inbox". Preconditions: relational mapped schema, same scoped context type across inbox/outbox, contracts/consumers installed by host. Registrations preserve prior TryAdd services. Model keys: admission MessageId and unique EventId; entry composite MessageId/TargetId and unique EventId/TargetId, target max 200 ASCII mapping. Validation checks relational provider, both model entities and processor resolvability, not database schema/migration/provider correctness. Hosted cleanup lifetime caveat is separately specified.

## INBOX-011-AC-01 — Bind configure and configuration overloads

- **Given** a host calls the service or builder overload and uses callback settings or the Inbox configuration section.
- **When** options resolve and startup validation executes.
- **Then** requested shared durable settings are available; missing/nonrelational mappings or mismatched configured persistence context types are rejected.

## INBOX-011-AC-02 — Require positive valid shared settings

- **Given** configured nonpositive durations/concurrency/capacity/batch/settlement/retention, renewal >= lease duration or failure max delay < recovery interval.
- **When** options startup validation runs.
- **Then** invalid settings fail rather than authorize an unsafe processing schedule.

## INBOX-011-AC-03 — Do not infer installed schema from model validation

- **Given** a relational context maps admissions and entries but database tables are absent or incompatible.
- **When** startup model validation succeeds and an admission/store operation follows.
- **Then** startup success alone does not certify schema; actual persistence can fail.
