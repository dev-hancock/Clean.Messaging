# OPS-004 — Observable metric boundary

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** MessageMetrics counters are named and tagged public output; processors increment retry/dead-letter only after true settlement.

Pre: a listener subscribes to the Clean.Messaging meter. Post: public methods increment their named counter with source, not proof of remote business success.

## OPS-004-AC-01 — Named counters
**Given** a listener observes the meter and a public metric operation is invoked with source X.
**When** Completed/Retried/DeadLettered/LeaseLost records an observation.
**Then** respectively messaging.completed/retries/dead_letters/lease_losses increments by one with source X.

## OPS-004-AC-02 — Rejected settlement metrics
**Given** retry/dead-letter settlement returns false in a processing attempt.
**When** metrics are observed.
**Then** no successful retry/dead-letter increment is reported for that rejected settlement; claim loss is observed instead.

## OPS-004-AC-03 — Completion observation
**Given** a local dispatcher short-circuits an already completed entry.
**When** processor observes a successful dispatcher result.
**Then** a completion observation can occur without a new database transition; the counter is not exactly-once business-effect count.

## OPS-004-AC-04 — No automatic exporter
**Given** only library services are registered.
**When** a process starts without application listeners/exporters.
**Then** no claim is made that metrics are externally collected or that correlation becomes an automatic logger scope.
