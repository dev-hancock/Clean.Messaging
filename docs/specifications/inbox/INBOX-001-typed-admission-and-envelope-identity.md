# INBOX-001 — Typed admission and envelope identity

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Incoming adapters need a durable acceptance boundary for a typed envelope, with stable delivery and logical identities so transport redelivery is not mistaken for a new operation. Acceptance transfers local processing responsibility; it does not announce a business outcome.

## Guarantees

- Valid typed admission MUST durably retain the envelope and independent initial consumer targets without claiming that any consumer completed or any broker acknowledgement occurred.
- The supplied nonempty delivery ID MUST remain MessageId. An omitted EventId MUST default to that ID; an explicit nonempty distinct EventId MUST remain distinct.
- Admission MUST reject a null envelope, null payload, empty delivery ID or explicitly empty event ID before creating durable work.
- The stored contract and payload MUST represent the concrete runtime message type. Supplied correlation and causation values, including absence, MUST be preserved without ambient substitution.
- An unregistered message contract MUST fail visibly rather than creating an undecodable accepted envelope.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. The public typed admission/envelope compatibility contract, together with coding standards sections 7, 9 and 14 for failure transparency and stable persisted identities.

## Contract boundaries

The typed IInbox operation owns admission; a nongeneric envelope is a representation, not an additional accepted operation. The application supplies stable identities and registered contracts/consumers. Initial target selection belongs to admission, not subsequent redelivery. No guarantee of admission-only support, consumer success, automatic contract migration or broker/database atomicity is made.

## Acceptance criteria

### INBOX-001-AC-01 — Admit without claiming consumer completion

- **Given** a valid typed envelope and its currently registered consumers.
- **When** admission succeeds.
- **Then** the envelope and one independent unresolved target for each selected consumer are durable, but neither consumer completion nor broker acknowledgement is asserted.

### INBOX-001-AC-02 — Default and distinct logical identity

- **Given** a valid delivery ID with either an omitted event ID or an explicit nonempty distinct event ID.
- **When** the envelope is admitted.
- **Then** MessageId equals the supplied delivery ID and EventId respectively equals that ID or preserves the distinct supplied value.

### INBOX-001-AC-03 — Reject invalid input before durable admission

- **Given** a null envelope or payload, an empty delivery ID, or an explicitly empty EventId.
- **When** admission is requested.
- **Then** the invalid input fails before any durable admission is created.

### INBOX-001-AC-04 — Use concrete contract and explicit metadata

- **Given** a payload passed through a base-type declaration with a registered concrete contract and explicit nullable correlation/causation values.
- **When** admission succeeds.
- **Then** the concrete contract and serialized concrete payload are retained, and the supplied metadata including null is preserved rather than inherited from ambient processing.

## Open decisions

The following original acceptance identifiers remain reserved; current behavior and unapproved recommendations do not establish normative authority.

### INBOX-001-AC-05 — Reject missing contracts or consumers

Reserved pending [GAP-DUR-001](../GAPS.md#gap-dur-001). The unresolved obligation combines required missing-contract failure with rejection of messages having no initial consumer and a prohibition on admission for later replay. Contract failure remains established above; no-consumer policy is not approved.

The complete original identifier remains reserved, not reassigned to its supported subset. Restore it only on approval of its original obligation, or approve an explicit split/replacement under new identifiers.
