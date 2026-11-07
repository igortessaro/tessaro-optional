# CLAUDE.md — Tessaro.Optional

A zero-dependency .NET port of Java's `java.util.Optional`.

## Read first

- `docs/constitution.md` — non-negotiable principles and quality gates. Check every change against it.
- `docs/adr/` — architecture decisions. Don't contradict an accepted ADR; propose a new one instead.
- `specs/NNN-*/` — the active spec, plan and tasks.

## How we work (Spec-Driven Development)

1. `spec.md` (what/why) → 2. `plan.md` (how) → 3. `tasks.md` (ordered steps) → 4. implement.
- Don't start a phase until the owner (Igor) approved the previous one.
- Implement test-first, one task at a time, on a feature branch; open a PR. **Never push to `main`.**
- Don't add public API that no approved spec requires (YAGNI).
- Commits: Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`, `ci:`, `chore:`).

## Code style

- Follow the constitution: KISS, fail fast (validate every argument eagerly), SOLID, DRY.
- Name design patterns explicitly when you use one, and only when they simplify.
- XML docs on every public member.

## Commands

_To be defined in `specs/001-optional-core/plan.md` (build, test, format, coverage)._

## Collaboration

- Igor wants to evolve this workflow: when an instruction is worth turning into a skill,
  subagent, hook or rule, say so explicitly.
- Write repository artifacts in English; talk to Igor in Portuguese.
