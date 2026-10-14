# SAGA-004 — Identity values and durable correlation

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

## Intent

Workflow authors need a durable type/key identity independent of transport metadata and instance IDs so replay and concurrency cannot merge unrelated workflows.

Public SagaKey/value descriptions and the identity/correlation standards establish text preservation and stable retained logical identity; persistent equality remains a separate provider decision. See the [coding standards](../../CODING_STANDARDS.md) and [architecture boundary](../../reference/architecture.md).

## Guarantees

- Valid SagaKey values MUST preserve exact nonblank text, including Unicode, within the supported 500-character bound.
- Ordinary routing MUST use configured key correlation and a durable trigger MessageId; retained type/key identity MUST remain unique even after terminal payload purge.

## Contract boundaries

CorrelationId is metadata, not the saga key. Default struct admission and persisted collation are unresolved. Retained uniqueness is conditional on the identity record existing, not a promise of indefinite storage.

## Acceptance criteria

### SAGA-004-AC-01 — Key boundaries preserve Unicode and exact text

**Given** null/blank or 501-character text and valid nonblank Unicode text up to 500 characters.
**When** SagaKey is constructed directly or through From.
**Then** invalid text is rejected and valid text is preserved without trimming or case normalization.

### SAGA-004-AC-03 — Correlation function chooses workflow

**Given** message metadata correlation and definition keys do not correspond one-to-one.
**When** ordinary saga routing occurs.
**Then** the definition’s correlation key chooses the workflow; metadata alone neither merges nor splits instance identities.

### SAGA-004-AC-04 — Ordinary message needs durable trigger identity

**Given** ordinary processing has no durable trigger MessageId in its execution context.
**When** saga handling is attempted.
**Then** the absent identity is rejected rather than inventing a replacement start identity.

### SAGA-004-AC-05 — Terminal key still occupies durable identity

**Given** a type/key identity is retained in an active, terminal or payload-purged record.
**When** a competing start seeks the same retained identity.
**Then** a second workflow row cannot commit for that identity; purge of payload alone does not free it.

## Open decisions

Separate boundary questions remain unapproved in the [decision register](../GAPS.md): `GAP-WF-001`, `GAP-WF-007`, `GAP-WF-011`. These do not suspend the independently justified guarantees above.

### SAGA-004-AC-02 — Instance identity formatting

**Reserved — no normative acceptance scenario.** Is acceptance/formatting of directly constructed empty identity values and empty SagaCancel an intentional compatibility contract?

Decision: [GAP-INTENT-WF-006](../GAPS.md#gap-intent-wf-006) ([unapproved decision register](../GAPS.md)). The original identity and heading remain reserved; approval of a different or narrower outcome requires explicit retirement/splitting and new criterion identities, not reuse of this ID.
