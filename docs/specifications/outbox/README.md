# Outbox capability catalogue

[Specification index](../INDEX.md)

## Capability

Outbox records outgoing intent with the state that justified it, then delivers independent local or external targets. Staging, owning commit, publication and target completion are distinct boundaries; external delivery can repeat after a successful remote effect and missing local completion.

## Contract boundaries

Atomic capture and local delivery require the configured scoped persistence transaction and installed integration. External publication and local completion are not one transaction. Hosts and recipients own protocol composition and effect-specific duplicate protection; targetless routing, content conflicts, uncertain-commit evidence and callback recovery remain explicit open decisions.

The specifications establish consumer-facing guarantees from independent standards and public compatibility. Reserved acceptance identifiers retain unresolved original obligations; an implementation observation or a documented recommendation is not policy approval. Required lifetime and failure-transparency obligations remain required even when the implementation differs.

## Specifications

- [OUTBOX-001 — Enqueue stages a concrete envelope and target snapshot](OUTBOX-001-enqueue-stages-a-concrete-envelope-and-target-snapshot.md): Applications need to stage the intent to send one logical operation to independent targets while preserving its concrete envelope and route snapshot.
- [OUTBOX-002 — Ambient outgoing metadata and representation members](OUTBOX-002-ambient-outgoing-metadata-and-representation-members.md): Consumers producing outgoing work need correlation continuity and immediate causal identity, while explicit overrides and later persisted delivery must not be replaced by unrelated ambient state.
- [OUTBOX-003 — Target validation, local selection and empty batches](OUTBOX-003-target-validation-local-selection-and-empty-batches.md): Hosts need target identities that distinguish independent obligations and survive persistence, without forcing provider-neutral routing to understand each transport's destination protocol. Includes reserved policy identifiers.
- [OUTBOX-004 — Tracked duplicate suppression versus persisted uniqueness](OUTBOX-004-tracked-duplicate-suppression-versus-persisted-uniqueness.md): Applications need retained event/target identity to prevent duplicate obligations while understanding that a staged duplicate guard is not an unconditional idempotent database enqueue or a content-conflict resolver. Includes reserved policy identifiers.
- [OUTBOX-005 — Atomic owning save and commit boundary](OUTBOX-005-atomic-owning-save-and-commit-boundary.md): An application must not commit the state that justifies an outgoing operation without durably recording that operation, or publish an operation whose owning local transaction did not commit.
- [OUTBOX-006 — Public save capture protocol and per-context deduplication](OUTBOX-006-public-save-capture-protocol-and-per-context-deduplication.md): Capture integrations need to recognize repeated facts within one owning context/transaction without acknowledging them before commit or sharing mutable lifecycle state across unrelated contexts. Includes reserved policy identifiers.
- [OUTBOX-007 — Save failure, cancellation, rollback and retry tracking](OUTBOX-007-save-failure-cancellation-rollback-and-retry-tracking.md): A failed save or known rollback must leave capture integrations able to retry their still-unacknowledged facts, without detaching caller-owned staged state or accidentally transferring pending work to another transaction.
- [OUTBOX-008 — Commit verification and uncertain outcome](OUTBOX-008-commit-verification-and-uncertain-outcome.md): Applications need an honest answer after a commit response is lost: local state may have committed, rolled back or remained unknown. Includes reserved policy identifiers.
- [OUTBOX-009 — Savepoints, ambient transactions and observer restrictions](OUTBOX-009-savepoints-ambient-transactions-and-observer-restrictions.md): Host authors need explicit limits on transaction observation so savepoints or ambient/unobserved transaction ends cannot silently invalidate the capture acknowledgement boundary.
- [OUTBOX-010 — Postcommit callbacks and wake hints](OUTBOX-010-postcommit-callbacks-and-wake-hints.md): Applications need to release in-memory capture responsibility only once outgoing work is durably owned, without confusing postcommit callbacks or wake hints with delivery to recipients. Includes reserved policy identifiers.
- [OUTBOX-011 — Transport dispatch and external repeat window](OUTBOX-011-transport-dispatch-and-external-repeat-window.md): Applications stage outgoing work so that a temporary transport failure does not silently lose the intent to deliver it.
- [OUTBOX-012 — Delivery registration and local atomic dispatch](OUTBOX-012-delivery-registration-and-local-atomic-dispatch.md): Hosts need unambiguous selection of a target's registered delivery integration.
- [OUTBOX-013 — Owned durable state, processor policy and cancellation](OUTBOX-013-owned-durable-state-processor-policy-and-cancellation.md): Outgoing target recovery needs temporary ownership and independent progress so a failing destination cannot erase a successful sibling or classify shutdown and stale ownership as ordinary message failure. Includes reserved policy identifiers.
- [OUTBOX-014 — Workers, signals, bounded queue and recovery](OUTBOX-014-workers-signals-bounded-queue-and-recovery.md): Outgoing work must survive missed hints and abandoned workers while bounded queueing and scoped processing keep durable recovery from exhausting host resources. Includes reserved policy identifiers.
- [OUTBOX-015 — Dead-letter management and cleanup horizon](OUTBOX-015-dead-letter-management-and-cleanup-horizon.md): Operators need to inspect, requeue or discard failed outgoing targets without erasing their history or resetting successful siblings, and clean old terminal evidence with an explicit duplicate-protection horizon.
- [OUTBOX-016 — EF composition, mappings, options and validation](OUTBOX-016-ef-composition-mappings-options-and-validation.md): Host authors need stable public registration and persistence integration while retaining responsibility for provider deployment, schema installation and valid service lifetimes.
- [OUTBOX-017 — Scoped cleanup lifetime requirement](OUTBOX-017-scoped-cleanup-lifetime-requirement.md): An application must be able to start scope-validating Outbox cleanup without a singleton worker retaining scoped persistence throughout the host lifetime.
- [OUTBOX-018 — Undecided duplicate, commit and callback policies](OUTBOX-018-undecided-duplicate-commit-and-callback-policies.md): Applications need explicit policy choices for identity conflict, targetless/external-only routing, uncertain commit and postcommit acknowledgement before treating accidental implementation outcomes as durable promises.

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
