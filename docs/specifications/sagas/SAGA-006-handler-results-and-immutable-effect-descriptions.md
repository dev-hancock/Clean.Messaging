# SAGA-006 — Handler results and immutable effect descriptions

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Handler authors need immutable declarative results that distinguish business outcome from delivery failure and describe supported local effects without executing them during construction.

The public restricted SagaResult factories and effect descriptors, immutable-value standards and explicit outcome/cancellation standards establish this contract. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Continue, Complete and valid Fail results MUST preserve their distinct business outcomes.
- Appending supported effects MUST return a new ordered description without mutating the original; invalid IDs/null effect payloads MUST be rejected at supported admission.

## Contract boundaries

The returned result must be retained. Fail diagnostic admission is GAP-WF-008; exact diagnostic clearing/history is separately undecided under GAP-INTENT-WF-003. Arbitrary external effect plugins are not supported. Constructing a result is not saving state or publishing.

## Acceptance criteria

### SAGA-006-AC-02 — Effect append preserves original result

**Given** a result already has an outcome and effects.
**When** another effect is appended.
**Then** a new result retains the same outcome and earlier ordered effects; the original result remains unchanged.

### SAGA-006-AC-03 — Effect overloads describe equivalent operations

**Given** equivalent valid arguments describe a send or timer through primitive and descriptor overloads.
**When** those results are consumed.
**Then** both describe the same logical effect identity, payload and supported metadata/due values.

### SAGA-006-AC-04 — Empty IDs and null effects reject

**Given** supported Send/Schedule/Cancel operations receive empty identities, or Send/Schedule receives null descriptors/payloads.
**When** effects are described.
**Then** the invalid input is rejected; a past due instant alone does not invalidate a valid timer description.

### SAGA-006-AC-05 — Null state factory and null handler result fail

**Given** the configured start factory returns null or the handler returns null instead of a result.
**When** processing evaluates that attempt.
**Then** it fails rather than committing an invented state or default outcome.

### SAGA-006-AC-06 — Throwing handler does not apply a returned result

**Given** a handler throws or observes owner cancellation.
**When** the owner rolls back that failed attempt.
**Then** no returned-result outcome or candidate state/effects from that attempt commits; cancellation retains its separate owner meaning.

## Open decisions

Separate boundary questions remain unapproved in the [decision register](../GAPS.md): `GAP-WF-008`. These do not suspend the independently justified guarantees above.

### SAGA-006-AC-01 — Continue complete and fail transition outcomes

**Reserved — no normative acceptance scenario.** The original composite also selects clearing failure diagnostics on completion. Is the exact retained diagnostic history part of the result contract, independently of the public Continue/Complete/Fail outcome distinction?

Decision: [GAP-INTENT-WF-003](../GAPS.md#gap-intent-wf-003) ([unapproved decision register](../GAPS.md)). Preserve the original identity and heading; a narrower outcome-only criterion requires explicit splitting and a new identity rather than silently reusing this ID.

### SAGA-006-AC-07 — Public cancellation descriptor is not direct effect execution

**Reserved — no normative acceptance scenario.** Is acceptance/formatting of directly constructed empty identity values and empty SagaCancel an intentional compatibility contract?

Decision: [GAP-INTENT-WF-006](../GAPS.md#gap-intent-wf-006) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
