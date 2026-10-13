# INBOX-012 — Scoped cleanup composition requirement and current mismatch

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Required behaviour

**Evidence:** Normative: [coding standards](../../CODING_STANDARDS.md) require scoped dependencies not be captured by singleton services. Descriptive counter-evidence: AddInbox registers singleton IHostedService InboxCleanupWorker whose constructor takes scoped IInboxStore. [GAP-DUR-005](../GAPS.md#gap-dur-005); this specification is not a statement that current source conforms.

Purpose/invariant: safe lifetime composition under scope validation. Applies to cleanup and any singleton extension dependencies. Current source can fail host scope validation or retain a root-resolved scoped store when validation is disabled; source review is not a successful-host claim.

## INBOX-012-AC-01 — Resolve cleanup without captive scoped persistence

- **Given** default inbox registrations with service-provider scope validation enabled.
- **When** the host resolves hosted cleanup services.
- **Then** required outcome: cleanup composition resolves without consuming a scoped store from the root; current constructor/registration appears to violate this outcome.
