# SAGA-015 — Undecided identity, schema and diagnostic hardening

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Proposed behaviour

**Contracts and applicability:** Proposals require approval, not compatibility obligations: revalidate default SagaKey; define persisted equality; validate Fail consistently; constrain collision translation; define unsaved repeated-start handling and retained schema migration policy.

**Evidence:** GAP-WF-001, GAP-WF-007, GAP-WF-008, GAP-WF-009, GAP-WF-012, GAP-WF-015; SagaKey, SagaResult, SagaConflictResolver, SagaStore and state serializer.

**Preconditions/postconditions/invariants:** common sections apply; each scenario narrows the input boundary and observable committed result. Failure, concurrency and limits are scoped to the criteria below.

## Question SAGA-015-Q-01 ? — Default key admission policy

**Scenario:** correlation returns default(SagaKey) bypassing constructor validation. admission hardening is approved.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-001 approves revalidation, handling rejects the invalid key before durable creation; current code does not promise this rejection.

## Question SAGA-015-Q-02 ? — Persisted equality policy

**Scenario:** keys/types differ only by case or provider collation treatment. a supported equality policy is chosen.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-007 approves ordinal persisted semantics, those identities remain distinct consistently across providers; current collation is provider-owned.

## Question SAGA-015-Q-03 ? — Fail diagnostic consistency policy

**Scenario:** Fail receives invalid diagnostic values. GAP-WF-008 chooses validation parity.
**Policy question:** Should the following conditional proposal be adopted: only under that decision, malformed Fail rejects before applying state/effects using agreed 200/500 bounds; this is not current validated behaviour.

## Question SAGA-015-Q-04 ? — Narrow collision translation policy

**Scenario:** unrelated DbUpdateException has one added start and a coincidental persisted winner. translation hardening is selected.
**Policy question:** Should the following conditional proposal be adopted: only if GAP-WF-009 restricts translation to proven identity collisions, the unrelated original error remains; current matching is not unique-error-code-specific.

## Question SAGA-015-Q-05 ? — Unsaved repeated-start policy

**Scenario:** two starts for one type/key are handled in one scope before save. GAP-WF-012 defines tracked-new-instance semantics.
**Policy question:** Should the following conditional proposal be adopted: only under the selected policy, the second operation either observes tracked start replay/conflict or deliberately fails the unit of work; current persisted query does not explicitly merge added rows.

## Question SAGA-015-Q-06 ? — State evolution policy

**Scenario:** retained state/message schemas differ from a new deployment. GAP-WF-015 selects compatibility or migration approach.
**Policy question:** Should the following conditional proposal be adopted: only under the selected policy, compatible work remains interpretable or an explicit migration/rejection occurs; numeric Version is not an automatic schema migration mechanism.
