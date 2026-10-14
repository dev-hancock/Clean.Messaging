# Automated testing foundation

## Scope and dependencies

`tests\Clean.Messaging.Tests` is a non-packable .NET 10 executable TUnit project, included under the solution's `tests` folder. It references core messaging and provider-neutral outbox. Tests exercise public contracts through real dependency injection, serialization, dispatch, context scopes and outbox staging.

| Dependency | Version | Use |
| --- | --- | --- |
| TUnit | 1.75.0 | Source-generated discovery, execution, native awaited assertions and TRX reporting |
| NSubstitute | 6.2.0 | Controlled public service overrides and outbox staging port |
| Microsoft.Extensions.DependencyInjection | 10.0.11 | Real containers and scopes, matching the library's Microsoft.Extensions version |

Testcontainers and WireMock.Net are not installed: this milestone has no database, broker or HTTP tests. No Microsoft.NET.Test.Sdk, FluentAssertions or additional fixture framework is used.

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

Namespaces start with `Clean.Messaging.Tests`. Small class-local registration/staging methods remove repeated setup without hiding scenario inputs. Awaiting, failing, recording and context consumers have focused roles. Serialization's retained `message.v1.json` fixture is copied to output independently of the current serializer; malformed JSON remains explicit boundary-test input. Outbox creation-time evidence uses a fixed clock.

## Supported commands

Run from the repository root with a .NET 10 SDK. The repository `global.json` selects Microsoft.Testing.Platform for `dotnet test` without pinning an SDK patch version.

```powershell
dotnet build tests\Clean.Messaging.Tests\Clean.Messaging.Tests.csproj -c Release
dotnet test --project tests\Clean.Messaging.Tests\Clean.Messaging.Tests.csproj -c Release --report-trx
dotnet test --project tests\Clean.Messaging.Tests\Clean.Messaging.Tests.csproj -c Release --no-build --list-tests
dotnet run --project tests\Clean.Messaging.Tests\Clean.Messaging.Tests.csproj -c Release --no-build -- --report-trx
```

Native .NET 10 `dotnet test` uses `--project` and passes runner options directly, without the legacy extra `--`. `dotnet run` retains its `--` separator. TRX/HTML/JSON reports are generated in ignored `TestResults` directories; the runner prints their locations. Use an explicit test project: the existing solution still contains [stale project paths](../maintenance.md#solution-membership).

## Criterion selection

TUnit's installed runner supports filtering by the method-level custom property:

```powershell
dotnet test --project tests\Clean.Messaging.Tests\Clean.Messaging.Tests.csproj -c Release --no-build --treenode-filter '/*/*/*/*[TestCaseId=CONFIG-003-AC-03]'
```

Metadata selects tests; it does not imply that a criterion is approved or fully verified. The installed TRX reporter exports `TestCaseId` as a key/value pair in mapped test definitions. Other reporters or CI consumers may represent metadata differently.

## Isolation and evidence

TUnit default parallel execution remains enabled. Each test owns its container, scopes, options, context and mutable consumer state. Asynchronous tests use completion-source barriers, not delays; ten-second waits are deadlock guards, not timing expectations. Cancellation is requested only after the awaited consumer starts; cancellation-aware waits and bounded cleanup observe its completion. Overlapping context flows release peers even if an assertion fails. Test code treats warnings as errors. All test methods follow `MethodName_StateUnderTest_ExpectedBehavior`. Specification-derived methods carry one primary `[Property("TestCaseId", "...")]`; comments can identify additional related criteria. Supporting scenarios without an exact documented criterion are not assigned an unrelated ID. Private nested doubles keep public constructors only where DI activation requires them. Dispatch cleanup reports secondary failures without masking an earlier assertion failure.

[Specification IDs](../specifications/INDEX.md) and test metadata provide durable traceability; current results belong in runner or CI artifacts, not a manually maintained specification-status register. Reserved identifiers retain unresolved obligations; neither test metadata nor implementation behavior approves them. Outbox storage substitutes prove staging arguments, not database commit, deduplication, retry identity, fencing or external delivery. Normal AsyncLocal scopes do not establish guarantees for out-of-order or cross-flow disposal.

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
- [Custom properties](https://tunit.dev/docs/writing-tests/test-context.md#custom-properties) and [property-based filters](https://tunit.dev/docs/execution/test-filters.md)
- [.NET 10 Microsoft.Testing.Platform runner selection](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test#mtp-mode-of-dotnet-test)
