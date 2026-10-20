# Automated testing foundation

## Scope and dependencies

`tests\Clean.Messaging.Tests` is a non-packable .NET 10 executable TUnit project, included under the solution's `tests` folder. It references core messaging and provider-neutral outbox. Tests exercise public contracts through real dependency injection, serialization, dispatch, context scopes and outbox staging.

| Dependency | Version | Use |
| --- | --- | --- |
| TUnit | 1.75.0 | Source-generated discovery, execution, native awaited assertions and TRX reporting |
| NSubstitute | 6.2.0 | Controlled public service overrides and outbox staging port |
| Microsoft.Extensions.DependencyInjection | 10.0.11 | Real containers and scopes, matching the library's Microsoft.Extensions version |

The fast core project has no database, broker or HTTP dependencies. PostgreSQL integration uses the separate [EF Core test project](#postgresql-persistence-integration); neither project uses Microsoft.NET.Test.Sdk, FluentAssertions or an additional fixture framework. WireMock.Net is not installed.

## Capability organisation

Folders and namespaces mirror the capabilities under test:

| Folder / namespace suffix | Test classes |
| --- | --- |
| `Core.Registration` (`Core\Registration`) | ContractRegistrationTests, ConsumerRegistrationTests |
| `Core.Consumers` (`Core\Consumers`) | ConsumerDispatchTests |
| `Core.Serialization` (`Core\Serialization`) | MessageSerializationTests |
| `Configuration` | MessagingRegistrationTests, ConsumerValidationTests |
| `Identity` | MessageContextTests |
| `Outbox` | OutboxIdentityTests |

Namespaces start with `Clean.Messaging.Tests`. Small class-local registration/staging methods remove repeated setup without hiding scenario inputs. Awaiting, failing, recording and context consumers have focused roles. Serialization's retained `message.v1.json` fixture is copied to output independently of the current serializer; malformed JSON remains explicit boundary-test input. Outbox creation-time evidence uses an advancing UTC clock so per-target clock reads cannot masquerade as one shared creation time.

## Supported commands

Run from the repository root with a .NET 10 SDK. The repository `global.json` selects Microsoft.Testing.Platform for `dotnet test` without pinning an SDK patch version.

```powershell
dotnet build tests\Clean.Messaging.Tests\Clean.Messaging.Tests.csproj -c Release
dotnet test --project tests\Clean.Messaging.Tests\Clean.Messaging.Tests.csproj -c Release --report-trx
dotnet test --project tests\Clean.Messaging.Tests\Clean.Messaging.Tests.csproj -c Release --no-build --list-tests
dotnet run --project tests\Clean.Messaging.Tests\Clean.Messaging.Tests.csproj -c Release --no-build -- --report-trx
```

Native .NET 10 `dotnet test` uses `--project` and passes runner options directly, without the legacy extra `--`. `dotnet run` retains its `--` separator. TRX/HTML/JSON reports are generated in ignored `TestResults` directories; the runner prints their locations. The commands above select the test project explicitly.

## Criterion selection

TUnit's installed runner supports filtering by the method-level custom property:

```powershell
dotnet test --project tests\Clean.Messaging.Tests\Clean.Messaging.Tests.csproj -c Release --no-build --treenode-filter '/*/*/*/*[TestCaseId=CONFIG-003-AC-03]'
```

Metadata selects tests; it does not imply that a criterion is approved or fully verified. The installed TRX reporter exports `TestCaseId` as a key/value pair in mapped test definitions. Other reporters or CI consumers may represent metadata differently.

## Isolation and evidence

TUnit default parallel execution remains enabled. Each test owns its container, scopes, options, context and mutable consumer state. Asynchronous tests use completion-source barriers, not delays; ten-second waits are deadlock guards, not timing expectations. Dispatch tests release blocked consumers in one `finally` and observe completion before checking captured state. Cancellation is requested only after the consumer starts and exercises its token-aware wait; dispatch cancellation is observed once. Overlapping context flows record complete metadata before and after a shared release, finish both flows, then assert their observations. Cleanup does not use assertion-success flags, nested catches or exception logging.

The failed-handler context test owns a scope around real dispatch. It observes restored metadata inside the failing flow, after scope disposal: checking only the calling flow would also pass when an async delegate implicitly restores AsyncLocal state. This is evidence for context-scope disposal around dispatch, not every processing host's scope composition.

Test code treats warnings as errors. All test methods follow `MethodName_StateUnderTest_ExpectedBehavior`. Specification-derived methods carry one primary `[Property("TestCaseId", "...")]`. Supporting scenarios without an exact documented criterion are not assigned an unrelated ID. Private nested doubles keep public constructors only where DI activation requires them. Explicit null assertions remain where the installed TUnit exception assertion returns a nullable value needed for subsequent member access.

[Specification IDs](../specifications/INDEX.md) and test metadata provide durable traceability; current results belong in runner or CI artifacts, not a manually maintained specification-status register. Reserved identifiers retain unresolved obligations; neither test metadata nor implementation behavior approves them. Outbox storage substitutes prove staging arguments, not database commit, deduplication, retry identity, fencing or external delivery. The identity/payload tests tagged `OUTBOX-001-AC-02` exercise its staging clauses only; its owning-save commit clause needs persistence integration. Normal AsyncLocal scopes do not establish guarantees for out-of-order or cross-flow disposal.

## PostgreSQL persistence integration

`tests\Clean.Messaging.EntityFrameworkCore.Tests` is a separate non-packable .NET 10 TUnit executable, registered under the solution's `tests` folder. Provider and container dependencies stay outside the fast core project and production packages.

| Dependency | Version | Use |
| --- | --- | --- |
| TUnit | 1.75.0 | Discovery, shared-resource lifecycle, parameterized cases and native assertions |
| Testcontainers.PostgreSql | 4.16.0 | A TUnit-owned PostgreSQL container |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | Real relational queries, constraints and transactions |
| Microsoft.Extensions.DependencyInjection | 10.0.11 | Actual library composition and independent scopes |

The prerequisites are a .NET 10 SDK and a Docker daemon accessible to Testcontainers, running Linux containers. The fixture uses `postgres:17.7-alpine`; first execution may download the image. One PostgreSQL server is shared for the test session. Every test invocation creates its own database, contexts and service provider; database cleanup runs on disposal, and TUnit owns container teardown. Connection pooling is disabled for these isolated databases. No existing application database or credentials are required.

The concrete model uses production `AddInbox()` mappings and `InboxEntry`, not a copied durable entity or an EF InMemory substitute. Actual `AddInbox<MessagingDbContext>()` registrations supply shared durable/dead-letter stores and the local Inbox dispatcher. Hosted workers are not started. The small `LocalEffect` model supplies participating state; the external-effect scenario commits through a separate test database. Queries used for assertions open independent, untracked contexts.

| Folder / namespace suffix | Boundary |
| --- | --- |
| `Persistence` | Retained identity constraints, invalid budgets, cancellation and actual local dispatch commit/rollback |
| `ClaimsLeases` | Bounded acquisition, terminal exclusion, stale-owner fencing, renewal, settlement and database-lock races |
| `Observability` | Dead-letter projection/management and terminal cleanup, including retained Inbox admission/replay |
| `Infrastructure` | Container/database lifetime, concrete context, serializer-based scenario entries and durable-state snapshots |

Namespaces start with `Clean.Messaging.EntityFrameworkCore.Tests`. Scenarios use real serialization and store operations. Database concurrency is coordinated by held PostgreSQL row locks and observation of blocked updates in `pg_stat_activity`; bounded cancellation guards prevent hangs without using delays to establish ordering. The in-flight cancellation case cancels an actual lock-waiting command before the blocker is released, rather than racing a commit and inferring rollback from cancellation.

```powershell
dotnet build tests\Clean.Messaging.EntityFrameworkCore.Tests\Clean.Messaging.EntityFrameworkCore.Tests.csproj -c Release
dotnet test --project tests\Clean.Messaging.EntityFrameworkCore.Tests\Clean.Messaging.EntityFrameworkCore.Tests.csproj -c Release --no-build --report-trx
dotnet test --project tests\Clean.Messaging.EntityFrameworkCore.Tests\Clean.Messaging.EntityFrameworkCore.Tests.csproj -c Release --no-build --list-tests
dotnet test --project tests\Clean.Messaging.EntityFrameworkCore.Tests\Clean.Messaging.EntityFrameworkCore.Tests.csproj -c Release --no-build --treenode-filter '/*/*/ConcurrentOwnershipTests/*'
```

The project exercises established shared `PERSIST`, `CLAIM` and management/cleanup `OPS` contracts. The bounded-acquisition guarantee and in-flight cancellation scenario have no exact established acceptance-criterion IDs, so their supporting tests do not fabricate metadata. The composite `CLAIM-001-AC-03` remains reserved for its unresolved accounting clause. Exact deadline equality, abandonment budgets, invalid renewal policy, ambiguous-commit attribution and discarded-row retention-clock policy are not selected by these tests.

PostgreSQL execution is evidence for this provider and harness, not certification of all EF relational providers. Inbox local-dispatch tests establish the configured-context boundary used by the shared completion store; they do not certify every Outbox dispatcher, saga store, scheduler, broker, migration or execution-strategy failure scenario. Retry tests persist supplied failure/due state; classification, retry-budget evaluation and backoff belong to the provider-neutral runtime. Uncertain commit and concurrent acquisition-policy decisions remain linked from the [specification index](../specifications/INDEX.md) and [decision register](../specifications/GAPS.md).

## Verification boundaries

Choose the boundary that can establish the criterion's observable outcome, rather than assigning a default boundary to every specification or testing internal call sequences.

| Boundary | Evidence it can establish |
| --- | --- |
| Unit / in-process | Public registration, serialization, mapping, state transitions and policy outcomes using isolated dependencies. |
| Persistence integration | Real relational constraints, commit/rollback outcomes, retained durable truth and the difference between tracked and stored state. Substitutes cannot establish these guarantees. |
| Concurrency | Competing ownership, cancellation and commit races coordinated with explicit barriers. Timing-only overlap or matching predicates does not prove exclusivity. |
| Transport integration | Real protocol packets, acknowledgements and connection/session behavior. A successful local adapter call does not prove a remote application effect. |
| Durable workflow | Connected Inbox, Outbox, scheduling or saga outcomes across the relevant local/external boundaries, including restart recovery and independent target completion. |
| Host integration | Actual startup, scoped service lifetimes and installed transaction/interceptor participation. |

A criterion may need evidence at more than one boundary. Provider support, concurrency and uncertain commit claims require the actual provider and relevant failure scenarios; in-process success is not provider certification. Use the exact criterion ID in applicable test metadata, keeping recommendations and reserved criteria distinct from approved requirements. Maintain results in runner/CI artifacts, not a second manual test-to-specification table.

## Tooling references

- [TUnit installation](https://tunit.dev/docs/getting-started/installation.md)
- [Native assertions](https://tunit.dev/docs/assertions/getting-started.md) and [exception assertions](https://tunit.dev/docs/assertions/exceptions.md)
- [Parameterized tests](https://tunit.dev/docs/writing-tests/arguments.md) and [execution](https://tunit.dev/docs/getting-started/running-your-tests.md)
- [Parallel execution](https://tunit.dev/docs/execution/parallelism.md)
- [Shared class data sources](https://tunit.dev/docs/writing-tests/class-data-source.md) and [matrix parameterization](https://tunit.dev/docs/writing-tests/matrix-tests.md)
- [Testcontainers PostgreSQL module](https://dotnet.testcontainers.org/modules/postgres/)
- [Custom properties](https://tunit.dev/docs/writing-tests/test-context.md#custom-properties) and [property-based filters](https://tunit.dev/docs/execution/test-filters.md)
- [.NET 10 Microsoft.Testing.Platform runner selection](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test#mtp-mode-of-dotnet-test)
