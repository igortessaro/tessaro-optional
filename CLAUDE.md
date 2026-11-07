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

Tests run on Microsoft.Testing.Platform (opted in via `global.json`), so `dotnet test` takes
`--solution`/`--project` and runner options directly (no `--` separator).

```bash
dotnet build Tessaro.Optional.slnx -c Release                       # build (warnings are errors)
dotnet test --solution Tessaro.Optional.slnx -c Release             # run tests on net8.0 and net10.0
dotnet test --solution Tessaro.Optional.slnx -c Release \
  --coverlet --coverlet-output-format opencover \
  --coverlet-threshold 80 --coverlet-threshold-type line --coverlet-threshold-type branch   # coverage gate
dotnet format Tessaro.Optional.slnx --verify-no-changes             # style gate
dotnet pack src/Tessaro.Optional/Tessaro.Optional.csproj -c Release  # package (version from git tags via MinVer)
```

A test run with zero tests exits with code 8 (MTP "zero tests ran").

## Collaboration

- Igor wants to evolve this workflow: when an instruction is worth turning into a skill,
  subagent, hook or rule, say so explicitly.
- Write repository artifacts in English; talk to Igor in Portuguese.
