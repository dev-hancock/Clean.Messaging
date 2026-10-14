# Clean.Messaging repository instructions

Read [the coding standards](../docs/CODING_STANDARDS.md) before changing code. Use [the documentation index](../docs/README.md) to find capability references and explanations, and consult [the architecture reference](../docs/reference/architecture.md) for package responsibilities and current limitations.

- This repository is a modular .NET 10 messaging library, not a business application. Consumer applications own domain policy and host composition.
- Keep provider-neutral messaging capabilities separate from EF Core, MQTTnet, and Quartz implementations. Do not introduce application-specific frameworks or domain concepts without a requirement.
- Inspect related APIs, registrations, project references, and end-to-end processing paths before editing. Preserve message/event identity, contract names, per-target completion, leases, retries, and transaction boundaries.
- Make focused, cohesive changes. Reuse existing mechanisms; avoid speculative abstractions, unrelated cleanup, and interface-per-class conventions.
- Preserve cancellation and failure classifications. External publication can repeat; do not claim exactly-once application effects.
- Add relevant tests when changing behaviour. Prefer TUnit for new .NET tests. Use available tooling and report validation accurately; a preferred framework is not evidence of an existing test suite.
- Update directly affected documentation. Use [Diátaxis](https://diataxis.fr/): factual product descriptions belong in `docs/reference/`; rationale and trade-offs belong in `docs/explanation/`. Tutorials and how-to guides are outside the current documentation scope.
- Describe implemented behaviour rather than intended guarantees. Do not silently fix unrelated repository issues while writing documentation.
