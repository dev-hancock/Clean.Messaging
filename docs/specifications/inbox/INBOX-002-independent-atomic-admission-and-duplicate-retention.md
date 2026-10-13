# INBOX-002 — Independent atomic admission and duplicate retention

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: `InboxStore.Accept`, identity query and EF mappings; `IInboxStore.Accept` extends the durable port. [Persistence](../../reference/persistence.md) is descriptive. No API XML grants payload-equivalent idempotency.

Precondition: correctly mapped relational store and no ambient Transaction.Current. Postcondition: admission and initial targets save together in an independent context. Duplicate acceptance is identity-only, not a content comparison or registration expansion. Failures: ordinary database errors propagate unless an admission with either identity is confirmed after DbUpdateException. Concurrency: unique constraints fence concurrent insertions; cancellation may race a durable save, so return failure alone does not prove absence.

## INBOX-002-AC-01 — Do not join caller changes

- **Given** unsaved application changes or an explicit transaction in the caller context, without an ambient transaction.
- **When** Accept succeeds and caller changes are later abandoned.
- **Then** admission remains independently durable and caller changes are not saved by admission.

## INBOX-002-AC-02 — Reject ambient admission

- **Given** Transaction.Current is present.
- **When** Accept or store Replay is requested.
- **Then** the independent-admission restriction throws without joining the ambient transaction.

## INBOX-002-AC-03 — Suppress duplicates by either retained identity

- **Given** a retained admission, including terminal targets, and a new envelope reusing MessageId or EventId but different payload/metadata or new registrations.
- **When** Accept finishes.
- **Then** the existing original envelope and target set remain unchanged; no new-consumer expansion or conflict report is supplied.

## INBOX-002-AC-04 — Resolve concurrent duplicate admission

- **Given** two independent callers submit the same new delivery/event identity.
- **When** their saves race.
- **Then** at most one admission and one row per original target persist; the losing duplicate can succeed when retained identity is confirmed; unrelated save errors remain failures.

## INBOX-002-AC-05 — Recover after admission and lost acknowledgement

- **Given** admission saved but the process stopped or caller was cancelled before upstream acknowledgement.
- **When** the same identity is submitted again after restart.
- **Then** retained admission blocks additional initial entries; pending targets remain recoverable regardless of a lost wake hint.
