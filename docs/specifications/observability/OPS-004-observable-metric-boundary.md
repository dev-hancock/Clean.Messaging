# OPS-004 — Observable metric boundary

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Operators need stable lifecycle measurements whose names and attribution can be consumed consistently. A measurement is evidence of its declared local observation, not an exactly-once count of remote business effects or proof that telemetry was exported.

## Guarantees

- Public metric operations MUST record one increment on their corresponding named Clean.Messaging counter with the supplied source tag.
- Rejected retry/dead-letter settlement MUST NOT increment a successful settlement counter; observed claim loss MUST be attributed instead.
- Completion observations MUST NOT be represented as unique new database transitions or exactly-once business-effect counts.
- Library registration MUST NOT be represented as installing external collection/exporters or automatic correlation logger scopes.

## Contract boundaries

The public meter is Clean.Messaging; Completed, Retried, DeadLettered and LeaseLost correspond to messaging.completed, messaging.retries, messaging.dead_letters and messaging.lease_losses. Host listeners/exporters own collection. A dispatcher can report retained completion without a new transition. Metric attribution does not settle the pending log-wording decision.

## Acceptance criteria

### OPS-004-AC-01 — Named counters

- **Given** a listener observes the Clean.Messaging meter and a public metric call supplies source X.
- **When** Completed, Retried, DeadLettered or LeaseLost records an observation.
- **Then** respectively messaging.completed, messaging.retries, messaging.dead_letters or messaging.lease_losses increments by one tagged source X.

### OPS-004-AC-02 — Rejected settlement metrics

- **Given** retry or dead-letter settlement returns false for a processing attempt.
- **When** its processing metrics are observed.
- **Then** no successful retry/dead-letter increment is recorded for that rejected settlement and claim loss is observed instead.

### OPS-004-AC-03 — Completion observation

- **Given** a local dispatcher short-circuits a retained already-completed target.
- **When** the processor observes the successful dispatcher result.
- **Then** a completion observation can occur without a new database transition and is not an exactly-once business-effect count.

### OPS-004-AC-04 — No automatic exporter

- **Given** only library services are registered and the host supplies no listeners/exporters.
- **When** the process starts.
- **Then** library registration establishes neither external metric collection nor automatic correlation logger scopes.

## Open decisions

- [GAP-CORE-007](../GAPS.md#gap-core-007): the related policy remains undecided; its recommendation is not approved.
