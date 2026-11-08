# Spec 001 — Optional core

**Status:** Implemented (2026-11-08) · **Approved:** 2026-11-07 · **Created:** 2026-11-07
**Constitution:** [v1.1.0](../../docs/constitution.md) · **ADRs:** [0001](../../docs/adr/0001-optional-as-readonly-struct.md), [0002](../../docs/adr/0002-naming-and-omitted-members.md)

> This spec defines **what** the first release offers and **why**. Signatures, target frameworks
> and tooling belong in `plan.md`.

---

## 1. Problem

.NET code frequently models "maybe there is a value" with `null`. The intent is invisible in the
type, and every consumer must remember to write `if (x != null) … else …`. Java's
`java.util.Optional` solves this with a container type and a fluent API. .NET has
`Nullable<T>` (value types only, no fluent API) and nullable reference types (compile-time
warnings only, no behaviour).

## 2. Goal

Provide an `Optional<T>` type with the full Java 8–11 `Optional` feature set, adapted to .NET
idioms, so that absent values are explicit and handled through expressions.

**Success looks like:**

```csharp
string city = FindUser(id)               // Optional<User>
    .FlatMap(u => u.Address)             // Optional<Address>
    .Map(a => a.City)                    // Optional<string>
    .Filter(c => c.Length > 0)
    .OrElse("Unknown");
```

## 3. Users

- **Library consumer** — a .NET developer who wants to express absence explicitly.
- **Java developer moving to .NET** — expects the familiar API.

---

## 4. User stories and acceptance criteria

Notation: *present* = optional holding a value; *empty* = optional without a value.
Unless stated otherwise, "throws" means the exception is raised immediately, before any delegate runs.

### US-1 — Create optionals

*As a consumer, I want to wrap a value, a possibly-null value, or nothing, so absence becomes explicit.*

| ID | Given | When | Then |
|---|---|---|---|
| AC-1.1 | — | `Optional.Empty<T>()` | the result is empty |
| AC-1.2 | — | `default(Optional<T>)` | the result is empty and equal to `Optional.Empty<T>()` |
| AC-1.3 | a non-null value `v` | `Optional.Of(v)` | the result is present with `v` |
| AC-1.4 | `null` | `Optional.Of(null)` | throws `ArgumentNullException` |
| AC-1.5 | a non-null value `v` | `Optional.OfNullable(v)` | the result is present with `v` |
| AC-1.6 | `null` | `Optional.OfNullable(null)` | the result is empty |
| AC-1.7 | a nullable reference or a `Nullable<T>` struct | `.ToOptional()` | same behaviour as `OfNullable` |

### US-2 — Inspect state

| ID | Given | When | Then |
|---|---|---|---|
| AC-2.1 | present | `IsPresent` / `IsEmpty` | `true` / `false` |
| AC-2.2 | empty | `IsPresent` / `IsEmpty` | `false` / `true` |

### US-3 — Transform without `if`

*As a consumer, I want to chain transformations that are skipped automatically when there is no value.*

| ID | Given | When | Then |
|---|---|---|---|
| AC-3.1 | present `v` | `Map(f)` | present with `f(v)` |
| AC-3.2 | present `v`, `f(v)` returns `null` | `Map(f)` | empty (same as Java) |
| AC-3.3 | empty | `Map(f)` | empty; `f` is **not** invoked |
| AC-3.4 | present `v` | `FlatMap(f)` | exactly the optional returned by `f(v)` |
| AC-3.5 | empty | `FlatMap(f)` | empty; `f` is not invoked |
| AC-3.6 | present `v`, `p(v)` is `true` | `Filter(p)` | the same present optional |
| AC-3.7 | present `v`, `p(v)` is `false` | `Filter(p)` | empty |
| AC-3.8 | empty | `Filter(p)` | empty; `p` is not invoked |
| AC-3.9 | any state | `Map`/`FlatMap`/`Filter` with a `null` delegate | throws `ArgumentNullException` |

### US-4 — Extract a value with a fallback

| ID | Given | When | Then |
|---|---|---|---|
| AC-4.1 | present `v` | `OrElse(o)` | `v` |
| AC-4.2 | empty | `OrElse(o)` | `o` |
| AC-4.3 | present `v` | `OrElseGet(s)` | `v`; `s` is not invoked |
| AC-4.4 | empty | `OrElseGet(s)` | `s()` |
| AC-4.5 | present `v` | `OrElseThrow()` | `v` |
| AC-4.6 | empty | `OrElseThrow()` | throws `InvalidOperationException` |
| AC-4.7 | present `v` | `OrElseThrow(factory)` | `v`; `factory` is not invoked |
| AC-4.8 | empty | `OrElseThrow(factory)` | throws the exception returned by `factory()` |
| AC-4.9 | present `v` | `Or(s)` | the same present optional; `s` is not invoked |
| AC-4.10 | empty | `Or(s)` | the optional returned by `s()` |
| AC-4.11 | any state | `OrElseGet`/`OrElseThrow`/`Or` with a `null` delegate | throws `ArgumentNullException` |
| AC-4.12 | empty | `OrElse(null)` | returns `null` (bridge to nullable code; see §8) |

### US-5 — Run side effects

| ID | Given | When | Then |
|---|---|---|---|
| AC-5.1 | present `v` | `IfPresent(a)` | `a(v)` runs once |
| AC-5.2 | empty | `IfPresent(a)` | `a` is not invoked |
| AC-5.3 | present `v` | `IfPresentOrElse(a, e)` | `a(v)` runs; `e` does not |
| AC-5.4 | empty | `IfPresentOrElse(a, e)` | `e()` runs; `a` does not |
| AC-5.5 | any state | any `null` delegate | throws `ArgumentNullException` |

### US-6 — .NET idioms *(additions to Java)*

| ID | Given | When | Then |
|---|---|---|---|
| AC-6.1 | present `v` | `Match(some, none)` | returns `some(v)`; `none` is not invoked |
| AC-6.2 | empty | `Match(some, none)` | returns `none()`; `some` is not invoked |
| AC-6.3 | present `v` | `TryGetValue(out x)` | returns `true`, `x == v` |
| AC-6.4 | empty | `TryGetValue(out x)` | returns `false`, `x` is `default` |
| AC-6.5 | present / empty | `AsEnumerable()` | a sequence with one element / no elements |
| AC-6.6 | two optionals | LINQ query syntax `from a in x from b in y where … select …` | behaves like the equivalent `FlatMap`/`Filter`/`Map` chain |
| AC-6.7 | any | `Select`, `Where`, `SelectMany` | identical results to `Map`, `Filter`, `FlatMap` |

### US-7 — Equality and text

| ID | Given | When | Then |
|---|---|---|---|
| AC-7.1 | two empties of the same `T` | `Equals` / `==` | `true` |
| AC-7.2 | two presents with equal values (default equality comparer) | `Equals` / `==` | `true` |
| AC-7.3 | present vs empty, or different values | `Equals` / `==` | `false`; `!=` is `true` |
| AC-7.4 | equal optionals | `GetHashCode()` | equal hash codes |
| AC-7.5 | present `v` / empty | `ToString()` | `Optional[v]` / `Optional.empty` (same as Java) |

---

## 5. Deliberate deviations from Java

| Java behaviour | Ours | Why |
|---|---|---|
| `get()` exists | Omitted; use `OrElseThrow()` | ADR 0002 |
| Some methods accept a `null` delegate when empty (e.g. `ifPresent`) | Always throws `ArgumentNullException` | Constitution II — fail fast |
| `Optional` reference can be `null` | Impossible (`struct`) | ADR 0001 |
| `stream()` | `AsEnumerable()` | ADR 0002 |
| `isPresent()` / `isEmpty()` methods | Properties | .NET convention |
| `NoSuchElementException` | `InvalidOperationException` | Standard .NET exception for invalid state |

## 6. Out of scope (future specs)

- Async variants (`MapAsync`, `FlatMapAsync`, `IfPresentAsync`, …) — candidate future spec.
- Collection helpers (`FirstOrEmpty`, `SingleOrEmpty`, `dictionary.GetOptional(key)`) — candidate future spec.
- Serialization (System.Text.Json converter) — separate package.
- Implicit conversion `T → Optional<T>` — intentionally excluded until a spec proves its value
  (it hides the null-check that `Of`/`OfNullable` make explicit).

## 7. Non-functional requirements

- **NFR-1** Zero runtime dependencies (constitution VI).
- **NFR-2** No heap allocation for creating, chaining or extracting optionals, except when the
  caller's own delegates allocate. Validated with a benchmark in `plan.md`.
- **NFR-3** Thread-safe by construction (immutable).
- **NFR-4** All quality gates in the constitution apply.

## 8. Resolved questions

Decided by the owner on 2026-11-07. They may be revisited in a future spec.

1. **`Map` to a nullable value type** (mapper returning e.g. `int?`) — **deferred to v1.1**
   (YAGNI). In v1 the mapper's result type must be non-nullable; use `FlatMap` with `ToOptional()`
   for nullable value types.
2. **`OrElse(null)`** — **allowed.** `OrElse` accepts `null` and returns `T?`, as a bridge back to
   nullable-aware code (AC-4.12).
3. **Custom equality** — **not exposed in v1.** Only the default equality comparer is used.

## 9. Review checklist

- [x] Every Java 8–11 `Optional` member is either covered or listed in §5 with a reason
- [x] Every acceptance criterion is testable as written
- [x] Nothing in this spec prescribes implementation details
- [x] Out-of-scope items are acceptable for v1
- [x] Open questions answered or deferred
