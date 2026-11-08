# ADR 0005 — Pull request title and description standard

**Status:** Accepted (2026-11-08) · **Date:** 2026-11-08

## Context

Every change reaches `main` as one squash commit whose subject is the pull request title, and
GitHub's generated release notes (spec 002) list those same titles. Until now titles followed
Conventional Commits (`docs: document Optional<T> usage and close spec 001 (T10)`): the type is
machine-readable, but the spec and task a change belongs to are buried at the end, and the
release notes read like a commit log.

The version comes from git tags (MinVer), so no tool in this repository parses commit types.

## Decision

**Title:** `<Context> | <Imperative summary>`

| Context | Use for | Example |
|---|---|---|
| `Spec NNN (TNN)` | a task of an SDD task list | `Spec 001 (T10) \| Document Optional<T> usage` |
| `Spec NNN` | spec-phase documents (spec, plan, tasks) of one spec | `Spec 002 \| Add NuGet release spec` |
| `Hotfix` | an urgent fix to released code | `Hotfix \| Fix OrElse for empty structs` |
| `Deps` | dependency updates (Dependabot) | `Deps \| Bump xunit.v3 from 4.0.2 to 4.1.0` |
| `Chore` | anything else without a spec (tooling, repo settings) | `Chore \| Update the PR template` |

- The summary starts with a capital letter and an imperative verb ("Add", "Fix", "Document"), has
  no trailing period, and the whole title stays within 72 characters.
- No issue number in the title: `Closes #N` goes in the body and the squash merge appends `(#PR)`.
- The squash commit subject on `main` is the PR title followed by ` (#PR)`.
- Commits on feature branches may keep Conventional Commits; they are squashed away.

**Description sections**, in this order; a section that does not apply is omitted, never left empty:

1. **Summary** — two to four sentences: what changed and why.
2. **Changes** — bullet list.
3. **Testing** — what was run and the real results (a table), including manual checks.
4. **Notes for reviewer** — ADRs, deviations from the plan, risks, what is left for later.
5. **Checklist** — the definition of done; the CI box is ticked only once CI is green.
6. `Closes #N` as the last line.

The title format is enforced by a CI check (spec 002).

## Consequences

**Positive**

- The spec and task of every change are visible first, in the PR list, in `git log` and in
  release notes.
- Release notes are readable by consumers without editing.

**Negative / trade-offs**

- `main` no longer follows Conventional Commits, so tools that derive versions or changelogs from
  commit types (release-please, semantic-release) would not work without changes. Acceptable:
  versions come from tags, and automated version bumps are out of scope (spec 002 §6).
- Dependabot titles need a configured prefix to match the format.

## Alternatives considered

- **Conventional Commits with a scope** (`docs(001/T10): document Optional<T> usage`) — keeps
  tool compatibility, but is less readable; rejected by the owner.
- **Keep the current titles** — the spec and task are easy to miss.
