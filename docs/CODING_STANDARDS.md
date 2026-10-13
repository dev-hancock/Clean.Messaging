# Coding standards

## Purpose

These standards define how code should be designed, written, reviewed, tested, and maintained in Clean.Messaging. They apply to human contributors and AI coding agents.

Clean.Messaging is a modular .NET library for durable messaging, domain-event capture, transport integration, scheduling, and sagas. It is not a business application and does not own its consumers' domain rules, UI, device control, or deployment configuration.

Software should be correct, readable, maintainable, cohesive, decoupled, testable, and minimal. These principles draw on Clean Code, SOLID, Clean Architecture, domain-driven design, and pragmatic software engineering. Clean code is the simplest design that correctly expresses intended behaviour while preserving appropriate boundaries, not the design with the most patterns or abstractions.

Existing code is not automatically authoritative. Follow sound established conventions, but investigate conflicts with documented boundaries instead of perpetuating them. This document defines contribution standards; [the architecture reference](reference/architecture.md) describes the current implementation and its limitations. The [documentation index](README.md) links to capability contracts and conceptual explanations.

## 1. Before changing code

1. Read [the repository instructions](../.github/copilot-instructions.md), this document, and the relevant reference documentation.
2. Understand the requirement and intended behaviour.
3. Inspect related code, project references, registrations, callers, consumers, and available tests. Trace end-to-end paths for persistence, messaging, scheduling, or saga changes.
4. Identify affected package, public API, persisted-data, and transport compatibility boundaries.
5. Look for a suitable existing mechanism before introducing another.
6. Consider failure, concurrency, cancellation, time, and compatibility where applicable.
7. Make the smallest coherent change and validate its behaviour.

Prefer focused changes over unrelated cleanup, speculative rewrites, or broad reformatting. Do not change public contracts without considering their consumers. If code, tests, and documentation disagree, investigate rather than assuming one is right.

## 2. Fundamental design principles

### Simplicity

- Prefer direct control flow and the fewest concepts needed to express correct behaviour.
- Avoid indirection, configurable frameworks, and generic machinery without a demonstrated need.
- Simplicity does not mean combining unrelated responsibilities or ignoring boundaries.

### Single responsibility and cohesion

- A class or module should have one cohesive reason to change, not necessarily only one method.
- Keep message admission, dispatch, persistence, transport mapping, retries, and workflow state in their owning components.
- Split components when doing so improves responsibility or protects a boundary, not to satisfy line-count limits.

### Separation of concerns and encapsulation

- Put behaviour where its responsibility belongs; expose capabilities rather than internal implementation details.
- Default to private; use internal for assembly-level collaboration and public for intentional consumer or provider contracts.
- Protect lifecycle rules through controlled transitions, not unrestricted mutation.
- Every piece of mutable state must have a clear owner and lifecycle. Avoid shared mutable configuration, deferred mutation, and partially initialised objects. Prefer constructing complete, valid objects and passing immutable descriptions of work between components.
- Prefer immutable messages, snapshots, envelopes, and values where mutation is unnecessary. Follow .NET options conventions for configuration models.

### Explicitness

- Make dependencies, mutation, side effects, ownership, and failure semantics apparent.
- Avoid implicit global state, service locators, unexplained constants, boolean mode flags, and surprising side effects.
- Use clear, role-based names and early returns where they clarify execution.

### DRY and premature abstraction

DRY applies to duplicated knowledge, not superficially similar syntax. Small duplication is often better than coupling responsibilities that may evolve independently. Extract shared behaviour only when it represents the same semantics, changes for the same reasons, and makes consumers easier to understand.

An abstraction must express a meaningful contract, policy boundary, or variation point. It should not merely reproduce a concrete implementation's public methods. Evaluate abstractions according to the coupling they remove, not the number of interfaces they introduce.

### SOLID, pragmatically

- Single Responsibility: keep components cohesive.
- Open/Closed: add extension points for demonstrated variation, not hypothetical flexibility.
- Liskov Substitution: implementations must honour the behaviour of their contracts.
- Interface Segregation: keep capability interfaces focused.
- Dependency Inversion: provider-neutral policy must not depend on provider implementation details.

Prefer composition to inheritance except for genuine substitutable relationships. SOLID does not justify gratuitous factories, base classes, or interfaces.

## 3. Architecture and ownership

The repository uses separate projects for capabilities and provider integrations. Do not impose a Domain/Application/Infrastructure/Web folder structure on this library. Use [the architecture reference](reference/architecture.md) for the actual dependency graph.

- **Public abstractions:** keep `Clean.Messaging.Abstractions` independent of runtime and provider libraries. Consumer contracts and message context must not expose EF Core or MQTTnet types.
- **Provider-neutral runtime:** `Clean.Messaging` owns contract registration, serialization, consumer dispatch, processing primitives, retry policy, and runtime context. Its Microsoft.Extensions dependencies are intentional; provider-neutral does not mean dependency-free.
- **Capabilities:** Inbox, Outbox, Scheduling, and Sagas own their respective semantics and extension ports. Keep feature-local behaviour local; share infrastructure only when lifecycle and consistency semantics genuinely match.
- **Domain events:** `Clean.Messaging.DomainEvents` provides event-source support, not a business domain model. Consumer applications own aggregates, invariants, event meaning, and translations into external contracts.
- **Persistence:** EF Core projects implement stores, mappings, transaction integration, and concurrency controls. Keep ORM types out of provider-neutral consumer APIs. Provider extension contracts may expose framework types when that is their explicit purpose.
- **Transport:** `Clean.Messaging.Transport` owns transport representations and serialization/mapping contracts. Ingress and Dispatch bridge transport payloads and durable messages; MQTT integrations own broker-specific routing, metadata, connection, acknowledgement, and publishing behaviour.
- **Composition:** the consuming application's host is the composition root. Keep registration extensions grouped by capability; do not introduce a library `Program.cs` or a mandatory application framework.

Do not require Mediator, ErrorOr, Ardalis Specification, a UI library, or a generic repository/unit-of-work pattern merely because a consuming application uses one. They are not current repository dependencies. Do not create empty feature folders for symmetry. Reserve shared namespaces for genuinely shared concepts, not unrelated helpers.

## 4. Naming

Names should express meaning and responsibility, not implementation technique.

- Use PascalCase for types/members, camelCase for parameters/locals, and established `_camelCase` private fields.
- Prefer concise role names such as `InboxWorker`, `OutboxProcessor`, `MqttRoute`, `SagaEffectWriter`, `Claim`, `Dispatch`, and `Complete`.
- Avoid `DataManager`, `CommonService`, `MessageHelper`, and inflated names unless they describe a distinct responsibility.
- Generic `Process`, `Handle`, `Run`, and `Execute` are appropriate where they genuinely describe the contract.
- Name conditions `IsTerminal`, `HasExpired`, and `CanRetry`, not `Flag` or `Valid`.
- Interfaces represent capabilities, boundaries, or substitution points, not mandatory one-to-one partners for classes.
- Use namespaces instead of repeating an entire hierarchy in a type name.

## 5. Classes, methods, and APIs

- Keep classes cohesive, public APIs minimal, mutable state contained, and constructors responsible for valid setup.
- Prefer sealed when inheritance is not part of the intended design.
- Extract methods to expose meaningful operations or reduce cognitive complexity, not to meet arbitrary size limits.
- Keep methods at a consistent abstraction level with straightforward control flow and explicit failure paths.
- Avoid parameter bags that conceal unrelated arguments and booleans that toggle unrelated modes.
- Queries should not intentionally mutate business state; commands should make changes explicit. Incidental caching or telemetry must preserve query semantics.
- Make invalid API use difficult, overloads consistent, and implementation details private.
- Follow existing `Task`/`ValueTask` contracts. Use read-only collections where callers must not mutate state; avoid redundant response wrappers.
- Distinguish application-facing APIs from provider extension contracts. Public visibility is not permission to expose runtime internals unnecessarily.

## 6. Dependency injection and composition

- Prefer constructor injection and explicit dependencies.
- Restrict `IServiceProvider` resolution to deliberate composition, scope, or dispatch infrastructure.
- Use correct lifetimes; never capture a scoped service in a singleton.
- Keep registration discoverable and grouped by capability. Prefer explicit typed registrations to complex runtime scanning.
- Keep provider implementation dependencies in integration projects.
- Use `InternalsVisibleTo` only for deliberate, tightly coupled implementation relationships, not as a general substitute for integration contracts.
- Preserve documented registration, identity, and persistence guarantees. Changes to established constraints require deliberate design, compatibility assessment, and corresponding documentation updates.

### Configuration API design

- Use consistent, discoverable registration names across capabilities and providers, such as `AddInbox`, `AddOutbox`, and `AddMqttInbox`.
- Use the fewest generic type parameters needed to express the consumer's configuration; infer types where practical.
- Introduce intermediate builders only when they represent a meaningful configuration boundary, not merely to forward calls.
- Keep implementation details out of application-facing registration signatures; expose provider types only at intentional provider integration boundaries.
- Validate arguments at registration and cross-registration or operational constraints at the appropriate startup boundary. Do not defer invalid configuration until message processing or mutate shared configuration during later calls.

## 7. Error handling and failure semantics

The runtime uses exceptions and explicit messaging failure classifications; it does not establish an application result library. Consumer applications may translate their own expected outcomes at their boundaries.

- Use existing messaging and capability-specific exceptions for their intended meanings. Preserve error codes and transient/permanent classifications.
- Distinguish invalid use, permanent message failure, transient failure, cancellation, idempotent replay, and ownership/concurrency loss.
- Catch only where recovery, retry, translation, useful context, or cleanup is possible. Preserve original diagnostic information.
- Never hide failures behind successful/default outcomes, empty catch blocks without an explicit lifecycle reason, or unbounded retries.
- Validate untrusted input at boundaries; avoid duplicating validations between dependable internal contracts.
- Propagate `CancellationToken` through async I/O and long-running work. Do not misclassify shutdown or lease-loss cancellation as an ordinary message fault.
- Settlement after processing failure may have its own bounded consistency boundary; preserve that distinction when changing cancellation behaviour.

## 8. Async, concurrency, transactions, and time

- Use async/await; avoid `.Result`, `.Wait()`, and `async void` except required event handlers.
- Identify lifetimes, shared state, overlapping operations, ordering, and multi-instance execution before assuming thread safety.
- Make ownership, synchronization, and claim fencing explicit. A DI lifetime is not a thread-safety guarantee.
- Keep transaction boundaries explicit. Do not assume a database transaction includes MQTT or another external side effect.
- Preserve atomicity between local consumer changes and completion, or between owning state and staged outgoing work, where the implementation provides it.
- Document consistency limitations, uncertain commits, and retry-sensitive side effects.
- Use `TimeProvider` for replaceable clocks when time affects behaviour. Store instants consistently in UTC; distinguish wall-clock timestamps from elapsed durations.
- Avoid timing-dependent tests and arbitrary sleeps.

## 9. Messaging, retries, and workflows

Assume external delivery can repeat. MQTT QoS 2 is not proof of exactly-once application processing or external effects.

### Contracts and identity

- Treat contract names, serialized shapes, consumer/target IDs, transport names, destinations, and reserved metadata as compatibility contracts.
- Preserve stable logical identity across retries. Distinguish `EventId` from delivery `MessageId` according to the implemented API; do not silently merge them or generate a replacement identity during a retry.
- Carry correlation and causation only where meaningful; preserve documented propagation rules.
- Keep transport payloads separate from durable consumer messages and map representations at the boundary.
- Trace both producers and consumers when changing routing, metadata, serialization, or message identity.

### Inbox and outbox

- Separate durable admission from later dispatch. Preserve per-consumer/per-target completion and duplicate protection.
- Avoid re-executing successful targets merely because another target failed.
- Persist outgoing work atomically with its owning state change when transactional outbox guarantees apply.
- Preserve the distinction between transactional local consumer dispatch and external publication followed by completion.
- Retention and cleanup affect duplicate protection and replay. Document those limits.
- Do not acknowledge broker admission failures as successful processing. Preserve the explicit poison-message acknowledgement policy unless deliberately changing it.

### Claims, retries, and sagas

- Make processing ownership and its lifecycle explicit. Prevent stale owners from settling work they no longer own; document the limits of protection for external effects.
- Classify transient/permanent failures and bound retries. Coordinate broker, EF execution-strategy, and durable retry layers to avoid accidental attempt multiplication.
- Use sagas for durable multi-step/message/time workflows, not ordinary local calls.
- Make saga identity, correlation, state versioning, timers, effects, completion, failure, and recovery explicit.
- Compensation is not database rollback; do not claim automatic compensation the implementation does not provide.
- Check retained delivery, session/reconnect behaviour, metadata, routing, and serialization compatibility when changing MQTT integration.

## 10. Data modelling, immutability, and nullability

- Use meaningful types when primitives hide identity or lifecycle semantics; do not wrap every primitive without a reason.
- Prefer immutable messages, envelopes, snapshots, and values; use controlled mutable state for persisted lifecycles.
- Express optional values honestly with nullable reference types. Do not suppress warnings to conceal invalid state.
- Validate untrusted input without redundant null checks against trustworthy contracts.
- Use enums for meaningful closed sets and named constants for literals needing explanation.

## 11. C# and .NET style

All current source projects target .NET 10 with nullable reference types and implicit usings enabled. Follow any repository configuration and adjacent formatting; do not assume an `.editorconfig` or language-version override exists.

- Use modern language features when they improve clarity: primary constructors, records, pattern matching, collection expressions, file-scoped namespaces, and required/init members.
- Prefer readable loops to opaque LINQ chains with hidden branching or side effects.
- Avoid deeply nested ternaries and clever one-liners.
- Use primary constructors for straightforward dependencies and explicit constructors for validation or clearer setup.
- Do not perform network/database I/O in constructors.
- Use static members for stateless operations, not mutable global coordination or shortcuts around DI.
- Use extension methods for cohesive APIs and registration, not hidden unrelated behaviour.
- Avoid unrelated formatting changes, unnecessary regions, and verbose boilerplate.

## 12. Comments and documentation

Comments explain why a non-obvious decision exists: transaction guarantees, stale-owner protection, protocol compatibility, or why a simpler-looking alternative is unsafe. Do not narrate self-explanatory statements or compensate for poor names.

Use XML documentation for public contracts where semantics are not evident from signatures, especially identity reuse, preconditions, failures, ordering, concurrency, cancellation, and transaction ownership. Avoid automatically documenting trivial members.

Use [Diátaxis](https://diataxis.fr/) for documentation:

- **Reference** (`docs/reference/`): factual descriptions of packages, APIs, configuration, schemas, execution paths, guarantees, and limitations.
- **Explanation** (`docs/explanation/`): rationale, alternatives, trade-offs, and conceptual relationships.
- Tutorials and how-to guides are outside the current documentation scope; do not create those folders merely for symmetry.

Keep normative coding standards and concise AI instructions separate from descriptive product reference. Link to relevant code and related documents. Do not present intended architecture as implemented behaviour.

## 13. Logging and observability

- Follow structured `ILogger<T>` and source-generated `[LoggerMessage]` conventions where appropriate.
- Include meaningful message, event, target/consumer, correlation, saga, or schedule identifiers.
- Preserve trace context where supported; do not invent tracing guarantees.
- Avoid routine noise and duplicate exception logging at every layer.
- Never log secrets, credentials, tokens, or unnecessary sensitive payloads or saga state.
- Use Trace for fine detail, Debug for diagnostics, Information for meaningful normal operations, Warning for recoverable anomalies, Error for failed operations, and Critical for severe operational failures.
- Preserve operational metrics when changing processing lifecycle behaviour.

## 14. Package and integration compatibility

- Treat public APIs, package/project references, extension points, persisted contracts, and provider behaviour as compatibility boundaries.
- Do not introduce EF Core, MQTTnet, Quartz, or another provider dependency into a provider-neutral project without a deliberate architectural decision.
- Keep project references, package metadata, namespaces, and solution membership consistent when moving or renaming projects.
- Consider stored messages, existing rows, rolling deployments, and independently deployed consumers before changing serialized contracts or persistence mappings.
- Supply an explicit migration/compatibility approach when a change cannot preserve those contracts.
- Do not invent support for untested database providers, broker settings, target frameworks, or deployment topologies.

## 15. Testing and validation

Test behaviour and contracts, not incidental implementation mechanics. TUnit is the preferred framework for new .NET tests. The absence of an existing test project does not establish an alternative convention.

- Add focused tests for message identity, duplicate admission, per-target completion, retries, lease loss, serialization, registration, scheduling, and saga transitions when modifying those behaviours.
- Prefer deterministic tests with controlled time and state. Avoid arbitrary delays, uncontrolled brokers/networks, or shared state unless deliberately integration-testing them.
- Use simple fakes where appropriate; do not mock integration boundaries so heavily that the test proves nothing.
- Integration tests should exercise real transactions, persistence constraints, serialization, registration, broker admission/publication, and concurrency where those are the behaviour under test.
- Reproduce bugs in regression tests when practical.
- Use descriptive scenario/expected-result names, such as `Accept_WhenMessageAlreadyAdmitted_DoesNotDuplicateConsumerEntries`.
- Run the narrowest relevant build/tests, then broaden as required. Use existing tooling; introduce a test framework only as part of an intentional testing change.
- Report actual commands, results, and blockers accurately. Documentation-only changes require link/content checks, not an unrelated code build, unless documentation tooling is configured.

### Testing framework and naming
The standard .NET testing stack is **TUnit**, **NSubstitute**, and **Testcontainers**, with **WireMock.Net** for HTTP integration tests where appropriate. Use TUnit's native assertions. Do not introduce FluentAssertions or commercially licensed assertion libraries. Check which tools are actually configured before claiming they are available in a repository.
All .NET test methods **must** follow:
`MethodName_StateUnderTest_ExpectedBehavior`
- `MethodName` identifies the method, operation, or entry point being tested.
- `StateUnderTest` describes the relevant scenario, inputs, or preconditions.
- `ExpectedBehavior` describes the observable outcome.
- Use PascalCase for each segment, separated by underscores.
- Apply this convention consistently to unit, integration, and end-to-end tests.
- Use meaningful domain terminology; avoid generic names or descriptions of private implementation details.
- Keep acceptance-criteria identifiers in traceability metadata or documentation, **not** in test method names.
Examples:
```csharp
Accept_WhenMessageAlreadyAdmitted_DoesNotDuplicateConsumerEntries
Claim_WhenLeaseExpired_AllowsNewOwner
Dispatch_WhenConsumerFails_RetriesMessage
```
Follow the user-level `clean-testing` skill for testing strategy and the official `tunit` skill for framework-specific syntax and execution.

### Test-case traceability
Each .NET test verifying a documented acceptance criterion must include a method-level TUnit property with the criterion's **existing stable ID**:
```csharp
[Test]
[Property("TestCaseId", "INBOX-001-AC-02")]
public async Task Accept_WhenMessageAlreadyAdmitted_DoesNotDuplicateConsumerEntries()
{
    // Test implementation
}
```
- Use exactly `TestCaseId` for the property name across repositories.
- Use the ID from the canonical behavioural specification; do not invent or silently rename identifiers.
- Keep test method names in `MethodName_StateUnderTest_ExpectedBehavior` format, without embedding IDs.
- Different tests may reference the same acceptance criterion when they verify different scenarios or boundaries.
- Flag tests with no approved specification rather than attaching an unrelated ID. Do not claim coverage merely because an attribute exists.
- Validate the ID-to-test mapping during test review and report unlinked or unverified criteria.


## 16. Performance and resource management

Prioritise correctness and clarity; optimise demonstrated bottlenecks, not speculation. Avoid unnecessary allocations on hot paths, repeated queries, hidden expensive properties, and needless materialisation. Stream or batch where justified and maintainable.

Dispose owned `IDisposable`/`IAsyncDisposable` resources; do not dispose DI-owned resources. Bound scopes, connections, transactions, channels, handles, and processing concurrency.

## 17. Configuration and security

Validate configuration at boundaries and fail clearly on critical misconfiguration. Keep consumer business policy separate from operational options. Never hard-code or commit secrets or credentials. Treat external input as untrusted. Do not assume anonymous MQTT, disabled TLS, port 1883, in-memory state, or localhost are safe production defaults. Avoid speculative configuration flags.

## 18. AI coding agent instructions

AI-generated code must meet the same standards as human code.

- Inspect relevant instructions, adjacent code, contracts, registrations, available tests, and end-to-end paths before editing.
- Respect intentional designs; do not replace them with fashionable patterns without cause.
- Avoid automatic interface-per-class, generic bases, vague managers, reflection registries, unnecessary packages, service locators, and forwarding wrappers.
- Avoid unjustified defensive checks, broad catches, default fallbacks, and repeated validation.
- Do not silently change public APIs, persistence mappings, contract names, broker metadata, routing, or retry/idempotency semantics.
- Keep the diff focused; add relevant tests and update documentation when semantics change.
- Do not invent APIs, guarantees, or successful validations.
- Report changes, validation, and remaining risks accurately.

## 19. Code review checklist

### Design

- [ ] Behaviour belongs to the correct capability and responsibility.
- [ ] Dependencies preserve provider boundaries and consumer-owned business policy.
- [ ] No unnecessary abstraction, indirection, or framework was introduced.
- [ ] Public APIs are cohesive, minimal, and intentional.

### Implementation

- [ ] Names, control flow, ownership, and side effects are clear.
- [ ] Errors, cancellation, concurrency, time, and transactions are correctly handled.
- [ ] No unjustified duplication, defensive boilerplate, or complexity was introduced.

### Compatibility

- [ ] Producers, consumers, public APIs, persisted data, and transport contracts remain compatible or have a deliberate migration.
- [ ] Retry, identity, per-target completion, retention, lease, and acknowledgement assumptions were verified where relevant.

### Quality

- [ ] Tests cover important changed behaviour and are deterministic where practical.
- [ ] Validation was actually run or limitations were reported.
- [ ] Security and logging requirements are observed.
- [ ] Documentation reflects material semantic and architectural changes.

## 20. Anti-patterns

| Anti-pattern | Why to avoid it |
| --- | --- |
| God classes | Combine unrelated responsibilities and make changes unpredictable. |
| Consumer business policy in library infrastructure | Couples reusable mechanisms to an application's domain. |
| Service locator | Hides dependencies and ownership. |
| Interface per class | Adds indirection without a meaningful contract. |
| Generic repository proliferation | Duplicates persistence APIs without a boundary benefit. |
| Excessive inheritance | Couples implementations and confuses substitution. |
| Premature abstraction | Couples concepts before shared semantics are established. |
| Primitive obsession | Hides meaningful identity or lifecycle rules. |
| Boolean mode parameters | Conceal distinct behaviours behind one signature. |
| Hidden side effects | Make calls and consistency boundaries difficult to reason about. |
| Catch-all failure handling | Conceals failure distinctions and can corrupt retry semantics. |
| Excessive mocking | Tests implementation details instead of observable behaviour. |
| Speculative configurability | Expands complexity without a real requirement. |
| Broad utilities | Accumulate unrelated responsibilities. |
| Clever one-liners | Sacrifice readability for brevity. |
| Unbounded retries | Hide permanent failures and exhaust resources. |
| Transport/provider leakage | Couples consumer contracts to external frameworks. |
| Unsupported exactly-once claims | Conceal the gap between external delivery and local completion. |

## 21. Decision hierarchy

When multiple approaches are valid, evaluate them in this order:

1. Correctness: meets requirements and preserves invariants and delivery semantics.
2. Architectural integrity: respects capability ownership and provider boundaries.
3. Clarity: behaviour and design are readily understandable.
4. Cohesion: related responsibilities live together.
5. Minimal complexity: avoids unnecessary concepts and indirection.
6. Consistency: follows established, sound repository conventions.
7. Testability: important behaviour is verifiable.
8. Extensibility: supports demonstrated variation appropriately.
9. Performance: meets real resource and performance constraints.

These priorities do not excuse ignoring mandatory correctness, safety, compatibility, or performance requirements. Prefer explicit, cohesive, maintainable code over clever, abstract, or fashionable code. Every abstraction and dependency should have a clear reason to exist.
