# OUTBOX-003 — Target validation, local selection and empty batches

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: IOutboxTargetProvider.GetTargets, OutboxTarget, OutboxTargetProvider, LocalTransport.Name, factory validation and OutboxWriter.Stage. [GAP-DUR-006](../GAPS.md#gap-dur-006) covers external-only default selection.

Contracts: GetTargets(Type messageType, object message); target ID/transport/destination; local transport name "local". Target IDs: nonblank <=200 ASCII, unique ordinally across the whole batch even across transports; transport nonblank. Destination and transport lengths/grammar are not comprehensively checked in the factory. Default local provider uses exact-type consumer Get and throws if none even when external providers could supply work. A custom composition may return no targets, producing IsEmpty true and no-op Stage.

## OUTBOX-003-AC-01 — Validate target boundary and duplicates

- **Given** targets contain blank, 201-character or non-ASCII IDs, blank transport, or duplicate ordinal TargetIds across routes.
- **When** Create evaluates the complete target set.
- **Then** invalid IDs/transports fail; duplicate target identity fails independent of destination or transport; distinct case-sensitive IDs remain distinct.

## OUTBOX-003-AC-02 — Do not infer transport or destination schema validation

- **Given** a nonblank unusually long transport or null/blank/custom destination and otherwise valid target.
- **When** factory creates a batch.
- **Then** factory does not itself reject by destination semantics or transport maximum length; provider mapping/persistence/delivery may reject later.

## OUTBOX-003-AC-03 — Observe external-only default local failure

- **Given** a payload has no exact-type local consumers but an external provider could supply targets and the default local provider remains installed.
- **When** Create/Enqueue enumerates providers.
- **Then** consumer.message_unhandled fails staging; external provider availability alone does not make default composition external-only.

## OUTBOX-003-AC-04 — Stage an empty custom batch without work

- **Given** a custom provider composition returns no targets and serialization succeeds.
- **When** factory and writer Stage run.
- **Then** IsEmpty is true and no entries are added; empty batch staging is not external delivery.
