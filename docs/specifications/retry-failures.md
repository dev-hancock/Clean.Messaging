# Retry policy and failure classification — Behavioural specification

**Status:** Draft; source reviewed 2026-10-13; no acceptance criterion has been executed.
**Scope:** `MessageException` (Code, Action, IsTransient, inner exception), `FailureAction`, `MessageFailureFactory.Create`, `MessageFailure`, `FailureCategory`, `RetryOptions`, `RetryBackoff`, `RetryPolicy.Evaluate`, `RetryResult.Fault/After`.

## Purpose
Expose classified failure decisions and bounded failure-retry delays without conflating broker redelivery, provider retries, claims or business workflow failure.

## Contracts, preconditions and postconditions
Pre: positive policy MaxAttempts, valid mode and consistent attempt accounting; runtime supplies claimed attempt count. Post: policy returns retry eligibility and delay or Fault; it does not execute the consumer itself. Failure snapshots are diagnostics, not proof of terminal status.

## Invariants
- Permanent message failures do not become successful outcomes.
- Shutdown/lease loss is not silently converted into an ordinary durable message fault.
- Exhaustion and failure category remain different concepts.

## Failure, cancellation and consistency
Criteria below distinguish invalid use, failed execution, unsupported boundaries and loss of ownership. Required criteria protect established repository contracts; observed criteria are unapproved compatibility baselines. Proposed criteria are expressly unapproved. Unknown specifications have no decided acceptance outcome and are counted separately as gaps, not executable requirements.

## Limitations
- No fixed total number of consumer invocations/external effects, retry-until-success guarantee or schedule/MQTT/EF retry unification.
- Public direct policy inputs outside normal positive attempts are unresolved, and enum validation is incomplete.

## Specifications and acceptance criteria

### RETRY-001 — Failure action and safe diagnostics
**Classification:** Required behaviour
**Evidence:** Coding standards §7 requires preserving code/transient/permanent distinctions and §13/§17 prohibits unnecessary sensitive diagnostics.

Pre: a messaging exception conveys Retry/Fault. Post: classification preserves action/code; unknown exceptions must not be exposed as fabricated success or persisted sensitive payload text.

#### RETRY-001-AC-01 — Messaging classification
**Given** MessageException conveys Retry or Fault with a code.
**When** its failure snapshot is created.
**Then** Retry maps to Transient and Fault to Permanent while the code is preserved within the bounded diagnostic field.

#### RETRY-001-AC-02 — Unknown exception message
**Given** an arbitrary exception includes sensitive business payload text in its Message.
**When** the standard failure snapshot is created.
**Then** stored Message is the generic unexpected-processing description rather than the arbitrary exception Message.

### RETRY-002 — Failure snapshot values and bounds
**Classification:** Observed behaviour
**Evidence:** MessageFailureFactory maps UTC and truncates known messages/code/type; unknown code is processing.message.unhandled.

Pre: nonnull exception and supplied attempt/time. Post: snapshot reports latest failure using UTC and bounded strings; unsupported FailureAction values map Unknown.

#### RETRY-002-AC-01 — Snapshot truncation
**Given** a messaging exception has code over 200 and message over 500 characters.
**When** the snapshot is created.
**Then** code is at most 200 and stored message/type at most 500 characters, preserving leading text.

#### RETRY-002-AC-02 — Failure time and attempt
**Given** an exception is supplied with attempt N and a nonzero-offset instant.
**When** snapshot is created.
**Then** Attempt remains N and FailedAt is the equivalent UTC instant.

#### RETRY-002-AC-03 — Unknown category
**Given** a nonmessaging exception.
**When** snapshot is created.
**Then** Category is Unknown, code is `processing.message.unhandled`, and exception type remains identifiable.

#### RETRY-002-AC-04 — Null exception
**Given** a null exception.
**When** failure snapshot creation is requested.
**Then** ArgumentNullException is raised rather than a success snapshot.

### RETRY-003 — Failure-retry budget
**Classification:** Observed behaviour
**Evidence:** RetryPolicy permits nonpermanent exceptions only when attempt < MaxAttempts; RetryResult describes a decision.

Pre: valid policy and claimed attempt >=1. Post: permanent faults stop immediately; transient/unknown failures consume the same configured failure budget.

#### RETRY-003-AC-01 — Permanent fault early
**Given** attempt 1 with remaining budget and MessageException Fault.
**When** policy evaluates.
**Then** ShouldRetry is false and Delay is zero.

#### RETRY-003-AC-02 — Transient or unknown before budget
**Given** Retry or ordinary exception at attempt below MaxAttempts.
**When** policy evaluates.
**Then** ShouldRetry is true with the configured calculated delay.

#### RETRY-003-AC-03 — Exhaustion boundary
**Given** a retryable exception at attempt equal to or above MaxAttempts.
**When** policy evaluates.
**Then** ShouldRetry is false without relabelling its original failure category.

#### RETRY-003-AC-04 — Decision not execution
**Given** a failure is evaluated.
**When** policy returns RetryResult.
**Then** no consumer invocation or persistence retry is performed by evaluating the decision alone.

### RETRY-004 — Backoff and jitter boundaries
**Classification:** Observed behaviour
**Evidence:** RetryPolicy formulas are fixed/linear/exponential capped before full jitter; see reference/processing.md defaults.

Pre: valid mode and nonnegative delay/cap. Post: delays respect configured mode/cap; jitter is a range, not deterministic timing of eventual handling.

#### RETRY-004-AC-01 — Mode delays
**Given** initial 2s, cap 1m, jitter disabled and attempts 1/2/3.
**When** Fixed, Linear and Exponential are evaluated.
**Then** delays are respectively (2,2,2), (2,4,6) and (2,4,8) seconds.

#### RETRY-004-AC-02 — Capped delay
**Given** a mode computes delay greater than MaxDelay.
**When** policy evaluates with jitter disabled.
**Then** the result delay equals MaxDelay, not the uncapped delay.

#### RETRY-004-AC-03 — Zero delay
**Given** InitialDelay is zero and options otherwise valid.
**When** retryable failure is evaluated.
**Then** zero delay is allowed and no positive minimum is invented.

#### RETRY-004-AC-04 — Full jitter
**Given** a positive unjittered bound D.
**When** retries are evaluated with jitter.
**Then** every returned delay is nonnegative and no greater than D; no fixed minimum or distribution is asserted.

#### RETRY-004-AC-05 — Retry options validity
**Given** MaxAttempts <=0, negative InitialDelay or MaxDelay < InitialDelay.
**When** standard option validation runs.
**Then** those values are rejected; zero InitialDelay is accepted.

### RETRY-005 — Processing settlement boundary
**Classification:** Required behaviour
**Evidence:** Coding standards §7 and §9 preserve separate failure settlement and host/claim cancellation; processors use classified policy decisions after unsuccessful attempts.

Pre: active durable processor with local unsuccessful attempt. Post: retry/dead-letter metadata can be recorded independently of rolled-back consumer work, but only by current ownership.

#### RETRY-005-AC-01 — Failure rollback and retry
**Given** consumer state rolls back while claim remains valid and policy permits retry.
**When** failure settlement succeeds.
**Then** a later due attempt is recorded without committing the failed consumer changes.

#### RETRY-005-AC-02 — Lost ownership
**Given** the attempt fails after loss is signalled.
**When** processor handles that failure.
**Then** it does not overwrite successor state with retry/dead-letter metadata.

#### RETRY-005-AC-03 — Host cancellation
**Given** handling throws cancellation due to the host stopping token.
**When** processor exits.
**Then** the target is left recoverable by expiry rather than dead-lettered solely because of shutdown.

#### RETRY-005-AC-04 — Settlement timeout
**Given** failure settlement cannot complete within its separate configured timeout.
**When** the bound elapses.
**Then** no successful retry/dead-letter settlement is claimed; durable state must be inspected/recovered later.

### RETRY-006 — Out-of-domain public policy inputs
**Classification:** Unknown behaviour
**Evidence:** Evaluate does not explicitly reject null exceptions or negative/zero attempts; Backoff validation omits enum membership.

Unknown: public Evaluate preconditions beyond runtime-supplied positive attempts. Startup enum rejection is an unapproved CONFIG-006 proposal; do not freeze negative delay behavior as a valid requirement.

**Open decisions:** [GAP-CORE-006](GAPS.md#gap-core-006), [GAP-CORE-003](GAPS.md#gap-core-003).

## Traceability and verification matrix
All rows are **Not tested**. Unit means isolated observable contract verification, not internal-call assertions. Persistence/concurrency verification requires a suitable real relational provider, not EF InMemory. The eventual TUnit suite can combine criteria while retaining their IDs.

| Specification | Criterion | Classification | Verification | Observable assertion | Status |
| --- | --- | --- | --- | --- | --- |
| RETRY-001 | RETRY-001-AC-01 | Required behaviour | Unit | Messaging classification | Not tested |
| RETRY-001 | RETRY-001-AC-02 | Required behaviour | Unit | Unknown exception message | Not tested |
| RETRY-002 | RETRY-002-AC-01 | Observed behaviour | Unit | Snapshot truncation | Not tested |
| RETRY-002 | RETRY-002-AC-02 | Observed behaviour | Unit | Failure time and attempt | Not tested |
| RETRY-002 | RETRY-002-AC-03 | Observed behaviour | Unit | Unknown category | Not tested |
| RETRY-002 | RETRY-002-AC-04 | Observed behaviour | Unit | Null exception | Not tested |
| RETRY-003 | RETRY-003-AC-01 | Observed behaviour | Unit | Permanent fault early | Not tested |
| RETRY-003 | RETRY-003-AC-02 | Observed behaviour | Unit | Transient or unknown before budget | Not tested |
| RETRY-003 | RETRY-003-AC-03 | Observed behaviour | Unit | Exhaustion boundary | Not tested |
| RETRY-003 | RETRY-003-AC-04 | Observed behaviour | Unit | Decision not execution | Not tested |
| RETRY-004 | RETRY-004-AC-01 | Observed behaviour | Unit | Mode delays | Not tested |
| RETRY-004 | RETRY-004-AC-02 | Observed behaviour | Unit | Capped delay | Not tested |
| RETRY-004 | RETRY-004-AC-03 | Observed behaviour | Unit | Zero delay | Not tested |
| RETRY-004 | RETRY-004-AC-04 | Observed behaviour | Unit | Full jitter | Not tested |
| RETRY-004 | RETRY-004-AC-05 | Observed behaviour | Unit | Retry options validity | Not tested |
| RETRY-005 | RETRY-005-AC-01 | Required behaviour | Persistence integration | Failure rollback and retry | Not tested |
| RETRY-005 | RETRY-005-AC-02 | Required behaviour | Concurrency integration | Lost ownership | Not tested |
| RETRY-005 | RETRY-005-AC-03 | Required behaviour | Integration | Host cancellation | Not tested |
| RETRY-005 | RETRY-005-AC-04 | Required behaviour | Integration | Settlement timeout | Not tested |

## Implementation trace
These locations are evidence, not mandated internal architecture. The full inspected-file/public-surface matrix is in [INSPECTION.md](INSPECTION.md).

- [src/Clean.Messaging/Exceptions/MessageException.cs](../../src/Clean.Messaging/Exceptions/MessageException.cs)
- [src/Clean.Messaging/Failures/MessageFailure.cs](../../src/Clean.Messaging/Failures/MessageFailure.cs)
- [src/Clean.Messaging/Retry/RetryOptions.cs](../../src/Clean.Messaging/Retry/RetryOptions.cs)
- [src/Clean.Messaging/Retry/RetryOptionsValidator.cs](../../src/Clean.Messaging/Retry/RetryOptionsValidator.cs)
- [src/Clean.Messaging/Retry/RetryPolicy.cs](../../src/Clean.Messaging/Retry/RetryPolicy.cs)

## Open decisions and evidence limits
The [gap register](GAPS.md) records unresolved input domains, provider evidence and proposed changes. Source inspection is not execution proof. [INDEX.md](INDEX.md) explains classification authority and cross-capability ownership.
