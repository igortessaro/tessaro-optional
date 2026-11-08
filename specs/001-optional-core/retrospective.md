# Retrospective 001 — Optional core

**Date:** 2026-11-08 · **Spec:** [001](spec.md) (Implemented 2026-11-08) · **Tasks:** T00–T10, one PR each

## Outcome

- All 47 acceptance criteria have at least one test tagged with their ID (checked in T10 by
  comparing the spec's AC IDs with the `Trait("AC", …)` values: none missing, none unknown).
- NFR-2 (no allocation) is guarded by 10 tests, each proved able to fail with a deliberate mutant.
- 202 tests (101 per target framework), 100 % line and branch coverage; CI enforces ≥ 80 %.

## What went well

- **Written decisions paid off.** ADRs and dated plan amendments meant every surprise ended up
  recorded where the next task would read it, instead of only in a PR conversation.
- **Checks from the outside caught what unit tests could not:** a consumer project found the
  namespace clash (ADR 0004) and the toolchain run found that xUnit v3 no longer supports VSTest
  on the .NET 10 SDK.
- **Small tasks, one PR each,** kept every review short and CI-gated.

## What we would change

| Problem | When it showed up | Change for next time |
|---|---|---|
| The test platform in the plan (VSTest + `coverlet.msbuild`) did not work with the pinned SDK | T01 | The plan phase runs a **toolchain spike** (create → build → test → coverage → pack with the exact SDK) before approval |
| The type name `Optional` clashed with the `Tessaro.Optional` namespace | T03 | The plan phase runs a **consumer spike** of the public names from an unrelated namespace and from a namespace sharing the root |
| A test project with no tests exits with code 8 | T01–T03 | The task list says how scaffolding tasks deal with "no tests yet" |
| Branch naming, trunk-based workflow and commit attribution were corrected mid-way | T02–T04 | The constitution phase asks for **repository conventions** (branching model, branch names, attribution) up front |
| The LINQ allocation was a language cost, not a library one | T09 | Already in `sdd-task`: find the cause before fixing a failing guard; never loosen the assertion |
| AC traceability was checked by hand | T10 | Acceptable for one spec (tasks.md resolved question 1); script it if a second spec repeats it |

These changes were applied to the `sdd` and `sdd-task` skills used to run this workflow.
