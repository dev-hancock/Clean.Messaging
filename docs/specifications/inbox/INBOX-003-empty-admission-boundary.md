# INBOX-003 — Empty admission boundary

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: `InboxBatch.IsEmpty`, `InboxStore.CreateAdmission` requires a first entry despite a complete batch envelope; the default consumer registry rejects no-consumer types earlier. See [GAP-DUR-001](../GAPS.md#gap-dur-001).

Contracts: public IInboxStore.Accept permits representing an empty InboxBatch; EF implementation does not accept a new empty batch. A retained duplicate is checked before creation. This is a source-derived boundary, not an approved desired contract.

## INBOX-003-AC-01 — Reject a new empty batch

- **Given** an unretained InboxBatch with complete original envelope and Entries empty.
- **When** the EF store accepts it directly.
- **Then** InvalidOperationException occurs and no admission is retained.

## INBOX-003-AC-02 — Recognize retained empty duplicate

- **Given** an already retained MessageId or EventId and an otherwise empty batch.
- **When** the EF store accepts the duplicate directly.
- **Then** it returns without adding targets; empty-batch validation is not applied to retained identities.
