# OUTBOX-011 — Transport dispatch and external repeat window

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour

**Evidence:** Normative: repository instructions/coding standards preserve identity, claims and failure classification and explicitly forbid exactly-once external-effect claims. Source: OutboxDispatcher.Dispatch, IOutboxDelivery, IOutboxTransport, OutboxTransport wrapper.

Purpose: route persisted work while retaining repeat-safe message identity. Contracts: IOutboxDelivery.Transport/Dispatch returns true for completion, false for ownership loss; dispatcher selects stored transport ordinally; IOutboxTransport.Name/Dispatch performs publication, not durable completion. Preconditions: resolvable delivery and valid ownership for settlement. Invariant: a successful transport return precedes separate fenced completion, never an atomic database+broker transaction. Duplicate delivery transport names fail construction; unknown stored name fails dispatch (observed details).

## OUTBOX-011-AC-01 — Preserve envelope across retries

- **Given** a stored external target is published and completion then fails or the process crashes.
- **When** the unresolved row is recovered and dispatched again.
- **Then** external publication may repeat with the same delivery/event/target/context identity; no exactly-once downstream effects are claimed.

## OUTBOX-011-AC-02 — Do not complete on transport failure

- **Given** external transport throws or observes cancellation before returning success.
- **When** the wrapper exits to the processor.
- **Then** durable completion is not manufactured; failure/cancellation remains distinguishable for processor policy.

## OUTBOX-011-AC-03 — Fence settlement after successful publication

- **Given** external publication succeeds but the owner loses its claim before Complete.
- **When** the wrapper attempts settlement.
- **Then** false completion cannot overwrite the new owner; publication already happened and repeat remains possible.
