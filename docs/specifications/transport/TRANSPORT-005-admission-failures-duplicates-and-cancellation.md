# TRANSPORT-005 — Admission failures, duplicates and cancellation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** [TransportIngress](../../../src/Clean.Messaging.Inbox.Ingress/TransportIngress.cs), [InboxIngressException](../../../src/Clean.Messaging.Inbox.Ingress/InboxIngressException.cs), dependency [InboxStore](../../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxStore.cs), [inbox mappings](../../../src/Clean.Messaging.Inbox.EntityFrameworkCore/InboxConfiguration.cs), [Inbox](../../../src/Clean.Messaging.Inbox/Inbox.cs).

The EF observations here are conditional on correct relational configuration, retained admissions and registered consumer contracts. Generic ingress itself implements neither duplicate detection nor acknowledgement.

## TRANSPORT-005-AC-01 — Mapping and persistence failures remain distinguishable
**Given** a serializer/mapper that throws before inbox invocation, **When** ingress accepts the envelope, **Then** it exposes `InboxIngressException` preserving the original exception as its inner cause (or preserves an existing ingress exception). **Given** successful mapping followed by an inbox admission failure, **When** admission fails, **Then** that failure escapes without ingress wrapping.

## TRANSPORT-005-AC-02 — Concurrent retained duplicate admissions create one obligation set
**Given** valid identical envelopes, registered consumers and the relational inbox schema, **When** two concurrent admissions compete using the same message/event identity, **Then** successful admissions leave one admission and no duplicate event/consumer entries; completed retained consumer work is not reset by the duplicate.

## TRANSPORT-005-AC-03 — Identity conflict does not overwrite retained data
**Given** a retained admission and a newly mapped envelope sharing either message ID or event ID but carrying different payload/context, **When** admission reaches the built-in EF store, **Then** it returns as already accepted without replacing the stored admission or comparing payload equivalence. This is an observed conflict policy, not approval for identity reuse for distinct operations.

## TRANSPORT-005-AC-04 — Admission I/O cancellation remains cancellation
**Given** successful mapping and an admission operation blocked on cancellable I/O, **When** its supplied cancellation token is cancelled, **Then** inbox I/O receives that token and the cancellation exception propagates without ordinary ingress-failure wrapping; no successful admission result is manufactured. A commit that already happened is not undone by later cancellation.

## TRANSPORT-005-AC-05 — Synchronous mapper cancellation currently becomes a mapping fault
**Given** a mapper or payload serializer throwing `OperationCanceledException` before inbox invocation, **When** ingress runs even with its token cancelled, **Then** generic ingress wraps that exception as `InboxIngressException`, since the synchronous mapper has no token contract. This discrepancy with normative cancellation guidance is pending [GAP-TX-002](../GAPS.md#gap-tx-002), not an approved cancellation policy.

## TRANSPORT-005-AC-06 — Independent admission rejects ambient transactions
**Given** valid ingress input and an ambient transaction in the caller, **When** the EF inbox admission path is invoked, **Then** admission fails rather than joining that transaction. **Given** no ambient transaction and committed admission, **When** unrelated caller work later fails, **Then** that caller failure does not by itself erase admitted work.
