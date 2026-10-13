# SAGA-002 — Definitions, handler overloads and registration validation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** AddSagas(callback, optionsCallback?); SagaBuilder.Add<TState>(type,callback); On<TMessage,THandler>(consumerId,correlate,create), On(consumerId,correlate), On(). ISagaHandler.Handle receives state/message/token and returns ValueTask<SagaResult>.

**Evidence:** DependencyInjection.cs; SagaBuilder.cs; SagaRegistry.cs; SagaDefinition.cs; SagaMessageDefinition.cs; SagaRuntimeValidation.cs; shared core registries.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-002-AC-01 — Definitions require valid type and callbacks

**Given** null callback, no definitions, no messages, blank/overlong/non-ASCII type or duplicate state/type.
**When** saga definitions are registered.
**Then** invalid configuration rejects; nonblank ASCII type length 200 is accepted, 201 rejects, and duplicate identities/types do not silently override.

## SAGA-002-AC-02 — Message mapping is unique per definition

**Given** the same CLR message is registered twice by any combination of On overloads.
**When** the definition is built.
**Then** duplicate mapping rejects; another distinct message type can be registered.

## SAGA-002-AC-03 — Start and continuation overloads install consumers

**Given** valid consumerId and correlation with optional create callback.
**When** ordinary message handling is composed.
**Then** with create it can start; without create it can only continue; required delegates reject null and consumer IDs obey global nonblank ASCII <=200 uniqueness.

## SAGA-002-AC-04 — Timer-only mapping is instance-directed

**Given** On<TMessage,THandler>() without consumer identity/correlation.
**When** an ordinary message versus instance-directed timer is dispatched.
**Then** the mapping is available to that saga timer but supplies no ordinary durable consumer by itself.

## SAGA-002-AC-05 — All mappings require retained contracts

**Given** a definition includes a timer-only message without registered durable contract.
**When** saga startup validation runs.
**Then** the missing contract fails configuration validation rather than being silently exempt because no ordinary consumer exists.

## SAGA-002-AC-06 — Registered handler is scope-resolved

**Given** a host supplied valid handlers and a prior scoped handler registration.
**When** a message is handled.
**Then** the configured handler service is used in the owner scope; registering a saga does not make handler/context shared state thread-safe.
