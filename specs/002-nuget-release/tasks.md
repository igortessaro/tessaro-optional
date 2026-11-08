# Tasks 002 — NuGet release

**Status:** Approved (2026-11-08) · **Created:** 2026-11-08
**Spec:** [002](spec.md) (Approved 2026-11-08) · **Plan:** [002](plan.md) (Approved 2026-11-08)
**Verification:** [verification.md](verification.md) · **Retrospective:** [retrospective.md](retrospective.md) (2026-11-08)

> Ordered, small, independently verifiable steps that implement plan 002. Each task is **one
> branch and one pull request**. Tasks run in order: each one builds on the merged result of the
> previous one.

**Progress is tracked on GitHub**, not in this file: one issue per task in milestone
[002 — NuGet release](https://github.com/igortessaro/tessaro-optional/milestone/2), labelled `task`, on the [Tessaro.Optional board](https://github.com/users/igortessaro/projects/3).

---

## How each task is done

1. Create the task's branch from an up-to-date `main` (trunk-based; Gitflow prefixes).
2. **Red** — write the tests for the task's ACs first and see them fail. Shell tests call
   `ac "AC-x.y" "<description>"`; ACs that only a real run can show get a row in `verification.md`.
3. **Green** — the simplest change that makes them pass.
4. **Refactor** — ShellCheck clean; workflow inputs reach scripts only through `env:`.
5. Run locally: build, `dotnet format --verify-no-changes`, tests with the coverage gate, and
   `bash tests/scripts/*.test.sh` once that folder exists.
6. Push and open the PR titled as in the task (ADR 0005); the body follows the PR template and
   ends with `Closes #<issue>`.
7. Owner reviews; the owner squash-merges, or tells Claude explicitly to merge.

## Definition of done (every task)

- [ ] Every AC listed in the task has a passing script test or a `verification.md` row with evidence
- [ ] Workflows: actions pinned by SHA, least-privilege `permissions`, no `${{ }}` inside `run:` for event data
- [ ] ShellCheck clean; build has zero warnings; `dotnet format` is clean
- [ ] Deviations from the plan are recorded in `plan.md` in the same PR
- [ ] CI is green; the PR closes its issue

---

## Overview

| ID | Issue | Task | Covers | Branch |
|---|---|---|---|---|
| T00 | [#22](https://github.com/igortessaro/tessaro-optional/pull/22) (PR) | Spec, plan and this task list; ADR 0005, constitution v1.2.0, PR template | — | `feature/002-nuget-release` |
| T01 | #23 | PR title check and script tests in CI | US-5, plan §3.3 | `feature/002-pr-title-check` |
| T02 | #24 | Release guard script | AC-1.2, AC-1.3, AC-3.5 (logic) | `feature/002-release-guard` |
| T03 | #25 | Release workflow, environment and tag ruleset | US-1, US-2, AC-3.3, AC-3.4, AC-3.5 | `feature/002-release-workflow` |
| T04 | #26 | Package page and release routine | AC-3.1, AC-4.2 | `feature/002-release-docs` |
| T05 | #27 | Prepare and publish `0.0.1-rc.1` | AC-4.1; run of every US-1/2/3 AC | `feature/002-release-rc1` |
| T06 | #28 | Verify, publish `0.0.1` and close spec 002 | all ACs (evidence), closing task | `feature/002-final-review` |

T01–T06 touch `ci.yml`, `README.md` or the rulesets in turn, so they run in sequence.

---

### T00 — Spec, plan and tasks

The PR that carries this file, plus the decisions made while writing it: ADR 0005 (PR title and
description standard), constitution v1.2.0 and the new PR template.
PR: `Spec 002 (T00) | Add NuGet release spec, plan and tasks`.

### T01 — PR title check and script tests in CI

- `.github/scripts/check-pr-title.sh` and `tests/scripts/check-pr-title.test.sh` (valid and
  invalid titles from plan §3.3, including the 72-character limit and the `Deps` exception).
- `ci.yml`: job `scripts` — ShellCheck on `.github/scripts/` and `tests/scripts/`, then every test script.
- `pr-title.yml`: job `pr-title`, title passed through `env:`.
- `dependabot.yml`: prefix `Deps |` for both ecosystems (AC-5.3 is verified on the first Dependabot PR).
- After both jobs ran green on this PR: add `scripts` and `pr-title` to the required checks in
  `.github/rulesets/main.json` and apply it (Claude, with the owner's go-ahead).
- Outside the repo: `check-ac-traceability.sh` (sdd skill) accepts several test paths.

Done when an invalid title on a test PR is blocked, and this PR's own title passes.
PR: `Spec 002 (T01) | Add PR title check and script tests`.

### T02 — Release guard script

- `.github/scripts/release-info.sh <tag> <sha>` per plan §3.2, writing `version`, `prerelease`
  and `baseline` in the `$GITHUB_OUTPUT` format.
- `tests/scripts/release-info.test.sh`: temporary git repositories covering malformed tags,
  commits off `main`, prerelease tags, no previous tag, major/minor bumps for `0.y` and `≥ 1`, and
  a patch bump (baseline expected).

Done when every row of plan §3.2 has a passing test.
PR: `Spec 002 (T02) | Add release guard script`.

### T03 — Release workflow, environment and tag ruleset

- `ci.yml`: add `workflow_call`.
- `release.yml`: jobs `guard`, `ci`, `pack`, `publish` per plan §3.1 (no-dependency check, package
  validation with the computed baseline, attestation, NuGet login, push, GitHub Release).
- `.github/rulesets/release-tags.json` and the `release` environment (owner as required reviewer,
  tags `v*` only), applied with `gh api` — **Claude, with the owner's go-ahead**.
- Toolchain check: lint the workflows with `actionlint` (run once locally in the scratchpad), and
  pack exactly as the `pack` job does with a local baseline, as in the plan's spike R2.
- **Owner:** confirm the `NUGET_USER` secret exists.

Done when the workflows lint clean, the ruleset and environment exist (read back with `gh api`),
and nothing can publish without a tag push and an approval.
PR: `Spec 002 (T03) | Add release workflow`.

### T04 — Package page and release routine

- README: relative links → absolute GitHub URLs (plan §3.5).
- `CONTRIBUTING.md` "Releasing": preparation PR (move `PublicAPI.Unshipped.txt` to `Shipped`),
  choosing the version (SemVer `0.y` rules), tag and push, approve the run, verify the release,
  what to do when a run fails after tagging (delete the tag).

Done when every README link resolves from a packed `README.md` opened outside the repository.
PR: `Spec 002 (T04) | Document the release routine`.

### T05 — Prepare and publish `0.0.1-rc.1`

- Release-preparation PR: move every `PublicAPI.Unshipped.txt` entry to `PublicAPI.Shipped.txt` (AC-4.1).
- After the merge, the **owner** runs the routine: tag `v0.0.1-rc.1` on `main`, push, approve.
- Negative checks, recorded for T06: tag `v0.0.0-guard.1` on a branch commit → the run fails in
  `guard`, tag deleted (AC-1.3); one run left unapproved → nothing published (AC-1.6); re-run of
  `publish` after success → fails, version untouched (AC-1.5).

Done when `Tessaro.Optional 0.0.1-rc.1` is on nuget.org as a prerelease.
PR: `Spec 002 (T05) | Ship the public API for 0.0.1-rc.1`.

### T06 — Verify, publish `0.0.1` and close spec 002

- `verification.md`: one row per AC that only a real run can show, with evidence (run URL,
  `gh attestation verify` output, nuget.org page, debugger step-in from a consumer project).
- AC traceability: `check-ac-traceability.sh` over `tests/scripts/` and `verification.md`.
- README "Installation": the package is published; remove the "not on NuGet yet" note.
- After the merge, the **owner** tags and publishes `v0.0.1`.
- Spec 002 → `Implemented (YYYY-MM-DD)`; checklists ticked; `retrospective.md`; lessons applied to
  the `sdd` / `sdd-task` skills.

PR: `Spec 002 (T06) | Verify the release and close spec 002`.

---

## Review checklist

- [x] Every AC in the spec is assigned to a task (run-only ACs: exercised in T05, evidenced in T06)
- [x] Every task is one branch / one PR with a clear "done when"
- [x] Tasks are in an order where each builds on merged work only
- [x] Owner-only steps are called out
- [x] Open questions answered or deferred
