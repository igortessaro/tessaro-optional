# Plan 002 — NuGet release

**Status:** Draft — awaiting owner review · **Created:** 2026-11-08
**Spec:** [002 — NuGet release](spec.md) (Approved 2026-11-08) · **Constitution:** [v1.2.0](../../docs/constitution.md)

> This plan defines **how** spec 002 is built: workflows, scripts, repository settings and the
> owner-only setup. The ordered implementation steps belong in `tasks.md`.

---

## 1. Technical context

| Item | Choice | Reason |
|---|---|---|
| CI/CD | GitHub Actions, GitHub-hosted `ubuntu-latest` runners | Free for public repositories (spec §5) |
| Trigger | `push` of tags matching `v*` | The tag is the release act and the version source (spec §5) |
| Version | MinVer, already in the project (tag prefix `v`) | No change; spiked 2026-11-08 (R1) |
| Credentials | nuget.org Trusted Publishing via the `NuGet/login` action (OIDC → one-hour API key) | No stored API key (spec §5) |
| Approval | GitHub environment `release` with the owner as required reviewer | AC-1.6; free on public repositories |
| Breaking-change check | SDK package validation (`EnablePackageValidation` + `PackageValidationBaselineVersion`) | Built into the .NET SDK, no extra tool; spiked 2026-11-08 (R2) |
| Provenance | `actions/attest-build-provenance` | AC-3.4; free for public repositories |
| GitHub Release | `gh release create --generate-notes` (preinstalled on runners) | AC-2.x; no extra action |
| Scripts | Bash, linted with ShellCheck (preinstalled on `ubuntu-latest`) | Logic that can be unit-tested lives in scripts, not inline YAML |

Action versions: latest stable at implementation time, **pinned by SHA**, kept current by Dependabot.

## 2. Repository layout

```
.github/
├── workflows/
│   ├── ci.yml                    # CHANGED: also `on: workflow_call` (reused by release.yml); new `scripts` job
│   ├── release.yml               # NEW: guard → ci → pack → publish (US-1, US-2, US-3)
│   └── pr-title.yml              # NEW: title check on pull requests (US-5)
├── scripts/
│   ├── release-info.sh           # NEW: tag → version, prerelease flag, baseline; on-main guard (AC-1.2, 1.3, 3.5)
│   └── check-pr-title.sh         # NEW: ADR 0005 title rules (AC-5.1, 5.2)
├── rulesets/
│   ├── main.json                 # CHANGED: required checks + `scripts`, `pr-title`
│   └── release-tags.json         # NEW: only the owner creates/updates/deletes `v*` tags (AC-1.7)
└── dependabot.yml                # CHANGED: prefix `Deps |` for both ecosystems (AC-5.3)
tests/scripts/
├── release-info.test.sh          # NEW: AC-1.2, AC-1.3, AC-3.5 (temporary git repos)
└── check-pr-title.test.sh        # NEW: AC-5.1, AC-5.2 (table of titles)
CONTRIBUTING.md                   # NEW: "Releasing" routine (AC-4.1, AC-4.2)
README.md                         # CHANGED: absolute links, so they work on nuget.org (AC-3.1)
src/Tessaro.Optional/PublicAPI.*  # CHANGED in the release-preparation PR only (AC-4.1)
specs/002-nuget-release/verification.md  # NEW: evidence for the ACs only a real run can show
```

## 3. Design

### 3.1 Release workflow (`release.yml`)

```
push tag v*  ──►  guard  ──►  ci (reuses ci.yml)  ──►  pack  ──►  publish [environment: release]
                  │            format, build,          │           attest → NuGet login → push → GitHub Release
                  │            tests, coverage         │
                  release-info.sh                      package validation, "no dependencies" check,
                  (tag format, on main, baseline)      upload .nupkg/.snupkg as a run artifact
```

| Job | Permissions | Does | ACs |
|---|---|---|---|
| `guard` | `contents: read` | Checkout with full history; `release-info.sh "$GITHUB_REF_NAME" "$GITHUB_SHA"`; exposes `version`, `prerelease`, `baseline` as outputs | 1.2, 1.3, 3.5 |
| `ci` | inherited `contents: read` | `uses: ./.github/workflows/ci.yml` **without** `secrets`, so the SonarCloud steps skip themselves (they already require `SONAR_TOKEN`) | 1.4 |
| `pack` | `contents: read` | `dotnet pack -c Release -p:EnablePackageValidation=true` plus `-p:PackageValidationBaselineVersion=<baseline>` when set; fails if the `.nuspec` lists any `<dependency>`; uploads `artifacts/*.nupkg`, `*.snupkg` | 1.4, 3.3, 3.5 |
| `publish` | `id-token: write`, `contents: write`, `attestations: write` | `environment: release` (waits for approval); downloads the artifact; attests the `.nupkg`; `NuGet/login`; `dotnet nuget push *.nupkg` **without** `--skip-duplicate`; `gh release create "$TAG" --verify-tag --generate-notes` with `--prerelease --latest=false` for prereleases, attaching both files | 1.1, 1.2, 1.5, 1.6, 2.1–2.3, 3.4 |

- `concurrency: release` with `cancel-in-progress: false` — two releases never interleave, and none is cancelled midway.
- The package is built only in `pack`, from the tagged commit, after `ci` passed in the same run (NFR-3).
  Moving it to `publish` through a run artifact is part of the same run.
- Order inside `publish`: attest → push → GitHub Release. If the push fails (duplicate, AC-1.5), no
  GitHub Release is created. If only `gh release create` fails, the owner creates it by hand (spec §8.4).
- Workflow inputs from the event (tag name, PR title) reach scripts only through `env:`, never
  `${{ }}` inside `run:` — this prevents script injection.

### 3.2 `release-info.sh <tag> <sha>`

| Case | Behaviour | AC |
|---|---|---|
| Tag is not `v<SemVer 2.0>` (e.g. `v1.2`, `release-1`) | exit 1: "tag must look like v1.2.3 or v1.2.3-rc.1" | fail fast |
| `<sha>` is not an ancestor of `origin/main` | exit 1: "the tagged commit is not on main" | 1.3 |
| Tag has a pre-release part (`-rc.1`) | `prerelease=true` | 1.2 |
| No earlier `v*` tag reachable from `<sha>` | `baseline=` (empty: nothing to compare with) | 3.5 |
| Previous tag `vP`; new major > P major, or both majors are 0 and new minor > P minor | `baseline=` (breaking change allowed) | 3.5 |
| Otherwise | `baseline=<P without the v>` | 3.5 |

"Previous tag" is the closest `v*` tag reachable from the parent commit
(`git describe --tags --abbrev=0 --match 'v*' <sha>^`). Every pushed `v*` tag is assumed to be
published; if a run failed after tagging, the owner deletes that tag (CONTRIBUTING.md).

### 3.3 PR title check (`pr-title.yml` + `check-pr-title.sh`)

- Runs on `pull_request` (`opened`, `edited`, `reopened`, `synchronize`); job name `pr-title`;
  permissions `{}` (it reads only the event payload).
- Rules (ADR 0005), as Bash regular expressions:
  - `^Deps \| [^[:space:]].*$` — Dependabot titles: prefix only.
  - otherwise `^(Spec [0-9]{3}( \(T[0-9]{2}\))?|Hotfix|Chore) \| [A-Z].*[^.[:space:]]$` and at most 72 characters.
- On failure it prints the expected format and an example (AC-5.2). The check is required on `main`.

### 3.4 Repository settings as code

| Setting | Source | Applied with |
|---|---|---|
| Required checks on `main`: `format`, `build-test`, `SonarCloud Code Analysis`, **`scripts`**, **`pr-title`** | `.github/rulesets/main.json` | `gh api -X PUT repos/{owner}/{repo}/rulesets/26773917 --input …` |
| Tag ruleset: `refs/tags/v*`, rules `creation`, `update`, `deletion`; bypass: repository admin, mode `always` | `.github/rulesets/release-tags.json` | `gh api -X POST repos/{owner}/{repo}/rulesets --input …` |
| Environment `release`: required reviewer = owner; deployment policy = tags `v*` only | documented commands in CONTRIBUTING.md | `gh api -X PUT repos/{owner}/{repo}/environments/release …` |

The required `scripts` and `pr-title` checks are added to the ruleset only **after** they ran
green once on a PR (the chicken-and-egg lesson from plan 001).

### 3.5 Behaviour details not stated in the spec

| Case | Behaviour | Reason |
|---|---|---|
| `dotnet pack` writes the `.nupkg` before package validation fails | the job fails and the artifact is never uploaded | Spiked (R2): the file exists on disk even on failure, so upload is a later step that runs only on success |
| README relative links (`docs/adr/…`) | rewritten as absolute `https://github.com/igortessaro/tessaro-optional/blob/main/…` URLs | nuget.org does not resolve relative links (AC-3.1) |
| Release notes content | GitHub's default generated notes (PR titles since the previous tag) | ADR 0005 makes titles readable; categories by label are deferred |
| Moving `PublicAPI.Unshipped.txt` to `Shipped` | done in the release-preparation PR, including for prereleases | A prerelease on nuget.org is public, so its API counts as shipped |

## 4. Build configuration

No change to `Directory.Build.props`. Package validation is switched on only by the release
workflow (`-p:` properties), so ordinary builds and PRs need no baseline package and no network
access to nuget.org. Package metadata (licence, README, URLs, SourceLink, `snupkg`) is already in
`Tessaro.Optional.csproj` and was confirmed in the spike (R1).

## 5. Test strategy

- **Scripts are tested first** (`tests/scripts/*.test.sh`): plain Bash, no framework (YAGNI); each
  case calls a helper `ac "AC-x.y" "<description>"` so the AC traceability script can find it with
  `--marker '^ac "AC-'`. `release-info.test.sh` builds temporary git repositories with tags and a
  fake `origin/main`.
- **ACs that only a real run can show** (publish, approval, attestation, nuget.org page, GitHub
  Release) are verified on the `v0.0.1-rc.1` release and recorded with evidence (run URL, command
  output) in `verification.md`, one table row per AC. The traceability check runs over both
  `tests/scripts/` and `verification.md` (the script gains support for several test paths).
- **Negative paths verified on purpose** during the rc.1 release: a tag on a non-`main` commit
  (AC-1.3), re-running `publish` after success (AC-1.5), leaving a run unapproved (AC-1.6).
- The CI job `scripts` runs ShellCheck on `.github/scripts/` and `tests/scripts/`, then every
  `tests/scripts/*.test.sh`.

## 6. CI pipeline changes

- `ci.yml`: add `workflow_call` to `on:`; add the `scripts` job (ShellCheck + script tests).
  Nothing else changes for PRs and pushes to `main`.
- `pr-title.yml`: new, see §3.3.
- `release.yml`: new, see §3.1.
- `dependabot.yml`: `commit-message.prefix: "Deps |"` for `nuget` and `github-actions`. Dependabot
  adds a colon only after a prefix ending in a letter, digit or bracket; whether it adds a space
  after `|` is confirmed on the first Dependabot PR (R4).

## 7. One-time setup

| # | Step | Who |
|---|---|---|
| 1 | nuget.org → *Trusted Publishing* → policy `Tessaro.Optional`: package owner = the owner's nuget.org profile, repository owner `igortessaro`, repository `tessaro-optional`, workflow `release.yml`, environment `release`, scope "Push new packages and package versions", package `Tessaro.Optional` | **Owner** |
| 2 | Repository secret `NUGET_USER` = nuget.org profile name (not the e-mail) | **Owner** |
| 3 | Optional, in parallel: e-mail `account@nuget.org` to reserve the `Tessaro` ID prefix | **Owner** |
| 4 | Create the `release` environment and the tag ruleset (§3.4) | Claude, with the owner's go-ahead in the task |
| 5 | Add `scripts` and `pr-title` to the required checks after they ran green once | Claude, with the owner's go-ahead |
| 6 | Release-preparation PR, then `git tag v0.0.1-rc.1 && git push origin v0.0.1-rc.1`, then approve the run | **Owner** (Claude opens the PR) |

## 8. Risks

| ID | Risk | Mitigation |
|---|---|---|
| R1 | MinVer or packing behaves differently for a prerelease tag | **Spiked 2026-11-08:** tag `v0.0.1-rc.1` → `Tessaro.Optional.0.0.1-rc.1.nupkg` + `.snupkg`; nuspec has MIT licence expression, README, project URL, repository URL with commit SHA, and empty dependency groups for `net8.0` and `net10.0` |
| R2 | Package validation does not catch a breaking change, or needs a published baseline | **Spiked 2026-11-08** with a local feed: same API vs baseline `0.0.1` → passes; making `IsEmpty` internal → fails with "API breaking changes found" for both TFMs; no baseline → passes. Baseline is restored from nuget.org in the real run |
| R3 | Trusted Publishing may not allow the **first** push of a new package ID | **Resolved 2026-11-08:** the nuget.org policy form offers the scope "Push new packages and package versions". The policy uses that scope and the single package `Tessaro.Optional` (least privilege; a future `Tessaro.*` package lives in another repository and gets its own policy) |
| R4 | Dependabot's title format (`Deps | bump …`) differs from expectations | The `Deps` rule only checks the prefix; the first Dependabot PR confirms the spacing; if wrong, adjust the prefix (one line) |
| R5 | The CI badge image may be blocked on nuget.org (trusted image domains only) | Checked on the rc.1 page; if blocked, accept it or switch to a badge from an allowed domain |
| R6 | A spike that packs the real package ID pollutes the global NuGet cache (it happened in R2) | Cache entry removed; future spikes set `NUGET_PACKAGES` to a scratch folder (lesson for `sdd-task`) |

## 9. Constitution check

| Principle / gate | How this plan complies |
|---|---|
| I. KISS · YAGNI | No extra tools: SDK package validation, `gh`, Bash; no changelog, no release branches, no version bot |
| II. Fail fast | Guard job first; malformed tags, off-`main` commits, breaking changes and duplicates stop the run before publishing |
| V. DRY | Release reuses `ci.yml` through `workflow_call` instead of copying its steps |
| VI. Zero dependencies | Checked on every release (`pack` job) |
| Quality gates | Same gates run in the release; actions pinned by SHA; least-privilege permissions per job |
| Governance | Rulesets as code; owner-only tags and approval; no stored credentials |
| PR standard (ADR 0005) | Enforced by the required `pr-title` check |
| Cost | Every service is free for a public repository (spec NFR-1) |

## 10. Deferred (not part of plan 002)

- Release-note categories (`.github/release.yml`) — needs labels on PRs; revisit if notes get long.
- `dotnet-validate` / NuGet Package Explorer checks in CI — the manual SourceLink check on rc.1 is enough for now.
- SonarCloud analysis on tag builds — the tagged commit was already analysed on `main`.

## 11. Open questions

Each with a recommendation; "follow the suggestions" accepts them all.

1. **`NUGET_USER` as a secret or a repository variable?** The profile name is not a credential.
   **Recommendation:** a secret anyway, as Microsoft's docs recommend; it costs nothing.
2. **Environment and tag ruleset created by Claude with `gh api`** (with your go-ahead), as was
   done for the `main` ruleset? **Recommendation:** yes; the JSON lives in the repo either way.
3. **Delete the test tag used for AC-1.3** right after the check? **Recommendation:** yes; it is a
   throwaway (`v0.0.0-guard.1` on a branch commit) and never reaches nuget.org.

## 12. Review checklist

- [ ] Every acceptance criterion maps to a job, script or verification row in §3 and §5
- [ ] Nothing in this plan contradicts the constitution or an ADR
- [ ] Tooling choices are justified, minimal and free
- [ ] CI enforces every quality gate in the constitution, including in the release run
- [ ] Owner-only steps (nuget.org, secrets, tags, approval) are clearly separated
- [ ] Open questions answered or deferred
