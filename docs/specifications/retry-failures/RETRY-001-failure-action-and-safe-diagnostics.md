# RETRY-001 — Failure action and safe diagnostics

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour
**Evidence:** Coding standards §7 requires preserving code/transient/permanent distinctions and §13/§17 prohibits unnecessary sensitive diagnostics.

Pre: a messaging exception conveys Retry/Fault. Post: classification preserves action/code; unknown exceptions must not be exposed as fabricated success or persisted sensitive payload text.

## RETRY-001-AC-01 — Messaging classification
**Given** MessageException conveys Retry or Fault with a code.
**When** its failure snapshot is created.
**Then** Retry maps to Transient and Fault to Permanent while the code is preserved within the bounded diagnostic field.

## RETRY-001-AC-02 — Unknown exception message
**Given** an arbitrary exception includes sensitive business payload text in its Message.
**When** the standard failure snapshot is created.
**Then** stored Message is the generic unexpected-processing description rather than the arbitrary exception Message.
