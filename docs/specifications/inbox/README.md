# Inbox capability catalogue

[Specification index](../INDEX.md)

## Capability

Inbox provides typed durable admission and independent per-consumer processing. Adapters rely on admission, consumers rely on their local atomic result, and operators rely on retained identity, recovery and truthful management outcomes. These are separate boundaries: admission is not consumer completion or broker acknowledgement.

## Contract boundaries

Local completion guarantees apply only to participating changes in the configured scoped persistence transaction. Stable delivery/event/target identities and retained completion support recovery; removed evidence cannot provide an unlimited duplicate-protection horizon. Consumerless admission, replay validation/progress and admission retention remain explicit open decisions.

The specifications establish consumer-facing guarantees from independent standards and public compatibility. Reserved acceptance identifiers retain unresolved original obligations; an implementation observation or a documented recommendation is not policy approval. Required lifetime and failure-transparency obligations remain required even when the implementation differs.

## Specifications

- [INBOX-001 — Typed admission and envelope identity](INBOX-001-typed-admission-and-envelope-identity.md): Incoming adapters need a durable acceptance boundary for a typed envelope, with stable delivery and logical identities so transport redelivery is not mistaken for a new operation. Includes reserved policy identifiers.
- [INBOX-002 — Independent atomic admission and duplicate retention](INBOX-002-independent-atomic-admission-and-duplicate-retention.md): An adapter needs accepted work to survive its caller failing or losing the acknowledgement, while equivalent redelivery must not mutate the retained operation or multiply its target obligations. Includes reserved policy identifiers.
- [INBOX-003 — Empty admission boundary](INBOX-003-empty-admission-boundary.md): Consumers need an explicit answer about accepting history for later processing when no initial target exists. Includes reserved policy identifiers.
- [INBOX-004 — Durable entry state and fenced claims](INBOX-004-durable-entry-state-and-fenced-claims.md): Workers need temporary exclusive settlement authority over each unresolved target so crashes permit recovery and an obsolete worker cannot overwrite a newer attempt or a completed sibling. Includes reserved policy identifiers.
- [INBOX-005 — Atomic local delivery and repeatable execution](INBOX-005-atomic-local-delivery-and-repeatable-execution.md): An application's local state must not commit without the completion evidence that prevents the same Inbox target from applying those effects again.
- [INBOX-006 — Processor metadata, failure settlement and cancellation](INBOX-006-processor-metadata-failure-settlement-and-cancellation.md): Operators need each processing attempt to retain its incoming identity and classify retryable failure, permanent failure, host shutdown and lost ownership differently so recovery does not fabricate completion or damage another owner's work.
- [INBOX-007 — Signals, workers and restart recovery](INBOX-007-signals-workers-and-restart-recovery.md): Durable incoming work must remain recoverable after a missed in-memory wake or a worker crash, while host load and infrastructure retry pressure remain bounded. Includes reserved policy identifiers.
- [INBOX-008 — Dead-letter inspection and independent management](INBOX-008-dead-letter-inspection-and-independent-management.md): Operators need to inspect unsuccessful targets and explicitly requeue or discard eligible work without erasing its diagnosis, resetting successful siblings, or implying all-or-nothing batch management.
- [INBOX-009 — Replay retained admissions into missing targets](INBOX-009-replay-retained-admissions-into-missing-targets.md): Applications need to assign missing consumer obligations to retained incoming history without replacing its envelope or resetting any already represented target. Includes reserved policy identifiers.
- [INBOX-010 — Terminal cleanup and admission-retention boundary](INBOX-010-entry-cleanup-and-indefinite-admission-retention.md): Hosts need bounded removal of old terminal processing evidence without deleting unresolved work. Includes reserved policy identifiers.
- [INBOX-011 — Composition, mappings and public options](INBOX-011-composition-mappings-and-public-options.md): Host authors need a predictable public composition contract for durable Inbox processing and clear separation between valid service/model configuration and an actually installed persistence schema.
- [INBOX-012 — Scoped cleanup composition requirement](INBOX-012-scoped-cleanup-composition-requirement-and-current-mismatch.md): An application must be able to start a scope-validating host and run cleanup without retaining a scoped persistence context for the singleton worker's lifetime.
- [INBOX-013 — Replay failure transparency requirement](INBOX-013-replay-failure-transparency-requirement.md): An operator must not be told that replay succeeded merely because a failed persistence operation was converted into a small or zero result count.
- [INBOX-014 — Admission and replay policy boundaries](INBOX-014-undecided-admission-and-replay-policies.md): Applications need explicit durable-admission, replay and retention policy decisions before relying on consumerless acceptance, historical target creation or an unbounded duplicate-protection horizon.
- [INBOX-015 — Replay failure reconciliation policy](INBOX-015-unapproved-replay-failure-improvement.md): Callers need truthful replay outcomes when concurrent requests or persistence failures make insertion results uncertain. Includes reserved policy identifiers.

## Related documentation

- [Specification index](../INDEX.md)
- [Verification boundaries](../../reference/testing.md#verification-boundaries)
- [Repository instructions](../../../.github/copilot-instructions.md)
- [coding standards](../../CODING_STANDARDS.md)
- [documentation index](../../README.md)
- [architecture](../../reference/architecture.md)
- [configuration](../../reference/configuration.md)
- [contracts](../../reference/contracts.md)
- [inbox](../../reference/inbox.md)
- [outbox](../../reference/outbox.md)
- [domain events](../../reference/domain-events.md)
- [persistence](../../reference/persistence.md)
- [processing](../../reference/processing.md)
- [transport](../../reference/transport.md)
- [capability boundaries](../../explanation/capability-boundaries.md)
- [message identity](../../explanation/message-identity.md)
- [transaction boundaries](../../explanation/transaction-boundaries.md)
- [delivery semantics](../../explanation/delivery-semantics.md)
- [durable workflows](../../explanation/durable-workflows.md)
- [package contract and source map](../../reference/contract-map.md)
- [GAPS.md](../GAPS.md)
