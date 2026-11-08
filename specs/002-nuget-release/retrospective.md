# Retrospective 002 — NuGet release

**Date:** 2026-11-08 · **Spec:** [002](spec.md) (Implemented 2026-11-08) · **Tasks:** T00–T06, one PR each

## Outcome

- `Tessaro.Optional 0.0.1-rc.1` is on nuget.org. It was published by CI from a tag on `main`
  after the owner's approval, with no stored NuGet key (Trusted Publishing), a verifiable
  provenance attestation, SourceLink symbols and a GitHub Release.
- 19 of 20 ACs have evidence ([verification.md](verification.md)). AC-5.3 waits for the first
  Dependabot PR ([#34](https://github.com/igortessaro/tessaro-optional/issues/34)).
- The negative paths were exercised on the real workflow, not only in tests: a tag off `main`, an
  unapproved run and a duplicate version.
- Everything runs on free tiers.

## What went well

- **Spikes before the plan was approved.** Packing from a prerelease tag and package validation
  against a baseline were proved in the plan phase, so T03 had no surprises in the .NET part.
- **Logic in scripts, not YAML.** The guard and the title check are unit-tested (41 cases) and
  ShellCheck/actionlint-clean. Each was proved able to fail with mutants.
- **Negative checks on the real pipeline** caught nothing broken, and now the evidence exists.
- **The PR title standard shows its value in the generated release notes:** the old
  `feat: …` titles and the new `Spec 002 (T05) | …` titles sit side by side.

## What we would change

| Problem | When it showed up | Change for next time |
|---|---|---|
| A step `run:` without an explicit shell is `bash -e` **without `pipefail`**, so `script \| tee` hid a failure. With `pipefail` on, `unzip \| grep -q` can SIGPIPE and hide a match | T03 | Every workflow sets `defaults.run.shell: bash`; capture output before piping it, and read files fully before `grep -q` (added to `sdd-task`) |
| A spike packed the real package ID into the global NuGet cache | Plan phase | Spikes use a scratch `NUGET_PACKAGES` (already added to `sdd-task`) |
| The PR status tool showed green checks from the **previous** commit | T00 | Before merging, read the checks for the PR's head SHA (added to `sdd-task`) |
| `Closes #N` on a PR whose task finishes only after the merge (a tag, a release) would have closed the issue too early | T05 | Use `Part of #N` and close the issue by hand when "done when" is met (added to `sdd-task`) |
| An owner step that Claude assumed had run (the `publish` re-run) had not | T05 | Read the resulting state (run attempt, pending deployments) before asking for the next approval; offer to run commands that need no credentials |
| The traceability check counted a **Pending** verification row as covered | T06 | The script now reports rows marked `Pending` and fails unless `--allow-pending` is passed |
| AC-5.3 depends on an external event (Dependabot's schedule) that the spec's own runs could not trigger | T06 | Write ACs that the spec's own runs can exercise, or name the manual trigger in the plan (here: *Insights → Dependency graph → Dependabot → Check for updates*) |

These changes were applied to the `sdd` and `sdd-task` skills.
