# Configuration, validation and host composition

[Specification index](../INDEX.md) · [Capability reference](../../reference/configuration.md)

## Purpose

Hosts compose reusable capabilities with their own resources, compatible service overrides and validated operational settings. The catalogue separates registration inputs, startup checks, context selection and unresolved validation extensions.

## Capability boundaries

Core registration, MessagingBuilder and common options do not provision a database, schema, broker or application framework. Feature registration, model mappings/interceptors and resource deployment are separate host responsibilities. Singleton library services cannot retain scoped resources.

## Decisions and limitations

Configuration before use is distinct from live reconfiguration. Public defaults are compatibility values, not universal sizing advice. Cleanup scope capture is an existing requirement mismatch in [GAP-DUR-005](../GAPS.md#gap-dur-005); null-group/enum validation proposals are not approved.

## Specifications

- [CONFIG-001 — Host ownership and resource lifetimes](CONFIG-001-host-ownership-and-resource-lifetimes.md)
- [CONFIG-002 — Public registration defaults and validation](CONFIG-002-core-defaults-and-override-preservation.md)
- [CONFIG-003 — Registration identifier boundaries](CONFIG-003-registration-identifier-boundaries.md)
- [CONFIG-004 — Startup validation and configuration binding](CONFIG-004-startup-validation-and-configuration-binding.md)
- [CONFIG-005 — Persistence-context composition constraints](CONFIG-005-one-messaging-persistence-context-type.md)
- [CONFIG-006 — Complete boundary validation](CONFIG-006-complete-boundary-validation.md)
