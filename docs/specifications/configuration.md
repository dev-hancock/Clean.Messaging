# Configuration, validation and host composition — Behavioural specification

**Status:** Draft; source reviewed 2026-10-13; no acceptance criterion has been executed.
**Scope:** Core AddMessaging overloads, AddContract/AddConsumer; MessagingBuilder<T>.Services/AddContract/AddConsumer/ConfigureRetry; PersistenceServices.EnsurePersistence/AddOptions; MessageOptions and nested options, MessageOptionsValidator; feature registrars and validators are traced in their capability specifications.

## Purpose
Compose library capabilities with explicit host-owned contracts, scoped persistence and resources, rejecting specified invalid configurations without implying operational readiness.

## Contracts, preconditions and postconditions
Pre: host supplies provider, DbContext, mappings, applicable interceptors, deployed schema, consumers/routes and running service lifetime. Post: successful registration makes configured library services available, not an operational broker or database. Registration and option/model/interceptor validation occur at distinct boundaries.

## Invariants
- A singleton must not retain a scoped dependency (coding standards §6).
- Invalid critical configuration must fail clearly at an appropriate boundary.
- Provider-neutral consumer contracts do not require EF/MQTT/Quartz representations.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are counted separately as gaps, not executable requirements.

## Limitations
- No provisioning/migration execution, universal hot reload, provider certification or host framework is supplied.
- Public options remain mutable; modifying already-consumed objects is not a documented reconfiguration protocol.

## Specifications and acceptance criteria

### CONFIG-001 — Host ownership and resource lifetimes
**Classification:** Required behaviour
**Evidence:** Coding standards §3, §6 and §16 and repository instructions explicitly assign host composition and scoped-service ownership.

Pre: correctly composed host. Post: no library startup implicitly provisions resources or captures a scoped context for a singleton lifetime. Current cleanup wiring conflicts with the latter; see GAPS rather than treating observed wiring as required.

**Open decisions:** [GAP-DUR-005](GAPS.md#gap-dur-005).

#### CONFIG-001-AC-01 — Host-owned persistence setup
**Given** only core messaging services have been registered.
**When** the service collection is examined or built.
**Then** no application DbContext/provider/schema is provisioned merely by AddMessaging.

#### CONFIG-001-AC-02 — Valid scoped host
**Given** a host uses scope validation with all required capability services and its scoped DbContext.
**When** the capability hosted services are resolved and started.
**Then** no singleton-to-scoped dependency capture is required for successful composition.

#### CONFIG-001-AC-03 — Separate operation scopes
**Given** a correctly composed durable processing host handles successive independent attempts.
**When** consumer/context dependencies are observed through host-visible lifetimes.
**Then** attempt-scoped resources can be disposed independently and are not retained for the entire host lifetime.

### CONFIG-002 — Core defaults and override preservation
**Classification:** Observed behaviour
**Evidence:** Core registrations use TryAdd/standard options and register consumer startup checks; persistence builder does not register context.

Pre: optional prior service overrides. Post: repeated core calls do not replace prior compatible services; registration of explicit contract/consumer entries is separate.

#### CONFIG-002-AC-01 — Service override
**Given** a custom clock, serializer, registry, context or consumer service is registered before core registration.
**When** the relevant AddMessaging/AddConsumer path runs.
**Then** the prior compatible service registration is preserved.

#### CONFIG-002-AC-02 — Repeated core registration
**Given** AddMessaging is called more than once without adding duplicate explicit contracts.
**When** core services are resolved.
**Then** the same default singleton services remain available without duplicate-registry entries introduced by core registration alone.

#### CONFIG-002-AC-03 — Persistence builder forwarding
**Given** a builder callback configures a contract, consumer and retry options.
**When** AddMessaging<TContext> invokes it.
**Then** the callback can access Services and its registrations/configuration become available; it does not register the host context.

#### CONFIG-002-AC-04 — Null builder callbacks
**Given** the required persistence callback or ConfigureRetry callback is null.
**When** the corresponding public API is invoked.
**Then** ArgumentNullException is raised rather than accepting a successful no-op.

### CONFIG-003 — Registration identifier boundaries
**Classification:** Observed behaviour
**Evidence:** AddContract and AddConsumer argument checks. The exact size/ASCII limits are source-observed constraints, not inferred internationalization policy.

Pre: extension registration receives an identity. Post: accepted contract names are nonblank <=500 chars; consumer IDs are nonblank <=200 ASCII chars. Duplicate resolution is CORE-002.

#### CONFIG-003-AC-01 — Contract boundary
**Given** nonblank contract names of length 500 and 501.
**When** each is registered separately.
**Then** 500 is accepted by the extension and 501 is rejected before a successful registration.

#### CONFIG-003-AC-02 — Consumer boundary
**Given** nonblank ASCII IDs of length 200/201 and an ID containing a character above ASCII 127.
**When** each is registered separately.
**Then** 200 ASCII is accepted; 201 or non-ASCII is rejected.

#### CONFIG-003-AC-03 — Blank identities
**Given** a null, empty or whitespace-only name or consumer ID.
**When** registration is requested.
**Then** argument validation fails rather than registering an empty identity.

### CONFIG-004 — Startup validation and configuration binding
**Classification:** Observed behaviour
**Evidence:** Core and shared persistence options validators and ConsumerValidation startup checks.

Pre: complete feature registrations and configured options. Post: specified invalid values/consumer registrations fail startup; binding is section-specific rather than universal.

#### CONFIG-004-AC-01 — Missing consumer contract
**Given** a consumer is registered but its message contract is absent.
**When** consumer startup validation runs.
**Then** startup fails before successful normal consumer processing.

#### CONFIG-004-AC-02 — Missing consumer service
**Given** a provider-created invoker references a service absent from the container.
**When** consumer startup validation runs.
**Then** startup fails with the missing service registration condition.

#### CONFIG-004-AC-03 — Core section binding
**Given** IConfiguration contains Messaging:Retry and unrelated capability sections.
**When** core AddMessaging(configuration) is used.
**Then** retry values are bound from Messaging:Retry and unrelated feature options are not implicitly bound.

#### CONFIG-004-AC-04 — Common invalid options
**Given** nonpositive lease/processing/recovery/cleanup values, renewal >= duration or failure maximum < recovery interval.
**When** common MessageOptions validation runs.
**Then** validation reports the offending constraints instead of declaring options valid.

#### CONFIG-004-AC-05 — Aggregated option failures
**Given** several independently invalid nonnull common option groups.
**When** validation runs.
**Then** all applicable validation failures are returned, not only the first.

#### CONFIG-004-AC-06 — Valid defaults
**Given** new common MessageOptions and RetryOptions.
**When** their standard validation runs.
**Then** the documented defaults pass; common defaults are lease 30s/renewal 10s, concurrency 4/capacity 1024/batch 100/settlement 5s, recovery 10s/max 1m, cleanup 1h/retention 7d/batch 500; retry defaults are 5 attempts/2s initial/1m cap/exponential/full jitter.

### CONFIG-005 — One messaging persistence context type
**Classification:** Observed behaviour
**Evidence:** PersistenceServices.EnsurePersistence rejects a different type in the same collection; same type is accepted.

Pre: standard persistence registrations in one collection. Post: same type can be shared by capabilities; a different type conflicts. This check does not verify same instance/transaction for arbitrary contexts.

#### CONFIG-005-AC-01 — Consistent context registration
**Given** a collection already selects context A.
**When** another capability selects context A.
**Then** the persistence type check accepts it.

#### CONFIG-005-AC-02 — Conflicting context registration
**Given** a collection already selects context A.
**When** a capability selects different context B.
**Then** registration fails clearly rather than silently mixing standard messaging contexts.

### CONFIG-006 — Complete boundary validation
**Classification:** Proposed behaviour
**Evidence:** Coding standards §6/§17 require clear boundary validation; null nested groups and invalid enum values currently bypass the intended checks.

Unapproved proposal: null common option groups and undefined RetryBackoff values produce configuration-validation failures at startup, not later NullReferenceException/processing failure.

**Open decisions:** [GAP-CORE-003](GAPS.md#gap-core-003).

#### CONFIG-006-AC-01 — Missing nested options
**Given** a common options object has null Lease, Processing, Recovery or Cleanup.
**When** startup validation runs after this proposal is approved.
**Then** startup returns a clear options validation failure identifying the missing group.

#### CONFIG-006-AC-02 — Undefined retry mode
**Given** RetryBackoff contains an undefined enum value.
**When** startup validation runs after this proposal is approved.
**Then** startup rejects the mode before any message attempt.

## Traceability and verification matrix
All rows are **Not tested**. Unit means isolated observable contract verification, not internal-call assertions. Persistence/concurrency verification requires a suitable real relational provider, not EF InMemory. The eventual TUnit suite can combine criteria while retaining their IDs.

| Specification | Criterion | Classification | Verification | Observable assertion | Status |
| --- | --- | --- | --- | --- | --- |
| CONFIG-001 | CONFIG-001-AC-01 | Required behaviour | Unit | Host-owned persistence setup | Not tested |
| CONFIG-001 | CONFIG-001-AC-02 | Required behaviour | Integration | Valid scoped host | Not tested |
| CONFIG-001 | CONFIG-001-AC-03 | Required behaviour | Integration | Separate operation scopes | Not tested |
| CONFIG-002 | CONFIG-002-AC-01 | Observed behaviour | Unit | Service override | Not tested |
| CONFIG-002 | CONFIG-002-AC-02 | Observed behaviour | Unit | Repeated core registration | Not tested |
| CONFIG-002 | CONFIG-002-AC-03 | Observed behaviour | Unit | Persistence builder forwarding | Not tested |
| CONFIG-002 | CONFIG-002-AC-04 | Observed behaviour | Unit | Null builder callbacks | Not tested |
| CONFIG-003 | CONFIG-003-AC-01 | Observed behaviour | Unit | Contract boundary | Not tested |
| CONFIG-003 | CONFIG-003-AC-02 | Observed behaviour | Unit | Consumer boundary | Not tested |
| CONFIG-003 | CONFIG-003-AC-03 | Observed behaviour | Unit | Blank identities | Not tested |
| CONFIG-004 | CONFIG-004-AC-01 | Observed behaviour | Unit | Missing consumer contract | Not tested |
| CONFIG-004 | CONFIG-004-AC-02 | Observed behaviour | Unit | Missing consumer service | Not tested |
| CONFIG-004 | CONFIG-004-AC-03 | Observed behaviour | Unit | Core section binding | Not tested |
| CONFIG-004 | CONFIG-004-AC-04 | Observed behaviour | Unit | Common invalid options | Not tested |
| CONFIG-004 | CONFIG-004-AC-05 | Observed behaviour | Unit | Aggregated option failures | Not tested |
| CONFIG-004 | CONFIG-004-AC-06 | Observed behaviour | Unit | Valid defaults | Not tested |
| CONFIG-005 | CONFIG-005-AC-01 | Observed behaviour | Unit | Consistent context registration | Not tested |
| CONFIG-005 | CONFIG-005-AC-02 | Observed behaviour | Unit | Conflicting context registration | Not tested |
| CONFIG-006 | CONFIG-006-AC-01 | Proposed behaviour | Unit | Missing nested options | Not tested |
| CONFIG-006 | CONFIG-006-AC-02 | Proposed behaviour | Unit | Undefined retry mode | Not tested |

## Implementation trace
These locations are evidence, not mandated internal architecture. The full inspected-file/public-surface matrix is in [INSPECTION.md](INSPECTION.md).

- [src/Clean.Messaging/DependencyInjection.cs](../../src/Clean.Messaging/DependencyInjection.cs)
- [src/Clean.Messaging/Consumers/ConsumerValidation.cs](../../src/Clean.Messaging/Consumers/ConsumerValidation.cs)
- [src/Clean.Messaging/MessageOptions.cs](../../src/Clean.Messaging/MessageOptions.cs)
- [src/Clean.Messaging/Processing/MessageOptionsValidator.cs](../../src/Clean.Messaging/Processing/MessageOptionsValidator.cs)
- [src/Clean.Messaging.EntityFrameworkCore/DependencyInjection.cs](../../src/Clean.Messaging.EntityFrameworkCore/DependencyInjection.cs)
- [src/Clean.Messaging.EntityFrameworkCore/MessagingBuilder.cs](../../src/Clean.Messaging.EntityFrameworkCore/MessagingBuilder.cs)
- [src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs](../../src/Clean.Messaging.EntityFrameworkCore/PersistenceServices.cs)

## Open decisions and evidence limits
The [gap register](GAPS.md) records unresolved input domains, provider evidence and proposed changes. Source inspection is not execution proof. [INDEX.md](INDEX.md) explains classification authority and cross-capability ownership.
