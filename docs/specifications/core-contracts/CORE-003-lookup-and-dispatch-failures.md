# CORE-003 — Lookup and dispatch failures

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** Registry lookups and typed invoker branches; public MessageException code/action fields.

Pre: a consumer ID and retained MessageData. Post: lookup/deserialization succeeds before invoking the selected service. Failure codes classify known contract mismatches as permanent; service/handler errors are not uniformly wrapped.

## CORE-003-AC-01 — Missing contract
**Given** an unregistered CLR type or nonblank persisted contract name.
**When** its mapping is requested.
**Then** a Fault MessageException reports `contract.type.not_registered` or `contract.identifier.not_registered`, respectively.

## CORE-003-AC-02 — Missing consumer
**Given** a nonregistered consumer ID.
**When** dispatch is requested.
**Then** a Fault MessageException reports `consumer.not_registered` and no consumer handles the payload.

## CORE-003-AC-03 — Incompatible consumer message
**Given** a selected consumer expects one type but receives an incompatible nonnull object.
**When** the invoker invokes it.
**Then** a Fault MessageException reports `consumer.message_type_mismatch` and the consumer is not called.

## CORE-003-AC-04 — Handler completion and failure
**Given** a registered typed consumer that completes asynchronously or throws a known exception.
**When** its message is dispatched.
**Then** dispatch awaits completion or exposes the handler exception instead of returning a successful default.

## CORE-003-AC-05 — Invocation token
**Given** a consumer observing its supplied CancellationToken.
**When** the dispatcher invokes it with a caller token.
**Then** the consumer observes that token; dispatch adds no independent timeout.
