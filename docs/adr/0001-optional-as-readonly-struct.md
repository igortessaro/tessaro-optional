# ADR 0001 — `Optional<T>` is a `readonly struct`

**Status:** Accepted (2026-11-07) · **Date:** 2026-11-07

## Context

In Java, `Optional` is a class, so a variable of type `Optional<T>` can itself be `null` —
defeating the type's purpose. .NET lets us choose between a reference type (`class`/`record`)
and a value type (`struct`).

## Decision

`Optional<T>` is a `readonly struct` constrained with `where T : notnull`.

## Consequences

**Positive**

- An `Optional<T>` can never be `null`; `default(Optional<T>)` and `new Optional<T>()` are the
  empty optional. This is a safe default that removes a whole class of bugs.
- No heap allocation for creating or chaining optionals.
- `readonly` guarantees immutability and lets the compiler avoid defensive copies.
- `where T : notnull` makes the compiler warn when someone tries `Optional<string?>`,
  mirroring Java's rule that an `Optional` never holds `null`.

**Negative / trade-offs**

- Casting to an interface (e.g. `IEquatable<Optional<T>>`, `object`) boxes. Acceptable: the
  hot paths (`Map`, `OrElse`, …) are instance methods and do not box.
- Structs cannot be inherited — not a need for this type.
- `notnull` is a compile-time annotation only; runtime null checks are still required
  (see constitution principle II).

## Alternatives considered

- **`sealed class`** — mirrors Java exactly but reintroduces the "null optional" problem and allocates.
- **`record struct`** — gives equality for free, but its generated `ToString()` and `with`
  expressions do not fit the intended API, and hand-written equality is trivial.
