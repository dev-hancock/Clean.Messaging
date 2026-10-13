# Repository maintenance

This document tracks checkout observations separately from product architecture and contribution standards. These observations were recorded on 2026-10-13; re-check them before acting and remove or update resolved items. They are not architectural constraints or a provider-support guarantee.

## Solution membership

`Clean.Messaging.sln` refers to absent `Clean.Messaging.Ddd`, `Clean.Messaging.Ddd.EntityFrameworkCore`, and `Clean.Messaging.Outbox.Mqtt` project paths. Current source uses `Clean.Messaging.DomainEvents`, `Clean.Messaging.DomainEvents.EntityFrameworkCore`, and `Clean.Messaging.Outbox.Dispatch.Mqtt`; `Clean.Messaging.Outbox.Dispatch` is also absent from solution membership.

The [architecture package map](reference/architecture.md#package-map) follows source projects rather than stale solution entries. Updating solution membership is separate from this documentation change.

## Validation baseline

No test project, sample host, or repository documentation-validation tooling was present when the initial documentation was reviewed. Friend-assembly declarations mentioning tests do not establish an executable test suite. TUnit is the preferred framework for new .NET tests under the [coding standards](CODING_STANDARDS.md#15-testing-and-validation), not a claim that tests already exist.

The architecture reference is based on source inspection. It does not establish successful end-to-end execution or tested database-provider compatibility. Documentation links and structure can be checked independently of a code build.
