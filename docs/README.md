# Clean.Messaging documentation

Documentation for engineers integrating, maintaining, and reasoning about Clean.Messaging. Product documentation follows [Diátaxis](https://diataxis.fr/): **Reference** provides facts to consult while working; **Explanation** develops understanding of relationships and trade-offs. Tutorials and how-to guides are outside this documentation scope.

## Reference

Source-reviewed contracts and current behaviour, with conditional guarantees and limitations stated explicitly.

| Page | Lookup question |
| --- | --- |
| [Architecture](reference/architecture.md) | Which packages exist, how do they depend on one another, and where are the host boundaries? |
| [Contracts and identities](reference/contracts.md) | What are the message/consumer contracts, serialization rules, identities and shared results? |
| [Configuration and host integration](reference/configuration.md) | Which registration surfaces, lifetimes and validation boundaries apply? |
| [Inbox](reference/inbox.md) | How are messages admitted, dispatched, deduplicated, replayed and managed? |
| [Outbox](reference/outbox.md) | How is outgoing work staged, targeted, delivered and managed? |
| [Domain events](reference/domain-events.md) | How are raised events captured and acknowledged? |
| [Persistence](reference/persistence.md) | Which mappings, stores, keys, interceptors and transaction restrictions apply? |
| [Processing](reference/processing.md) | How do claims, retries, cancellation, workers, cleanup and observability behave? |
| [Transport and MQTT](reference/transport.md) | What are the mapping, route, metadata, connection and acknowledgement contracts? |
| [Scheduling](reference/scheduling.md) | How are durable schedules stored, executed, cancelled and integrated with outbox/Quartz? |
| [Sagas](reference/sagas.md) | How do definitions, handlers, state, timers, effects and concurrency work? |
| [Automated testing](reference/testing.md) | Which test project, dependencies, runner commands and isolation boundaries are supported? |

## Explanation

Conceptual reasoning based on the current mechanisms, not undocumented claims about historical intent.

| Page | Understanding question |
| --- | --- |
| [Capability boundaries](explanation/capability-boundaries.md) | How does a reusable library relate to application ownership and provider boundaries? |
| [Message identity](explanation/message-identity.md) | Why distinguish logical operations, deliveries, targets and attempts? |
| [Transaction boundaries](explanation/transaction-boundaries.md) | Which changes can be atomic, and why is admission independent of processing? |
| [Delivery semantics](explanation/delivery-semantics.md) | Why can delivery repeat despite durable storage, claims and broker QoS? |
| [Durable workflows](explanation/durable-workflows.md) | How do sagas, outbox effects and durable timers cooperate across time? |

## Behavioural specifications

[Specifications index](specifications/INDEX.md): capability-organised contracts and stable acceptance criteria, with shared boundaries and individual numbered specification pages. Specifications distinguish **Required**, **Observed**, **Proposed** and **Unknown** behaviour; source observations and proposals are not automatically approved requirements. [Open decisions](specifications/GAPS.md) remain explicit. The [testing reference](reference/testing.md) describes repeatable execution and stable criterion metadata; run results belong in test and CI artifacts.

Specifications are a separate responsibility from Diátaxis Reference and Explanation: they define the candidate behavioural baseline against which a future test suite can be reviewed.

## Governance and maintenance

These documents are not forced into a Diátaxis product-documentation quadrant:

- [Coding standards](CODING_STANDARDS.md): normative contribution and design guidance.
- [Copilot instructions](../.github/copilot-instructions.md): concise repository guidance for coding agents.
- [Repository maintenance](maintenance.md): dated checkout observations, not architectural guarantees.

## Evidence boundary

The baseline is source inspection on 2026-10-13, not executed integration examples or a supported-provider certification. Reference pages link to authoritative implementation; Explanation distinguishes conceptual consequences and alternatives from recorded design decisions. The documentation covers the current capability and integration surface, not generated XML documentation for every public implementation type.
