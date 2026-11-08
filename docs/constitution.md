# Tessaro.Optional — Constitution

> The non-negotiable principles of this project. Every spec, plan, task and pull request
> is checked against this document. Changing it requires an ADR in `docs/adr/`.

**Version:** 1.2.0 · **Ratified:** 2026-11-07 · **Amended:** 2026-11-07 ([ADR 0003](adr/0003-coverage-target-80.md)), 2026-11-08 ([ADR 0005](adr/0005-pull-request-title-and-description.md)) · **Owner:** Igor Tessaro

---

## Mission

Bring the expressiveness of Java's `java.util.Optional` to .NET so that absence of a value
is modeled explicitly and handled with semantic, expression-based code instead of
`if (x != null)` / `else` chains.

---

## Principles

### I. Simplicity first (KISS · YAGNI)

- A public member exists only if an **approved spec** requires it. No speculative API.
- Prefer the smallest design that satisfies the spec; clever code needs a written justification.
- New features land in a new numbered spec (`specs/NNN-*`), never by "while I'm here" changes.

### II. Fail fast

- Invalid input is rejected **immediately**, at the call site, with the standard .NET exception
  (`ArgumentNullException`, `InvalidOperationException`, …) and a clear message.
- **Every delegate argument is validated eagerly**, whether or not the optional holds a value.
  This deliberately deviates from Java, where some methods only fail when a value is present.
- No silent fallbacks, no swallowed exceptions.

### III. Semantics over control flow

- The API must make the common cases expressible as a single fluent expression.
- An `Optional<T>` itself can never be `null` (see ADR 0001).

### IV. Java fidelity, .NET idioms

- Java method names are kept, in PascalCase (`map` → `Map`, `orElseGet` → `OrElseGet`).
- .NET conventions win where Java's choice would feel foreign or unsafe
  (properties instead of `isX()` methods, LINQ aliases, `Try*` pattern).
- Every deviation from Java is recorded in the spec or an ADR, with the reason.

### V. Clean design (SOLID · DRY)

- Aliases (`Select`, `Where`, `SelectMany`) delegate to the primary implementation — one source of truth.
- Types are small and cohesive; extension methods are grouped by the concept they extend.

### VI. Zero runtime dependencies

- The shipped package depends only on the .NET base class library.
- Test and build tooling may use third-party packages.

---

## Quality gates (enforced by CI)

| Gate | Rule |
|---|---|
| Build | `TreatWarningsAsErrors=true`, nullable reference types enabled, .NET analyzers on |
| Format | `dotnet format --verify-no-changes` passes |
| Tests | Test-first (TDD). Every public member has tests for the present case, the empty case and invalid arguments |
| Coverage | ≥ 80 % line and branch coverage, enforced in CI by the test run; SonarCloud's default quality gate (80 % on new code) must also pass |
| Static analysis | SonarCloud quality gate green; no new bugs, vulnerabilities or code smells above *minor* |
| Documentation | XML docs on 100 % of the public API, including `<exception>` tags |
| Public API | Changes tracked with `Microsoft.CodeAnalysis.PublicApiAnalyzers`; breaking changes only in a major version (SemVer) |

---

## Workflow (Spec-Driven Development)

1. **Spec** (`spec.md`) — *what* and *why*, user stories, acceptance criteria. No implementation details.
2. **Plan** (`plan.md`) — *how*: signatures, target frameworks, tooling, test strategy.
3. **Tasks** (`tasks.md`) — small, ordered, independently verifiable steps, tests first.
4. **Implement** — one task (or a small group) per branch → pull request → green CI → merge.

Each phase is reviewed and approved by the owner before the next one starts.

## Repository governance

- `main` is protected: no direct pushes, no force-pushes, no deletion; pull request with all
  required checks green; linear history. Only the owner may merge or bypass.
- Claude (AI assistant) works exclusively on feature branches and opens pull requests;
  it never pushes to `main`.
- Pull request titles follow `<Context> | <Imperative summary>` (e.g. `Spec 001 (T10) | Document Optional<T> usage`)
  and descriptions use the sections Summary, Changes, Testing, Notes for reviewer, Checklist
  ([ADR 0005](adr/0005-pull-request-title-and-description.md)). The squash commit on `main` uses the PR title.
  Commits on feature branches follow [Conventional Commits](https://www.conventionalcommits.org/).
