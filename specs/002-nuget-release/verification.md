# Verification 002 — NuGet release

**Date:** 2026-11-08 · **Spec:** [002](spec.md) · **First release:** `0.0.1-rc.1`, published by
[run 42510664641](https://github.com/igortessaro/tessaro-optional/actions/runs/42510664641) from commit `e3dd6c6`

Evidence for every acceptance criterion. Logic covered by script tests is listed with the test file;
behaviour that only a real run can show is listed with the run, command or page that shows it.
The AC traceability check reads this table together with `tests/scripts/`.

| AC | How | Evidence |
|---|---|---|
| AC-1.1 | Real run | `publish` pushed `Tessaro.Optional.0.0.1-rc.1.nupkg` ("Your package was pushed.") and the `.snupkg` to the symbol server; a fresh consumer (`net10.0`, empty package cache) ran `dotnet add package Tessaro.Optional --version 0.0.1-rc.1` and printed the expected output |
| AC-1.2 | Script tests + real run | `release-info.test.sh` (2 cases); the run's guard output was `prerelease=true`, and nuget.org lists `0.0.1-rc.1` as a prerelease |
| AC-1.3 | Script tests + real run | `release-info.test.sh` (3 cases); tag `v0.0.0-guard.1` on `d9dddcc` (a branch commit) → [run 42509304320](https://github.com/igortessaro/tessaro-optional/actions/runs/42509304320): `guard` failed with "the tagged commit is not on main", `ci`/`pack`/`publish` skipped, nothing on nuget.org; tag deleted |
| AC-1.4 | Workflow structure + real runs | `publish` needs `pack`, which needs `guard` and `ci` (the full PR gates); run 42509304320 shows that a failing job skips every later job; in run 42510664641 `guard`, `ci / format`, `ci / scripts`, `ci / build-test` and `pack` passed before `publish` could start; a breaking change made `pack` fail locally (T03) |
| AC-1.5 | Real run | Re-run of `publish` (attempt 2 of run 42510664641) → push failed with `409 (A package with ID 'Tessaro.Optional' and version '0.0.1-rc.1' already exists and cannot be modified.)`; the GitHub Release step was skipped and nothing changed |
| AC-1.6 | Real run | At 18:39 UTC `publish` was `waiting` on the `release` environment (reviewer `igortessaro`), and nuget.org, the package page and the GitHub Release all returned "not found"; publishing happened only after the owner approved |
| AC-1.7 | Ruleset + real pushes | Ruleset `release-tags` (id 26806513) restricts creation, update and deletion of `refs/tags/v*` with only the admin bypass; the owner's pushes reported "Bypassed rule violations … Cannot create ref due to creations being restricted" and "… Cannot delete this tag", so anyone without the bypass is rejected |
| AC-2.1 | Real run | [GitHub Release v0.0.1-rc.1](https://github.com/igortessaro/tessaro-optional/releases/tag/v0.0.1-rc.1) with notes generated from PRs #1–#33 |
| AC-2.2 | Real run | Release assets: `Tessaro.Optional.0.0.1-rc.1.nupkg`, `Tessaro.Optional.0.0.1-rc.1.snupkg` |
| AC-2.3 | Real run | `isPrerelease=true`; `GET /releases/latest` returns 404 (no release is marked latest) |
| AC-3.1 | nuget.org page | [Tessaro.Optional 0.0.1-rc.1](https://www.nuget.org/packages/Tessaro.Optional/0.0.1-rc.1) renders the README (including the Java ↔ .NET table and the CI badge), the MIT licence and the GitHub links |
| AC-3.2 | Symbols package | `net8.0` and `net10.0` PDBs map sources to `https://raw.githubusercontent.com/igortessaro/tessaro-optional/e3dd6c6…/*`, the tagged commit; the `.snupkg` was pushed to nuget.org's symbol server. Stepping into the code in an IDE debugger was not exercised |
| AC-3.3 | Workflow check + package | `pack` fails on any `<dependency>` (proved with a crafted package in T03); the released nuspec has none |
| AC-3.4 | `gh attestation verify` | Exit 0 for the released `.nupkg` (sha256 `4fdff454…6933a`): signer `.github/workflows/release.yml@refs/tags/v0.0.1-rc.1`, source digest `e3dd6c6…`, trigger `push` ([attestation 65101117](https://github.com/igortessaro/tessaro-optional/attestations/65101117)) |
| AC-3.5 | Script tests + local pack | `release-info.test.sh` (10 cases) for the baseline rules; in T03, `v0.0.2` with `IsEmpty` made internal failed `pack` with "API breaking changes found" against a `0.0.1` baseline, and passed as `v0.1.0` |
| AC-4.1 | Pull request | [#33](https://github.com/igortessaro/tessaro-optional/pull/33) moved the public API to `PublicAPI.Shipped.txt` before `v0.0.1-rc.1` was tagged |
| AC-4.2 | Documentation | [CONTRIBUTING.md — Releasing](https://github.com/igortessaro/tessaro-optional/blob/main/CONTRIBUTING.md#releasing) |
| AC-5.1 | Script tests + real PRs | `check-pr-title.test.sh` (6 cases); `pr-title` passed on PRs #29–#33 |
| AC-5.2 | Script tests + real PR | `check-pr-title.test.sh` (13 cases); PR #29 renamed to `docs: invalid title to prove the check blocks merges` → `pr-title` failed with the format and an example ([run 42506986361](https://github.com/igortessaro/tessaro-optional/actions/runs/42506986361)) and the PR was `BLOCKED` |
| AC-5.3 | **Pending** | `dependabot.yml` uses the prefix `Deps \|`; no Dependabot PR has been opened yet. Tracked in [#34](https://github.com/igortessaro/tessaro-optional/issues/34) |
