# SAGA-004 — Identity values and durable correlation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour

**Contracts and applicability:** SagaId.Value/New/D-format text; SagaKey.Value/From/preserved text. Ordinary correlation uses the configured function, not message CorrelationId. Uniqueness is persisted (Type,Key), including terminal records.

**Evidence:** SagaId.cs; SagaKey.cs; SagaProcessor.cs; SagaRuntime.cs; SagaConfiguration.cs.

**Intent gaps:** [GAP-WF-001](../GAPS.md#gap-wf-001), [GAP-WF-007](../GAPS.md#gap-wf-007), [GAP-WF-011](../GAPS.md#gap-wf-011).

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## SAGA-004-AC-01 — Key boundaries preserve Unicode and exact text

**Given** nonblank Unicode/text keys at 500 characters, blank/null and 501 characters.
**When** SagaKey constructor or From is used.
**Then** 500 characters are accepted unchanged including Unicode; blank/null and 501 reject; no trim/case-normalisation is implied.

## SAGA-004-AC-02 — Instance identity formatting

**Given** new and directly supplied SagaId values including Guid.Empty.
**When** Value and ToString are read.
**Then** Value is preserved and text uses GUID D format; New supplies a nonempty GUID, not a correlation key.

## SAGA-004-AC-03 — Correlation function chooses workflow

**Given** two messages share metadata CorrelationId but correlate to different keys or conversely.
**When** ordinary durable saga handling occurs.
**Then** the definition-specific key controls instance lookup; metadata correlation does not merge or split instances by itself.

## SAGA-004-AC-04 — Ordinary message needs durable trigger identity

**Given** no message MessageId is in the execution context.
**When** ordinary saga consumer handling is attempted.
**Then** handling rejects the absent durable trigger identity instead of inventing a start identity.

## SAGA-004-AC-05 — Terminal key still occupies durable identity

**Given** an active or terminal row has a type/key.
**When** another start tries the same type/key.
**Then** the retained unique identity prevents a second committed row, even if payload state was purged.
