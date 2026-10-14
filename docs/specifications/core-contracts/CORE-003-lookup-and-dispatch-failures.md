# CORE-003 — Lookup and dispatch failures

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Dispatch callers need contract mismatches reported before invoking an incompatible consumer, and handler completion/failure to remain visible to the processing policy.

## Guarantees

- Missing contract mappings MUST report permanent contract.type.not_registered or contract.identifier.not_registered failures, according to the lookup.
- Missing consumers and incompatible nonnull messages MUST report permanent consumer.not_registered or consumer.message_type_mismatch failures without invoking a consumer for that payload.
- Dispatch MUST await the selected handler's asynchronous result and propagate its failure instead of fabricating success.
- Dispatch MUST forward the supplied cancellation token to the handler without adding an independent timeout.

## Contract boundaries

The supplied service scope owns consumer lifetime. Contract/consumer configuration errors are distinct from arbitrary service-resolution or handler errors, which are not universally wrapped. Null invocations and default/malformed MessageData are unresolved in [GAP-CORE-008](../GAPS.md#gap-core-008). Dispatch completion alone is not durable settlement.

## Acceptance criteria

### CORE-003-AC-01 — Missing contract

- **Given** an unregistered CLR type or nonblank persisted contract name.
- **When** its mapping is requested.
- **Then** a Fault MessageException reports `contract.type.not_registered` or `contract.identifier.not_registered`, respectively.

### CORE-003-AC-02 — Missing consumer

- **Given** a nonregistered consumer ID.
- **When** dispatch is requested.
- **Then** a Fault MessageException reports `consumer.not_registered` and no consumer handles the payload.

### CORE-003-AC-03 — Incompatible consumer message

- **Given** a selected consumer expects one type but receives an incompatible nonnull object.
- **When** the invoker invokes it.
- **Then** a Fault MessageException reports `consumer.message_type_mismatch` and the consumer is not called.

### CORE-003-AC-04 — Handler completion and failure

- **Given** a registered typed consumer that completes asynchronously or throws a known exception.
- **When** its message is dispatched.
- **Then** dispatch awaits completion or exposes the handler exception instead of returning a successful default.

### CORE-003-AC-05 — Invocation token

- **Given** a consumer observing its supplied CancellationToken.
- **When** the dispatcher invokes it with a caller token.
- **Then** the consumer observes that token; dispatch adds no independent timeout.

## Open decisions

[GAP-CORE-008](../GAPS.md#gap-core-008) leaves null invocations and malformed public serialization records unresolved.
