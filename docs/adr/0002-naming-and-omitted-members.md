# ADR 0002 — Naming conventions and members deliberately left out

**Status:** Accepted (2026-11-07) · **Date:** 2026-11-07

## Context

The library promises Java fidelity (constitution IV) but must feel natural to .NET developers
and enable LINQ query syntax.

## Decision

1. **Java names in PascalCase are the primary API**: `Map`, `FlatMap`, `Filter`, `OrElse`,
   `OrElseGet`, `OrElseThrow`, `IfPresent`, `IfPresentOrElse`, `Or`.
2. **Java `isX()` methods become properties**: `IsPresent`, `IsEmpty`.
3. **LINQ aliases** `Select`, `SelectMany` and `Where` are provided and delegate to the primary
   members. `SelectMany` includes the result-selector overload required by query syntax
   (`from a in optA from b in optB select a + b`).
4. **Omitted members**
   - `get()` — omitted. Java's own maintainers consider it a design mistake (Java 10 added the
     equivalent `orElseThrow()` precisely to give it an honest name). Use `OrElseThrow()`.
   - `OptionalInt`, `OptionalLong`, `OptionalDouble` — omitted. .NET generics are reified, so
     `Optional<int>` has no boxing cost.
   - `stream()` — replaced by `AsEnumerable()` instead of implementing `IEnumerable<T>` on the
     struct, to avoid boxing and to stop serializers/LINQ overloads from treating an optional as
     a collection.

## Consequences

- Java developers recognise the API immediately; .NET developers get LINQ integration.
- Two names exist for three operations; documentation must state that aliases are equivalent.
