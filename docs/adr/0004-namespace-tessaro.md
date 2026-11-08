# ADR 0004 — Public types live in the `Tessaro` namespace

**Status:** Accepted (2026-11-08, PR #14) · **Date:** 2026-11-08

## Context

The package is `Tessaro.Optional` and its main types are `Optional` and `Optional<T>`. Putting
them in a namespace `Tessaro.Optional` causes two problems, both found while implementing T03:

1. **Type/namespace clash.** In any code whose namespace starts with `Tessaro.` — such as the
   planned `Tessaro.Response` package — the C# compiler resolves `Optional` to the *namespace*
   `Tessaro.Optional` before the type, so `Optional.Of(1)` fails with
   `CS0234: The type or namespace name 'Of' does not exist in the namespace 'Tessaro.Optional'`.
   Reproduced with a throwaway consumer project on 2026-11-08.
2. **CA1716** flags the namespace (and the types) because `Optional` is a reserved keyword in VB.NET.

## Decision

- `Optional`, `Optional<T>` and `OptionalExtensions` are declared in the namespace **`Tessaro`**.
  The package ID, assembly name and folder names stay `Tessaro.Optional`.
- Tests use the namespace `Tessaro.Tests`.
- CA1716 is suppressed **on the two types only**, with a justification: the Java-compatible name is
  the purpose of the library (constitution IV), and VB.NET callers can escape it as `[Optional]`.

## Consequences

**Positive**

- `Optional.Of(...)` works in every namespace: with `using Tessaro;` from outside, and with no
  `using` at all from inside `Tessaro.*` (verified with the same throwaway consumer).
- Future `Tessaro.*` packages can use `Optional<T>` without aliases or `global::`.

**Negative / trade-offs**

- Namespace and package ID differ (`using Tessaro;` for package `Tessaro.Optional`). This is common
  for small libraries but must be shown in the README.
- Several `Tessaro.*` packages will share the root `Tessaro` namespace, so their type names must
  not collide with each other.

## Alternatives considered

- **Keep `Tessaro.Optional`** — breaks `Optional.Of(...)` inside any `Tessaro.*` namespace; every
  sibling package would need `global::Tessaro.Optional.Optional` or an alias.
- **`Tessaro.Optionals`** — avoids the clash but is an awkward, non-obvious name.
- **Rename the types (e.g. `Maybe<T>`)** — removes CA1716 entirely but abandons Java fidelity,
  the library's reason to exist.
