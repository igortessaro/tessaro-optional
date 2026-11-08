# Plan 001 — Optional core

**Status:** Approved (2026-11-07) · **Created:** 2026-11-07
**Spec:** [001 — Optional core](spec.md) (Approved 2026-11-07) · **Constitution:** [v1.0.0](../../docs/constitution.md)

> This plan defines **how** spec 001 is built: solution layout, public API signatures, build and
> test tooling, the CI pipeline and the one-time repository setup. The ordered implementation
> steps belong in `tasks.md`.

---

## 1. Technical context

| Item | Choice | Reason |
|---|---|---|
| Language | C# (latest stable `LangVersion`) | — |
| Target frameworks | `net8.0;net10.0` | Both current LTS releases; no polyfills needed |
| SDK | .NET 10, pinned in `global.json` (`rollForward: latestFeature`) | Reproducible builds; one SDK builds both TFMs |
| Solution format | `Tessaro.Optional.slnx` | Default format of the .NET 10 SDK; diff-friendly XML |
| Runtime dependencies | None | Constitution VI |
| Test framework | xUnit v3 + Shouldly | Owner decision (FluentAssertions is commercial since v8) |
| Test platform | Microsoft.Testing.Platform (MTP), opted in via `global.json` | Required: xUnit v3 4.x on the .NET 10 SDK no longer supports VSTest (amended 2026-11-07, T01) |
| Coverage | coverlet (`coverlet.MTP`), OpenCover format | Supported by SonarCloud; `--coverlet-threshold` enforces the gate locally and in CI (was `coverlet.msbuild`, which needs VSTest) |
| Package management | Central Package Management (`Directory.Packages.props`) | One place for versions; Dependabot-friendly |
| Versioning | MinVer (version from git tags `vX.Y.Z`) | No version numbers to edit by hand; build-time only |

Package versions are not fixed in this plan: each task uses the latest stable version at
implementation time, and Dependabot keeps them current.

---

## 2. Repository layout

```
tessaro-optional/
├── dotnet-tools.json                # local tools: dotnet-sonarscanner (SDK 10 default location)
├── .github/
│   ├── workflows/ci.yml
│   ├── rulesets/main.json           # branch ruleset as code (applied with gh api)
│   ├── dependabot.yml
│   ├── CODEOWNERS
│   └── pull_request_template.md
├── docs/ (constitution, adr/)
├── specs/ (001-optional-core/)
├── src/Tessaro.Optional/
│   ├── Optional.cs                  # static factory: Empty, Of, OfNullable
│   ├── Optional{T}.cs               # the struct: state, transform, extract, side effects
│   ├── Optional{T}.Linq.cs          # partial: Select, Where, SelectMany
│   ├── Optional{T}.Equality.cs      # partial: IEquatable, operators, GetHashCode, ToString
│   ├── OptionalExtensions.cs        # ToOptional()
│   ├── PublicAPI.Shipped.txt
│   └── PublicAPI.Unshipped.txt
├── tests/Tessaro.Optional.Tests/
│   ├── CreationTests.cs             # US-1
│   ├── StateTests.cs                # US-2
│   ├── TransformationTests.cs       # US-3
│   ├── ExtractionTests.cs           # US-4
│   ├── SideEffectTests.cs           # US-5
│   ├── IdiomTests.cs                # US-6
│   ├── EqualityTests.cs             # US-7
│   ├── AllocationTests.cs           # NFR-2
│   └── OverloadResolutionTests.cs   # compile-time checks for OfNullable/ToOptional (see §8, risk R1)
├── .editorconfig
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── Tessaro.Optional.slnx
├── CLAUDE.md · README.md · LICENSE
```

The struct is split into `partial` files by concern (Single Responsibility at file level) so each
file maps to one or two user stories and stays small.

---

## 3. Public API

All members validate every argument eagerly (constitution II) with
`ArgumentNullException.ThrowIfNull`. Every member gets XML docs with `<exception>` tags.

### 3.1 Factory — `Optional`

```csharp
public static class Optional
{
    public static Optional<T> Empty<T>() where T : notnull;                 // AC-1.1
    public static Optional<T> Of<T>(T value) where T : notnull;             // AC-1.3, AC-1.4
    public static Optional<T> OfNullable<T>(T? value) where T : notnull;    // AC-1.5, AC-1.6 (reference types)
    public static Optional<T> OfNullable<T>(T? value) where T : struct;     // AC-1.5, AC-1.6 (Nullable<T>)
}
```

### 3.2 The struct — `Optional<T>`

```csharp
public readonly partial struct Optional<T> : IEquatable<Optional<T>> where T : notnull
{
    private readonly T _value;
    private readonly bool _isPresent;          // default(Optional<T>) => false => empty (AC-1.2)

    // State — US-2
    public bool IsPresent { get; }
    public bool IsEmpty { get; }

    // Transform — US-3
    public Optional<TResult> Map<TResult>(Func<T, TResult?> mapper) where TResult : notnull;
    public Optional<TResult> FlatMap<TResult>(Func<T, Optional<TResult>> mapper) where TResult : notnull;
    public Optional<T> Filter(Func<T, bool> predicate);

    // Extract — US-4
    [return: NotNullIfNotNull(nameof(other))]
    public T? OrElse(T? other);                                              // AC-4.12: OrElse(null) allowed
    public T OrElseGet(Func<T> supplier);
    public T OrElseThrow();                                                  // InvalidOperationException("No value present")
    public T OrElseThrow<TException>(Func<TException> exceptionFactory) where TException : Exception;
    public Optional<T> Or(Func<Optional<T>> supplier);

    // Side effects — US-5
    public void IfPresent(Action<T> action);
    public void IfPresentOrElse(Action<T> action, Action emptyAction);

    // .NET idioms — US-6
    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none);
    public bool TryGetValue([MaybeNullWhen(false)] out T value);
    public IEnumerable<T> AsEnumerable();

    // LINQ aliases — AC-6.6, AC-6.7 (each is a one-line delegation; DRY)
    public Optional<TResult> Select<TResult>(Func<T, TResult?> selector) where TResult : notnull;
    public Optional<T> Where(Func<T, bool> predicate);
    public Optional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> selector) where TResult : notnull;
    public Optional<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Optional<TIntermediate>> selector,
        Func<T, TIntermediate, TResult?> resultSelector)
        where TIntermediate : notnull where TResult : notnull;

    // Equality and text — US-7
    public bool Equals(Optional<T> other);
    public override bool Equals(object? obj);
    public override int GetHashCode();                                       // empty => 0
    public override string ToString();                                       // "Optional[v]" / "Optional.empty"
    public static bool operator ==(Optional<T> left, Optional<T> right);
    public static bool operator !=(Optional<T> left, Optional<T> right);
}
```

### 3.3 Extensions — `OptionalExtensions`

```csharp
public static class OptionalExtensions
{
    public static Optional<T> ToOptional<T>(this T? value) where T : notnull;   // AC-1.7
    public static Optional<T> ToOptional<T>(this T? value) where T : struct;    // AC-1.7
}
```

### 3.4 Behaviour details not stated in the spec

| Case | Behaviour | Reason |
|---|---|---|
| `OrElseThrow(factory)` and `factory()` returns `null` | throws `InvalidOperationException` | Fail fast with a meaningful message instead of `NullReferenceException` (Java throws NPE) |
| `Map` with a value-type `TResult` | never produces empty from the mapper | `TResult?` is not `Nullable<T>` for unconstrained generics; nullable value types are deferred to v1.1 (spec §8.1) |
| `AsEnumerable()` | returns `[value]` or `Array.Empty<T>()` | Simplest correct implementation; allocates (excluded from NFR-2, it is an exit to collection-land) |
| `ToString()` of a present value | `$"Optional[{value}]"` | Same as Java |

---

## 4. Build configuration

`Directory.Build.props` (applies to every project):

- `Nullable=enable`, `LangVersion=latest`, `ImplicitUsings=enable`
- `TreatWarningsAsErrors=true`, `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild=true`
- `GenerateDocumentationFile=true` (missing XML docs become build errors via CS1591)
- `Deterministic=true`; `ContinuousIntegrationBuild=true` when `GITHUB_ACTIONS=true`

Library project only:

- Package metadata: `PackageId=Tessaro.Optional`, `Authors=Igor Tessaro`, `PackageLicenseExpression=MIT`,
  `RepositoryUrl`, `PackageReadmeFile=README.md`, `PackageTags=optional;maybe;functional;java`
- Symbols: `IncludeSymbols=true`, `SymbolPackageFormat=snupkg` (SourceLink ships with the .NET SDK)
- Build-time only packages (`PrivateAssets=all`, so constitution VI holds):
  `Microsoft.CodeAnalysis.PublicApiAnalyzers`, `MinVer`

`.editorconfig`: standard .NET conventions, file-scoped namespaces, `var` when the type is apparent,
private fields `_camelCase`, severity `warning` for style rules so they fail the build.

---

## 5. Test strategy

- **Test-first.** Each task in `tasks.md` starts with failing tests for its acceptance criteria.
- **Traceability.** Every test carries `[Trait("AC", "AC-3.2")]`; every AC in the spec has at least
  one test. Naming: `Member_Scenario_ExpectedResult`.
- **Both kinds of `T`.** Behaviour that can differ between reference and value types
  (nullability, equality, `ToString`) is tested with `string` and `int`.
- **Delegates not invoked.** "is not invoked" criteria use a delegate that fails the test if called.
- **Allocation (NFR-2).** `AllocationTests` measures `GC.GetAllocatedBytesForCurrentThread()`
  around a warmed-up loop of create/chain/extract calls using `static` lambdas and asserts `0` bytes.
- **Overload resolution (risk R1).** Tests that only need to compile, proving
  `OfNullable(string?)`, `OfNullable(int?)` and `ToOptional()` bind to the intended overloads.
- **Coverage gate.** `dotnet test` with coverlet fails below **80 % line and branch** (constitution).
  This fails fast locally and in CI; SonarCloud's default "Sonar way" gate applies the same 80 % to new code.

---

## 6. CI pipeline — `.github/workflows/ci.yml`

Triggers: `pull_request` targeting `main`, and `push` to `main`.
`concurrency` cancels superseded runs for the same ref. `permissions: contents: read`.
Third-party actions are pinned by commit SHA (with a version comment) and updated by Dependabot.

```
┌─────────────── format ───────────────┐   ┌──────────────────── build-test ────────────────────┐
│ checkout                             │   │ checkout (fetch-depth: 0 — MinVer + Sonar blame)   │
│ setup-dotnet (global.json)           │   │ setup-java 17 (SonarScanner) · setup-dotnet 8 + 10 │
│ dotnet format --verify-no-changes    │   │ cache NuGet · dotnet tool restore                  │
└──────────────────────────────────────┘   │ sonarscanner begin  (skipped if no SONAR_TOKEN)    │
                                           │ dotnet build -c Release                            │
                                           │ dotnet test  -c Release  + coverage ≥ 80 %         │
                                           │ sonarscanner end  (sonar.qualitygate.wait=true)    │
                                           └────────────────────────────────────────────────────┘
```

- `format` runs in parallel and fails fast on style problems.
- `build-test` runs the tests for **both** target frameworks.
- Required status checks on `main`: `format`, `build-test`, and SonarCloud's `SonarCloud Code Analysis`.
- Only Ubuntu runners: the library is pure managed code, so an OS matrix adds cost without value (YAGNI).

`dependabot.yml`: weekly updates for `nuget` and `github-actions`.

---

## 7. One-time repository setup

| # | Step | Who |
|---|---|---|
| 1 | `brew install gh` and `gh auth login` | Owner (done 2026-11-07) |
| 2 | `git init -b main`; first commit with docs | Claude |
| 3 | `gh repo create igortessaro/tessaro-optional --public`; remote over SSH (`git@github.com:igortessaro/tessaro-optional.git`); push `main` | Claude, after owner approval |
| 4 | Repo settings: squash merge only, delete branch on merge, no wiki/projects | Claude (`gh repo edit`) |
| 5 | Apply `.github/rulesets/main.json` with `gh api` (see below) | Claude |

Status checks are added to the ruleset only after the CI workflow has run green once (otherwise no
PR could ever be merged).
| 6 | SonarCloud: sign in with GitHub, create organization, import the repo, **turn off Automatic Analysis**, generate a token | Owner |
| 7 | `gh secret set SONAR_TOKEN` (owner pastes the token at the prompt; Claude never handles it) | Owner |
| 8 | Open the first PR to verify all checks run and the ruleset blocks direct pushes | Claude |

**Ruleset for `main`** (versioned as code in `.github/rulesets/main.json`):

- Block deletion and force-pushes (`deletion`, `non_fast_forward`)
- `required_linear_history`
- `pull_request` with 0 required approvals (the owner cannot approve their own PR)
- `required_status_checks`: `format`, `build-test`, `SonarCloud Code Analysis`
- Bypass: repository admin role only (the owner), mode `pull_request` (§11.1)

In a personal repository only the owner and invited collaborators can write; with no collaborators,
the owner is the only person who can ever merge into `main`.

---

## 8. Risks

| ID | Risk | Mitigation |
|---|---|---|
| R1 | `OfNullable`/`ToOptional` overloads (`T?` with `notnull` vs `struct`) may resolve ambiguously | **Spiked 2026-11-07 (.NET 10 SDK, warnings as errors):** `string?` → `notnull` overload, `int?` → `struct` overload, `int` → `notnull` overload (always present), unconstrained generic `T?` → `notnull` overload — all as intended, no ambiguity. Kept as regression tests in `OverloadResolutionTests` |
| R2 | `notnull` is only a compiler warning, so `Optional<string?>` can still be created | Warnings are errors in this repo; runtime null checks remain (constitution II) |
| R3 | Allocation tests can be flaky (JIT tiering, test-runner noise) | Warm up before measuring; measure per thread; a single retry is not allowed — investigate instead |
| R4 | A custom SonarCloud quality gate may require a paid plan | Not needed: the 80 % target matches the default "Sonar way" gate (ADR 0003) |
| R5 | Sonar steps fail on PRs from forks (no secrets) | Sonar steps are skipped when `SONAR_TOKEN` is absent; only the owner contributes for now |

---

## 9. Constitution check

| Principle / gate | How this plan complies |
|---|---|
| I. KISS · YAGNI | Only spec 001 members; no OS matrix, no benchmark project, no release workflow yet |
| II. Fail fast | `ThrowIfNull` on every delegate; `OrElseThrow` factory returning `null` throws |
| III. Semantics | `readonly struct`, fluent API, LINQ query syntax |
| IV. Java fidelity | Java names; deviations listed in spec §5 and §3.4 here |
| V. SOLID · DRY | Partial files by concern; aliases delegate to primary members |
| VI. Zero dependencies | Analyzers and MinVer are `PrivateAssets=all` |
| Quality gates | Warnings as errors, `dotnet format`, ≥ 80 % coverage, Sonar default gate, XML docs, PublicApiAnalyzers |
| Governance | Ruleset as code; Claude works only on branches and PRs |

---

## 10. Deferred (not part of plan 001)

- **Release workflow** (pack + publish to NuGet on tag `v*`) — its own small plan once the API is
  implemented and reviewed; nothing to publish before that.
- **Mutation testing** (Stryker.NET) — worth revisiting once the core is stable.
- **Reusable GitHub Actions workflow** for future `Tessaro.*` packages — when a second package exists.

---

## 11. Resolved questions

Decided by the owner on 2026-11-07.

1. **Ruleset bypass mode** — `pull_request`: the owner bypasses rules only when merging a PR, so
   every change, including the owner's, goes through CI.
2. **ADRs 0001 and 0002** — `Accepted`.
3. **Repository name** — `tessaro-optional` under the owner's GitHub account.
4. **Coverage target** — lowered to **80 % line and branch**, using SonarCloud's default quality
   gate; no custom gate for now ([ADR 0003](../../docs/adr/0003-coverage-target-80.md), constitution v1.1.0).

## 12. Review checklist

- [x] Every acceptance criterion in the spec maps to a member in §3 and a test file in §2
- [x] Nothing in this plan contradicts the constitution or an ADR
- [x] Tooling choices are justified and minimal
- [x] CI enforces every quality gate in the constitution
- [x] Owner-only steps (tokens, sign-ins) are clearly separated
- [x] Open questions answered or deferred
