# TRANSPORT-003 — Registration and exact mapping resolution

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** ingress [registration](../../../src/Clean.Messaging.Inbox.Ingress/DependencyInjection.cs), [registry](../../../src/Clean.Messaging.Inbox.Ingress/TransportIngressRegistry.cs), dispatch [registration](../../../src/Clean.Messaging.Outbox.Dispatch/DependencyInjection.cs), [registry](../../../src/Clean.Messaging.Outbox.Dispatch/TransportDispatchRegistry.cs), identity mappers in both projects.

`AddIngress()` installs shared default transport serializer, singleton inbox factory, scoped registry and `IInboxIngress`. `AddDispatch()` installs shared default serializer, scoped factory/registry and `IOutboxDispatch`. Generic single-type overloads register identity mapping; three-type overloads use scoped concrete mappers and typed map registrations. `TryAdd` preserves earlier compatible registrations. Identical typed implementation registrations are suppressed by `TryAddEnumerable`; distinct mapper implementations for the same pair conflict when the registry resolves. Neither API installs persistence, consumer contracts, hosted processing or broker connections.

## TRANSPORT-003-AC-01 — Custom serializer and mapper registration are retained
**Given** an earlier custom `ITransportSerializer` and concrete mapper registration, **When** ingress/dispatch registration is added and its mapping service is resolved, **Then** mapping uses the earlier services rather than replacing them with built-in JSON or a new mapper registration.

## TRANSPORT-003-AC-02 — Exact type pair chooses mapping
**Given** mappings A-to-B and A-to-C, but none for a subclass of A to B, **When** each registered exact pair is requested and then the unregistered pair is requested, **Then** registered requests use their respective mapper and the unregistered pair fails; variance in the mapper interface does not create registry fallback.

## TRANSPORT-003-AC-03 — Duplicate pair has no precedence winner
**Given** two distinct mapper implementations registered for one ingress pair or one dispatch pair, **When** the corresponding registry is resolved, **Then** resolution fails rather than choosing first/last; dispatch reports `OutboxDispatchException` and ingress dictionary construction reports a configuration exception. Repeating the same typed registration alone does not create this conflict.

## TRANSPORT-003-AC-04 — Invalid composition is not durable readiness
**Given** a null service collection, **When** any `AddIngress` or `AddDispatch` overload is invoked, **Then** argument validation fails. **Given** only valid mapping registrations without durable capability/runtime dependencies, **When** admission or dispatch services requiring those dependencies are resolved/used, **Then** registration alone supplies no successful durable admission, consumer processing or publication.
