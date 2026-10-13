# INBOX-001 — Typed admission and envelope identity

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: `Inbox.Accept`, `InboxMessage<T>`, `InboxBatch`, `InboxAdmission`; shared serializer and exact-type consumer registry. Reference: [inbox](../../reference/inbox.md) and [contracts](../../reference/contracts.md) are descriptive, not normative.

Purpose: admit a typed delivery for later local consumption. Contracts: `IInbox.Accept<T>` / `Inbox.Accept<TMessage>`; both inbox message records; admission/batch envelope members. Preconditions: nonnull envelope and payload, nonempty Id and supplied EventId, runtime payload contract and consumers registered. Postcondition: successful new admission retains one original envelope and one unresolved entry per current consumer. Invariants: `MessageId = Id`; `EventId = supplied EventId ?? Id`; payload, CreatedAt and context metadata are common to targets. The nongeneric record is representation only: there is no nongeneric Accept operation. Constructor/required members do not independently validate runtime values.

## INBOX-001-AC-01 — Admit without claiming consumer completion

- **Given** a valid typed envelope with two registered exact-type consumers.
- **When** Accept finishes successfully.
- **Then** the original envelope and two independent pending targets are retained; neither consumer completion nor broker acknowledgement is implied.

## INBOX-001-AC-02 — Default and distinct logical identity

- **Given** a nonempty delivery Id, first with absent EventId and then a distinct nonempty EventId.
- **When** each envelope is admitted in a clean store.
- **Then** the first logical identity equals delivery Id; the second preserves distinct delivery and event identities.

## INBOX-001-AC-03 — Reject invalid input before durable admission

- **Given** a null envelope, null payload, empty Id or explicit empty EventId.
- **When** Accept is requested.
- **Then** the request fails and creates no admission or target entries.

## INBOX-001-AC-04 — Use concrete contract and explicit metadata

- **Given** a payload held through a base/object generic type, registered concrete contract/consumers, and supplied nullable metadata.
- **When** Accept is requested while unrelated ambient delivery metadata exists.
- **Then** the concrete contract and payload are stored; supplied correlation/causation, including null, are preserved without ambient fallback.

## INBOX-001-AC-05 — Reject missing contracts or consumers

- **Given** a valid envelope whose runtime contract is missing or whose exact runtime type has no consumers.
- **When** Accept is requested.
- **Then** contract failure or permanent consumer.message_unhandled failure propagates; no envelope is admitted for future replay.
