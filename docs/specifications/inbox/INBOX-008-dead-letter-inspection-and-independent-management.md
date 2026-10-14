# INBOX-008 — Dead-letter inspection and independent management

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need to inspect unsuccessful targets and explicitly requeue or discard eligible work without erasing its diagnosis, resetting successful siblings, or implying all-or-nothing batch management.

## Guarantees

- Inspection MUST return only retained nondiscarded dead letters, bounded by a positive limit and ordered oldest dead-letter first.
- Requeue MUST transition only eligible dead-letter keys, preserve envelope/identity and latest diagnosis, reset attempts and terminal/claim/due state, and make transitioned work available to processing.
- Discard MUST mark only eligible nondiscarded dead letters without deleting their diagnosis or pretending they succeeded. Valid empty collections MUST produce zero transitions.
- Management counts MUST reflect requested keys and successful transitions, not invented batch atomicity. Previously committed per-key changes MUST remain visible after later failure or cancellation.
- Null collections and ambient transactional mutations MUST be rejected visibly.

The independent basis is the [coding standards](../../CODING_STANDARDS.md), especially the cited identity, lifetime, failure and local-transaction requirements. Public management/result compatibility and coding standards sections 7 and 9 for failure diagnosis, per-target transitions and explicit transaction boundaries establish the management contract.

## Contract boundaries

Requeue and discard operate independently per requested key, including repeated keys; Requested counts supplied keys, while success counts actual eligible transitions. They are not an API to reset successful entries. Wake hints reduce latency but do not constitute durability. Cleanup may later remove diagnosis under the separate retention contract.

## Acceptance criteria

### INBOX-008-AC-01 — Inspect active dead letters with limit validation

- **Given** retained rows include nondiscarded dead letters and other lifecycle states.
- **When** inspection is requested with a positive limit or a nonpositive limit.
- **Then** positive inspection returns at most that many active dead letters in oldest-first order, including their key, event ID, attempts and failure summary, while a nonpositive limit fails.

### INBOX-008-AC-02 — Requeue only eligible keys and wake

- **Given** requested keys include eligible dead letters and ineligible or repeated keys.
- **When** requeue completes.
- **Then** only eligible transitions count as Requeued, Requested counts supplied keys, identities/payload/diagnosis remain, attempts and terminal/claim/due state reset, and successful requeue signals processing.

### INBOX-008-AC-03 — Discard and empty collections

- **Given** eligible and already discarded keys are submitted, or a valid empty collection is supplied.
- **When** discard or empty management completes.
- **Then** discard records only eligible transitions without deletion or success completion, and empty operations return zero counts.

### INBOX-008-AC-04 — Expose partial success and reject ambient management

- **Given** an earlier per-key mutation committed before later failure/cancellation, or an ambient transaction surrounds management.
- **When** the batch is interrupted or ambient mutation is attempted.
- **Then** earlier durable transitions remain rather than rolling back as a batch, and ambient mutation is rejected.

### INBOX-008-AC-05 — Reject null management collections

- **Given** a null key collection is supplied.
- **When** requeue or discard is requested.
- **Then** ArgumentNullException remains observable and no key transition occurs; the null collection is not a valid empty request.

## Open decisions

None.
