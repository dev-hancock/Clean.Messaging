# OPS-003 — Terminal cleanup boundaries

[Capability contracts and boundaries](README.md) · [Specification index](../INDEX.md)

**Classification:** Observed behaviour
**Evidence:** DurableStore deletes a limited set with any terminal timestamp strictly older than cutoff; inbox admissions are not this generic entry type.

Pre: positive limit and terminal-retention cutoff. Post: bounded old terminal rows disappear; unresolved and boundary-equal rows are retained.

## OPS-003-AC-01 — Strict cutoff
**Given** terminal timestamps are older than, equal to and newer than cutoff.
**When** Cleanup runs.
**Then** only strictly older qualifying terminal rows can be deleted.

## OPS-003-AC-02 — Pending protection
**Given** an unresolved pending/retrying/claimed entry has very old CreatedAt.
**When** Cleanup runs.
**Then** creation age alone does not delete it.

## OPS-003-AC-03 — Bounded deletion
**Given** more eligible terminal entries than limit.
**When** one Cleanup runs.
**Then** the reported deletion count is at most limit; a pass is not promised to drain all eligible rows.

## OPS-003-AC-04 — Discard age
**Given** a recently discarded row has an older dead-letter timestamp before cutoff.
**When** Cleanup runs.
**Then** it may be removed based on that older terminal timestamp rather than restarting retention at discard.

## OPS-003-AC-05 — Evidence removal
**Given** a completed target is cleaned while its inbox admission is retained.
**When** ordinary duplicate Accept and explicit replay are later attempted.
**Then** admission deduplication and missing-target replay differ; cleanup did not imply the admission was deleted.
