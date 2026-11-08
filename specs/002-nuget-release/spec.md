# Spec 002 — NuGet release

**Status:** Implemented (2026-11-08) — AC-5.3 awaits the first Dependabot PR ([#34](https://github.com/igortessaro/tessaro-optional/issues/34)) · **Approved:** 2026-11-08 · **Created:** 2026-11-08
**Verification:** [verification.md](verification.md) · **Retrospective:** [retrospective.md](retrospective.md)
**Constitution:** [v1.2.0](../../docs/constitution.md) · **ADRs:** [0005](../../docs/adr/0005-pull-request-title-and-description.md)

> This spec defines **what** a release offers and **why**. Workflows, actions and tooling belong
> in `plan.md`.

---

## 1. Problem

Spec 001 is implemented, but nobody can use it: the package exists only as a local `dotnet pack`
output. Publishing by hand from a laptop is not reproducible, needs a long-lived API key on that
machine, and skips the quality gates CI enforces.

## 2. Goal

Publishing a version is one deliberate act by the owner — pushing a version tag — after which the
package that CI built, tested and verified appears on nuget.org, with a matching GitHub Release.
Everything runs on **free tiers only**.

**Success looks like:**

```bash
git tag v0.0.1-rc.1 && git push origin v0.0.1-rc.1   # owner, on a commit of main
# … owner approves the "release" run …
dotnet add package Tessaro.Optional           # anyone, minutes later
```

## 3. Users

- **Owner (Igor)** — wants to publish safely, with one command and no secrets to rotate.
- **Library consumer** — wants a trustworthy package: readable docs on nuget.org, debuggable
  source, honest versions.

---

## 4. User stories and acceptance criteria

Notation: *tag* = a git tag named `v<SemVer>`, e.g. `v1.2.3` or `v1.2.3-rc.1`.

### US-1 — Publish a version from a tag

*As the owner, I want pushing a tag to publish exactly that version, so releases are one deliberate act.*

| ID | Given | When | Then |
|---|---|---|---|
| AC-1.1 | a commit on `main` | the owner pushes tag `vX.Y.Z` and approves the run | `Tessaro.Optional` `X.Y.Z` and its symbols package are published to nuget.org |
| AC-1.2 | a commit on `main` | the owner pushes tag `vX.Y.Z-label.N` | the version is published as a **prerelease** |
| AC-1.3 | a commit that is not on `main` | a tag is pushed for it | nothing is published; the run fails saying the commit is not on `main` |
| AC-1.4 | any quality gate fails (build, format, tests, coverage) | the release runs | nothing is published |
| AC-1.5 | the version already exists on nuget.org | the release runs | the run fails; the existing version is untouched |
| AC-1.6 | a release run is waiting | nobody approves it | nothing is published |
| AC-1.7 | someone other than the owner | creates or pushes a `v*` tag | GitHub rejects the tag |

### US-2 — A release record on GitHub

| ID | Given | When | Then |
|---|---|---|---|
| AC-2.1 | a successful publish of `vX.Y.Z` | — | a GitHub Release `vX.Y.Z` exists with notes generated from the merged PRs since the previous tag |
| AC-2.2 | a successful publish | — | the `.nupkg` and `.snupkg` files are attached to the GitHub Release |
| AC-2.3 | a prerelease tag | — | the GitHub Release is marked as a prerelease and not as "latest" |

### US-3 — A trustworthy package for consumers

| ID | Given | When | Then |
|---|---|---|---|
| AC-3.1 | the published package | viewed on nuget.org | it shows the README, the MIT licence, the project and repository URLs |
| AC-3.2 | the published package | a consumer debugs into it | the debugger steps into the exact source of that commit (SourceLink + symbols) |
| AC-3.3 | the published package | its dependencies are inspected | it has none (spec 001 NFR-1) |
| AC-3.4 | the published `.nupkg` | its provenance is checked (`gh attestation verify`) | it is attested as built by this repository's release workflow from the tagged commit |
| AC-3.5 | a version after the first | it removes or changes a public member without the required version bump — major from `1.0.0` on, at least minor before it (`0.y` → `0.(y+1)`, SemVer §4) | the release fails before publishing (breaking-change check against the previous version) |

### US-4 — Prepare a release

*As the owner, I want a short, written release routine, so every release is done the same way.*

| ID | Given | When | Then |
|---|---|---|---|
| AC-4.1 | members listed in `PublicAPI.Unshipped.txt` | a release is prepared | they are moved to `PublicAPI.Shipped.txt` in a PR merged before tagging |
| AC-4.2 | the repository docs | the owner wants to release | a release section in `CONTRIBUTING.md` (or the README) lists the steps: preparation PR, tag, approval, verification |

### US-5 — Readable release notes

*As a consumer, I want release notes that say what changed in plain words; they are built from PR titles, so titles must follow the standard ([ADR 0005](../../docs/adr/0005-pull-request-title-and-description.md)).*

| ID | Given | When | Then |
|---|---|---|---|
| AC-5.1 | a PR whose title matches `<Context> \| <Imperative summary>` (ADR 0005) | it is opened or its title edited | the title check passes |
| AC-5.2 | a PR whose title does not match (e.g. `docs: update readme`, lower-case summary, trailing period, over 72 characters) | it is opened or its title edited | the title check fails with the expected format and an example, and the PR cannot be merged |
| AC-5.3 | a Dependabot PR | it is opened | its title starts with `Deps \| ` and passes the check |

---

## 5. Decisions and constraints

| Topic | Decision | Why |
|---|---|---|
| Cost | Only free tiers: GitHub Actions hosted runners, environments, Releases and artifact attestations on a **public** repository; nuget.org | Owner requirement (2026-11-08) |
| Credentials | No long-lived NuGet API key; the workflow gets a short-lived key per run (nuget.org Trusted Publishing) | Nothing to leak or rotate; Claude never handles credentials |
| Version source | The tag is the only source of the version | Already decided in plan 001 (MinVer); no version numbers edited by hand |
| Branching | Tags are created on `main`; no `release/*` branches for now | Trunk-based repository; `release/*` only becomes useful to patch an older major (YAGNI) |
| Code signing | Packages are not author-signed; nuget.org adds its repository signature | Author-signing certificates cost money; provenance attestation (AC-3.4) covers the "who built this" question for free |

## 6. Out of scope (future specs)

- A hand-written `CHANGELOG.md` — GitHub generated release notes are enough for now.
- Automated version bumps (release-please, semantic-release).
- A reusable release workflow for other `Tessaro.*` packages — once a second package exists.
- Publishing to GitHub Packages or any other feed.

## 7. Non-functional requirements

- **NFR-1** Zero cost: every service used is free for a public repository (§5).
- **NFR-2** The release workflow never runs on pull requests and cannot publish from a branch.
- **NFR-3** The published package is built by the release run itself, from the tagged commit,
  after all quality gates pass in that same run (no artifacts reused from other runs).
- **NFR-4** All quality gates in the constitution apply to the workflow files (actions pinned by
  SHA, least-privilege permissions).

## 8. Resolved questions

Decided by the owner on 2026-11-08.

1. **First published version** — **`0.0.1-rc.1`**, then `0.0.1`. `1.0.0` waits until the project
   is more mature; until then SemVer's `0.y.z` rules apply (AC-3.5).
2. **Approval step (AC-1.6)** — **yes**, the owner approves each release in a GitHub environment.
3. **Reserve the `Tessaro` ID prefix on nuget.org** — **yes**, before the first publish
   (owner-only step).
4. **Re-running a release whose version is already published (AC-1.5)** — **fail** (fail fast).
5. **Breaking-change check (AC-3.5)** — **yes**, from the first release.
6. **PR title standard** — **option A**, `<Context> | <Imperative summary>`
   ([ADR 0005](../../docs/adr/0005-pull-request-title-and-description.md)), enforced by a CI check
   (US-5, added on the owner's request).

## 9. Review checklist

- [x] Every acceptance criterion is testable as written
- [x] Nothing in this spec prescribes implementation details
- [x] Every service used is free for this repository
- [x] Owner-only steps are identifiable (tag push, approval, nuget.org policy, prefix reservation)
- [x] Out-of-scope items are acceptable
- [x] Open questions answered or deferred
