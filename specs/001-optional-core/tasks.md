# Tasks 001 — Optional core

**Status:** Approved (2026-11-07) · **Created:** 2026-11-07
**Spec:** [001](spec.md) (Approved 2026-11-07) · **Plan:** [001](plan.md) (Approved 2026-11-07)
**Retrospective:** [retrospective.md](retrospective.md) (2026-11-08)

> Ordered, small, independently verifiable steps that implement plan 001. Each task is **one
> branch and one pull request**. Tasks run in order: each one builds on the merged result of the
> previous one.

**Progress is tracked on GitHub**, not in this file: one issue per task in milestone
[001 — Optional core](https://github.com/igortessaro/tessaro-optional/milestone/1), labelled `task`, on the [Tessaro.Optional board](https://github.com/users/igortessaro/projects/3).
This file holds the *what* of each task; the issue holds its *state* (one source of truth each).

---

## How each task is done

1. Create the branch named in the task from an up-to-date `main`. The repository is
   **trunk-based**: short-lived branches off `main`, named with the Gitflow prefixes
   (`feature/…`, `hotfix/…`, `release/…`); there is no `develop` branch.
2. **Red** — write the tests for the task's acceptance criteria first, each tagged
   `[Trait("AC", "AC-x.y")]`, and see them fail (or fail to compile).
3. **Green** — write the simplest code that makes them pass.
4. **Refactor** — clean up with the tests green; update XML docs and `PublicAPI.Unshipped.txt`.
5. Run locally: `dotnet build -c Release`, `dotnet test -c Release`, `dotnet format --verify-no-changes`.
6. Push and open the PR with the task's PR title; the PR body contains `Closes #<issue>`.
7. Owner reviews; the owner squash-merges, or tells Claude explicitly to merge.

## Definition of done (every task)

- [ ] All ACs listed in the task have at least one passing test with the matching `Trait`
- [ ] Build has zero warnings (warnings are errors), `dotnet format` is clean
- [ ] Every new public member has XML docs, including `<exception>` tags, and is in `PublicAPI.Unshipped.txt`
- [ ] No public member exists that the spec does not require
- [ ] CI is green (from T02 onwards); coverage ≥ 80 % line and branch
- [ ] The PR closes the task's issue (`Closes #N`)

---

## Overview

| ID | Issue | Task | Covers | Branch |
|---|---|---|---|---|
| T00 | [#1](https://github.com/igortessaro/tessaro-optional/pull/1) (PR) | This task list | — | `docs/001-tasks` (pre-Gitflow) |
| T01 | #2 | Solution scaffolding | plan §1, §2, §4 | `chore/001-scaffolding` (pre-Gitflow) |
| T02 | #3 | CI pipeline, SonarCloud and required checks | plan §6, §7 | `ci/001-pipeline` (pre-Gitflow) |
| T03 | #4 | Creation and state | US-1, US-2, R1 | `feat/001-creation-and-state` (pre-Gitflow) |
| T04 | #5 | Equality and text | US-7 | `feature/001-equality` |
| T05 | #6 | Transformations | US-3 | `feature/001-transformations` |
| T06 | #7 | Extraction with fallback | US-4 | `feature/001-extraction` |
| T07 | #8 | Side effects | US-5 | `feature/001-side-effects` |
| T08 | #9 | .NET idioms and LINQ | US-6 | `feature/001-idioms-and-linq` |
| T09 | #10 | Allocation guarantees | NFR-2 | `feature/001-allocations` |
| T10 | #11 | Documentation and final review | README, CLAUDE.md, spec checklist | `feature/001-final-review` |

T01 → T02 → T03 → T04 → T05 → T06 → T07 → T08 → T09 → T10. T05–T07 don't depend on each other
logically, but they all edit `Optional{T}.cs`, so they run one after another to avoid conflicts.

---

## Tasks

### T00 — Task list

Approve this file. PR: `docs: add tasks for 001-optional-core`.

### T01 — Solution scaffolding

**Goal:** an empty but fully configured solution that builds with every quality setting on.

- `global.json` pinning the .NET 10 SDK (`rollForward: latestFeature`)
- `Tessaro.Optional.slnx` with `src/Tessaro.Optional` (`net8.0;net10.0`) and
  `tests/Tessaro.Optional.Tests` (xUnit v3, Shouldly, `coverlet.msbuild`)
- `Directory.Build.props`, `Directory.Packages.props` (Central Package Management), `.editorconfig` — plan §4
- Package metadata, SourceLink/symbols, MinVer, PublicApiAnalyzers with empty
  `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`
- Fill the **Commands** section of `CLAUDE.md` (build, test, coverage, format)

**Done when:** `dotnet build -c Release` and `dotnet format --verify-no-changes` pass locally;
`dotnet pack` produces a package with **no dependencies** listed in its `.nuspec`.

PR: `chore: scaffold solution and build configuration`.

### T02 — CI pipeline, SonarCloud and required checks

**Goal:** every later PR is checked automatically.

- `dotnet-tools.json` (repo root, SDK 10 default) with `dotnet-sonarscanner`
- `.github/workflows/ci.yml` — jobs `format` and `build-test` exactly as plan §6, coverage
  threshold 80 %, Sonar steps skipped when `SONAR_TOKEN` is absent, actions pinned by SHA
- `.github/dependabot.yml` (nuget + github-actions, weekly), `.github/CODEOWNERS` (`* @igortessaro`),
  `.github/pull_request_template.md` (task ID, ACs covered, definition-of-done checklist)

**Owner steps (plan §7, steps 6–7):** create the SonarCloud organization and project, turn off
Automatic Analysis, run `gh secret set SONAR_TOKEN`. Claude gives the exact project key to use.

**After the PR's checks are green once:** add `format`, `build-test` and
`SonarCloud Code Analysis` as required status checks in `.github/rulesets/main.json` and apply it
with `gh api` (same PR).

**Done when:** the PR shows all three checks green, and the ruleset lists them as required.

**Note (confirmed in T01):** with zero tests, Microsoft.Testing.Platform exits with code 8, so the
`build-test` job cannot be green until T03 adds tests. Decide at the start of T02 between
(a) `--ignore-exit-code 8` until T03 removes it, or (b) running T03 before T02.

PR: `ci: add CI pipeline with format, tests, coverage and SonarCloud`.

### T03 — Creation and state

**ACs:** AC-1.1 – AC-1.7, AC-2.1, AC-2.2 · **Plan:** §3.1, §3.2 (state), §3.3

- `Optional.cs` (factory), `Optional{T}.cs` (struct with `_value`, `_isPresent`, `IsPresent`, `IsEmpty`),
  `OptionalExtensions.cs` (`ToOptional`)
- Tests: `CreationTests.cs`, `StateTests.cs`, `OverloadResolutionTests.cs` (risk R1 regression)
- AC-1.2's *"equal to `Optional.Empty<T>()`"* part is asserted in T04, once equality exists;
  here it is asserted through `IsEmpty`.

PR: `feat: add Optional<T> creation and state`.

### T04 — Equality and text

**ACs:** AC-7.1 – AC-7.5, and the equality part of AC-1.2 · **Plan:** §3.2 (equality), §3.4

- `Optional{T}.Equality.cs`: `IEquatable<Optional<T>>`, `Equals(object?)`, `GetHashCode`, `==`, `!=`, `ToString`
- Tests: `EqualityTests.cs`, with `string` and `int`

PR: `feat: add equality and ToString to Optional<T>`.

### T05 — Transformations

**ACs:** AC-3.1 – AC-3.9 · **Plan:** §3.2 (transform), §3.4

- `Map`, `FlatMap`, `Filter`
- Tests: `TransformationTests.cs`; "not invoked" cases use a delegate that fails the test if called

PR: `feat: add Map, FlatMap and Filter`.

### T06 — Extraction with fallback

**ACs:** AC-4.1 – AC-4.12 · **Plan:** §3.2 (extract), §3.4

- `OrElse` (with `[NotNullIfNotNull]`), `OrElseGet`, `OrElseThrow()`, `OrElseThrow<TException>`, `Or`
- Includes: factory returning `null` throws `InvalidOperationException` (plan §3.4)
- Tests: `ExtractionTests.cs`

PR: `feat: add OrElse, OrElseGet, OrElseThrow and Or`.

### T07 — Side effects

**ACs:** AC-5.1 – AC-5.5 · **Plan:** §3.2 (side effects)

- `IfPresent`, `IfPresentOrElse`
- Tests: `SideEffectTests.cs` (invocation counts)

PR: `feat: add IfPresent and IfPresentOrElse`.

### T08 — .NET idioms and LINQ

**ACs:** AC-6.1 – AC-6.7 · **Plan:** §3.2 (idioms, LINQ aliases)

- `Match`, `TryGetValue`, `AsEnumerable` in `Optional{T}.cs`
- `Optional{T}.Linq.cs`: `Select`, `Where`, both `SelectMany` overloads — each a one-line delegation
- Tests: `IdiomTests.cs`, including a LINQ query-syntax test (`from … from … where … select …`)

PR: `feat: add Match, TryGetValue, AsEnumerable and LINQ support`.

### T09 — Allocation guarantees

**NFR:** NFR-2 · **Plan:** §5, risk R3

- `AllocationTests.cs`: warm up, then assert `GC.GetAllocatedBytesForCurrentThread()` grows by
  `0` across create → `Map` → `Filter` → `FlatMap` → `OrElse` / `OrElseGet` / `Match` / `TryGetValue`
  with `static` lambdas, for `string` and `int`
- If a member allocates, fix the implementation — don't relax the test

PR: `test: verify zero-allocation guarantees`.

### T10 — Documentation and final review

- README: installation, usage examples per user story, Java ↔ .NET mapping table, link to deviations
- Verify every AC in the spec has a test (search the `Trait` values) and record the result in the PR
- Tick spec §9 and plan §12 checklists; set spec 001 status to `Implemented (YYYY-MM-DD)`
- Retrospective notes for the `/sdd` skill (what to change in the tasks phase)

PR: `docs: document Optional<T> usage and close spec 001`.

---

## Resolved questions

Decided by the owner on 2026-11-07.

1. **AC traceability check in CI** — not now (YAGNI); checked manually in T10.
2. **Who opens and merges PRs** — Claude opens them with `gh`; the owner merges, or tells Claude
   explicitly to merge a specific PR.
3. **Task size** — kept as is.
4. **Progress tracking** — one GitHub issue per task in milestone `001 — Optional core`, shown on
   a GitHub Project board; PRs close their issue with `Closes #N`.

## Review checklist

- [x] Every AC in the spec is assigned to exactly one task
- [x] Every task is one branch / one PR with a clear "done when"
- [x] Tasks are in an order where each builds on merged work only
- [x] Owner-only steps are called out
- [x] Open questions answered or deferred
