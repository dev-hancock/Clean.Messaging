# RETRY-002 — Failure snapshot values and bounds

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** MessageFailureFactory maps UTC and truncates known messages/code/type; unknown code is processing.message.unhandled.

Pre: nonnull exception and supplied attempt/time. Post: snapshot reports latest failure using UTC and bounded strings; unsupported FailureAction values map Unknown.

## RETRY-002-AC-01 — Snapshot truncation
**Given** a messaging exception has code over 200 and message over 500 characters.
**When** the snapshot is created.
**Then** code is at most 200 and stored message/type at most 500 characters, preserving leading text.

## RETRY-002-AC-02 — Failure time and attempt
**Given** an exception is supplied with attempt N and a nonzero-offset instant.
**When** snapshot is created.
**Then** Attempt remains N and FailedAt is the equivalent UTC instant.

## RETRY-002-AC-03 — Unknown category
**Given** a nonmessaging exception.
**When** snapshot is created.
**Then** Category is Unknown, code is `processing.message.unhandled`, and exception type remains identifiable.

## RETRY-002-AC-04 — Null exception
**Given** a null exception.
**When** failure snapshot creation is requested.
**Then** ArgumentNullException is raised rather than a success snapshot.
