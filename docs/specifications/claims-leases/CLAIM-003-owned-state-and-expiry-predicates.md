# CLAIM-003 — Renewal and settlement preserve ownership lifecycle

[Capability catalogue](README.md) · [Specification index](../INDEX.md)

## Intent

Workers need renewal to preserve an ongoing attempt and settlement to end that authority. Retry-waiting and terminal work must not be mistaken for a still-active attempt.

## Guarantees

- Owned updates MUST NOT treat retry-waiting, completed, dead-lettered or discarded work as an active claim.
- Successful renewal MUST preserve attempt count, message/target identities and completion state while updating the valid lease expiry.
- Successful completion, retry or dead-letter settlement MUST clear claim ownership and expiry. Completion and dead-letter are terminal; retry MUST leave unfinished work with the supplied next-attempt time.

## Contract boundaries

This contract covers valid current ownership and a valid later renewal expiry. Whether shortening or otherwise invalid renewal inputs are supported is separate [input-domain policy](../GAPS.md#gap-core-004). Renewal does not perform a handler invocation or make external work atomic. Exact expiry equality is not selected here.

## Acceptance criteria

### CLAIM-003-AC-02 — Waiting or terminal target

- **Given** a target is retry-waiting, completed, dead-lettered or discarded even though a matching key/token is supplied.
- **When** an owned update is requested.
- **Then** it is rejected without changing that target into an active attempt or altering its retained state.

### CLAIM-003-AC-03 — Successful renewal

- **Given** a current unexpired target and a valid later renewal expiry.
- **When** renewal succeeds.
- **Then** the renewed expiry is retained while Attempts, message/target identities and completion remain unchanged.

### CLAIM-003-AC-04 — Settlement clears ownership

- **Given** a valid owner requests completion, retry or dead-letter settlement.
- **When** settlement succeeds.
- **Then** claim/expiry are cleared; completion/dead-letter is terminal, and retry remains unfinished with the supplied next due time.

## Open decisions

[GAP-CORE-004](../GAPS.md#gap-core-004) covers valid renewal inputs. The reserved expiry criterion is governed by [GAP-CLAIM-002](../GAPS.md#gap-claim-002).

The following original identifiers are explicitly reserved, not normative acceptance scenarios. The original obligations remain associated with the decisions below; no narrower or opposite guarantee is assigned to those IDs.

### CLAIM-003-AC-01 — Exact expiry

Reserved for [GAP-CLAIM-002](../GAPS.md#gap-claim-002). Rejection of Renew/Complete/Retry/DeadLetter at exact expiry is the unresolved deadline-equality policy, not a general stale-owner replacement scenario.
