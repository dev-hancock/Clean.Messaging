# MQTT-013 — External repeat, cancellation and independently successful targets

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Normative basis:** repository instructions preserve per-target completion, leases, cancellation/failure classifications and reject exactly-once external effects; coding standards §§7–9 require separating publication from local settlement and rejecting stale ownership.
**Source evidence:** dependencies [transport wrapper](../../../src/Clean.Messaging.Outbox/Transport/OutboxTransport.cs), [processor](../../../src/Clean.Messaging.Outbox/Runtime/OutboxProcessor.cs), [durable store](../../../src/Clean.Messaging.EntityFrameworkCore/DurableStore.cs), MQTT publishing path.

## MQTT-013-AC-01 — Post-publish crash can produce the same logical publication again
**Given** the broker accepted publication but the process failed before persistent target completion, **When** durable recovery reclaims the unresolved entry, **Then** publication may repeat with the same retained message/event IDs and stored destination; neither QoS 2 nor local uniqueness proves a single subscriber effect.

## MQTT-013-AC-02 — Lease fencing limits settlement, not an issued publish
**Given** a publish in flight when its claim expires and another owner starts, **When** the stale publish finishes and tries database completion, **Then** its old claim cannot complete the new owner's work, although remote effects may overlap or have already succeeded.

## MQTT-013-AC-03 — Shutdown and claim-loss cancellation are not ordinary poison
**Given** a cancellable outgoing attempt, **When** shutdown or lease-loss cancels handling, **Then** cancellation flows to connection/publication/settlement boundaries and is not treated as a broker poison discard or ordinary permanent publish rejection. A publication accepted before cancellation is not claimed to be reversed.

## MQTT-013-AC-04 — Successful sibling target remains settled during another failure
**Given** one retained MQTT/local target is completed and another MQTT target fails, **When** durable retries/recovery process the failed target, **Then** the completed sibling is not deliberately re-executed or reset solely for that failure; no global topic/event completion ordering is promised.
