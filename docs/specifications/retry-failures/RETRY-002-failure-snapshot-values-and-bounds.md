# RETRY-002 — Failure snapshot values and bounds

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need retained failure information to fit the durable representation and identify when and on which attempt the latest failure occurred.

## Guarantees

- Snapshots MUST bound code to 200 and stored message/exception type to 500 characters, preserving leading diagnostic text.
- Snapshot Attempt MUST preserve the supplied count and FailedAt MUST represent the supplied instant in UTC.
- A nonmessaging exception MUST retain Unknown category, processing.message.unhandled code and an identifiable exception type.
- Creating a snapshot from null MUST reject the call with ArgumentNullException rather than create a success record.

## Contract boundaries

A snapshot records a failure, not proof that retry/dead-letter settlement committed. Callers supply the attempt value; its acquisition accounting is not decided by snapshot creation. Bounds reflect the supported durable diagnostic representation, not an unlimited exception dump.

## Acceptance criteria

### RETRY-002-AC-01 — Snapshot truncation

- **Given** a messaging exception has a code longer than 200 characters and message text longer than 500 characters.
- **When** its failure snapshot is created.
- **Then** code is at most 200 and stored message/type at most 500 characters, retaining the leading text of each bounded diagnostic.

### RETRY-002-AC-02 — Failure time and attempt

- **Given** an exception is supplied with attempt N and a nonzero-offset instant.
- **When** snapshot is created.
- **Then** Attempt remains N and FailedAt is the equivalent UTC instant.

### RETRY-002-AC-03 — Unknown category

- **Given** a nonmessaging exception.
- **When** snapshot is created.
- **Then** Category is Unknown, code is `processing.message.unhandled`, and exception type remains identifiable.

### RETRY-002-AC-04 — Null exception

- **Given** a null exception.
- **When** failure snapshot creation is requested.
- **Then** ArgumentNullException is raised rather than a success snapshot.

## Open decisions

[GAP-CLAIM-001](../GAPS.md#gap-claim-001) leaves attempt accounting open; retaining a supplied count does not approve its acquisition semantics.
