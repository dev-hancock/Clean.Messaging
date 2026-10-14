# MQTT-011 — Outgoing connection ownership, concurrency and disposal

[Capability catalogue](README.md) | [Specification index](../INDEX.md)

## Intent

Outgoing attempt owners need one shared establishment boundary, cancellation and explicit ownership without a hidden second retry controller.

Authority: Explicit singleton connection ownership and deliberate durable-owned retry/no second adapter retry documentation, protected by coding standards on lifecycle and cancellation. Composition does not authorize arbitrary publish/disposal races. See [coding standards](../../CODING_STANDARDS.md).

## Guarantees

- Concurrent disconnected callers MUST share serialized connection establishment and reuse successful connectivity. This MUST NOT imply globally serialized publication or full client concurrency certification.
- Failed establishment MUST fail the attempt rather than assume publication. Later durable attempts MUST reconnect when disconnected; the adapter MUST NOT add an outgoing background retry loop.
- Cancellable connection wait/I/O MUST receive the attempt token without manufactured publication/completion on cancellation.

## Contract boundaries

Connected-client return is not publication success; PublishAsync still receives cancellation. The connection lock is not a global publish lock. Client/semaphore release belongs to the connection after host work stops; graceful disconnect, draining and racing disposal support remain unresolved.

## Acceptance criteria

### MQTT-011-AC-01 — Concurrent disconnected dispatches share connection establishment

- **Given** a disconnected shared client with concurrent outgoing callers.
- **When** one connection succeeds and waiting callers recheck.
- **Then** they reuse it without simultaneous separate establishment; publication is not globally serialized by the connection lock.

### MQTT-011-AC-02 — Connection rejection and later disconnect require another durable attempt

- **Given** establishment returns disconnected, or a later disconnect occurs.
- **When** the failed or later durable attempt requests connectivity.
- **Then** the failed attempt does not publish on assumed connectivity and a later request reconnects, without an independent background retry loop.

### MQTT-011-AC-03 — Connection wait cancellation does not imply publication

- **Given** another caller waits on establishment or cancellable connect I/O.
- **When** its attempt token cancels.
- **Then** wait/I/O can cancel without publication/completion; an already-connected result still relies on PublishAsync cancellation.

## Open decisions

The original IDs and exact heading anchors below remain reserved, not normative scenarios. No replacement IDs are assigned before a decision.

### MQTT-011-AC-04 — Disposal releases owned client without promised graceful disconnect

What publish/disposal overlap and graceful-disconnect/draining contract is supported? Reserve the whole ownership/lifecycle composite rather than narrowing its ID to disposal alone. See [GAP-TX-006](../GAPS.md#gap-tx-006); the recommendation remains unapproved.
