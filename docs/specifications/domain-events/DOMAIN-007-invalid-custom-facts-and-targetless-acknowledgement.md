# DOMAIN-007 — Invalid custom facts and targetless acknowledgement

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Evidence:** Source: RaisedEvent lacks guards; DomainEventCapture queues acknowledgement then calls factory/writer; factory validates envelope/payload/contracts/targets. [GAP-DUR-011](../GAPS.md#gap-dur-011) covers zero-target acknowledgement.

Contracts: custom IDomainEventSource events and outgoing factories. Preconditions: nonempty custom event ID, nonnull serializable payload and applicable targets for ordinary durable fan-out. Capture failures occur inside save interception, preserving source pending facts. However an empty custom target batch successfully stages no rows while capture/ack state exists; successful observed commit can acknowledge without outgoing rows. Failed commit without keys cannot be verified as success. Default no-consumer provider throws earlier, not an empty target result.

## DOMAIN-007-AC-01 — Fail invalid custom fact capture

- **Given** custom source returns empty Id, null/unserializable payload, missing contract or default no-consumer payload.
- **When** owning save capture runs.
- **Then** factory failure aborts save capture and source facts stay pending; invalid RaisedEvent construction alone had not rejected input.

## DOMAIN-007-AC-02 — Observe targetless successful acknowledgement

- **Given** a valid event uses custom providers returning no targets.
- **When** save and observed commit succeed.
- **Then** no outgoing target rows are committed but the captured source event can still be acknowledged; this is observed, not an approved delivery guarantee.

## DOMAIN-007-AC-03 — Keep targetless failed commit uncertain

- **Given** targetless captured event has no outgoing keys and commit notification fails.
- **When** reconciliation executes.
- **Then** key verification cannot prove committed capture; acknowledgement remains tied to observed/verified outcome rather than assuming success.
