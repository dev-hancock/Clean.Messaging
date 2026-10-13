# OUTBOX-004 — Tracked duplicate suppression versus persisted uniqueness

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: IOutboxStore.Stage, IOutboxWriter.Stage, OutboxWriter, OutboxStore and mappings. [GAP-DUR-007](../GAPS.md#gap-dur-007) covers the scope of idempotency.

Precondition: valid batch and owning context. Postcondition: missing tracked logical event/target pairs become Added. Invariant: same EventId/TargetId tracked nondeleted rows suppress new rows without comparing payload or envelope. No database lookup occurs; fresh-context persisted duplicates can fail the owning save. Entry primary key (MessageId,TargetId), unique key (EventId,TargetId); retention deletes that evidence. Deleted tracked rows do not suppress replacement.

## OUTBOX-004-AC-01 — Suppress only nondeleted tracked target pairs

- **Given** same logical event/target is already tracked nondeleted with possibly different payload; another target is absent.
- **When** Stage runs in that context.
- **Then** existing tracked pair is left unchanged and only the missing target is staged; no payload-equivalence guarantee is given.

## OUTBOX-004-AC-02 — Expose duplicate persisted save failure

- **Given** the pair is stored but not tracked in a new context.
- **When** Enqueue/Stage adds the duplicate and owning SaveChanges runs.
- **Then** staging can return normally but unique event/target persistence can fail; successful enqueue is not unconditional durable idempotency.

## OUTBOX-004-AC-03 — Allow deleted tracked replacements and cleaned identities

- **Given** an existing pair is tracked Deleted or its old terminal row was physically cleaned.
- **When** a replacement is staged and saved with valid transaction semantics.
- **Then** deleted tracked state does not suppress staging; absent retained logical evidence no longer prevents a new delivery.
