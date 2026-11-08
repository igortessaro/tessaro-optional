# Tessaro.Optional

> Java's `Optional`, the .NET way — model absence explicitly and replace `if/else` null checks
> with expressive, fluent code.

[![CI](https://github.com/igortessaro/tessaro-optional/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/igortessaro/tessaro-optional/actions/workflows/ci.yml)

```csharp
using Tessaro;

string city = FindUser(id)               // Optional<User>
    .FlatMap(u => u.Address)             // Optional<Address>
    .Map(a => a.City)                    // Optional<string>
    .Filter(c => c.Length > 0)
    .OrElse("Unknown");
```

- The full Java 8–11 `Optional` API, with .NET naming
- `readonly struct`: an optional is never `null`, and creating, chaining and extracting allocate nothing
- LINQ query syntax (`from … in … select …`)
- .NET idioms: `Match`, `TryGetValue`, `AsEnumerable`
- Zero runtime dependencies · targets `net8.0` and `net10.0`

## Installation

> **Not on NuGet yet.** Spec 001 is implemented; the release workflow is the next planned step.

Once published:

```bash
dotnet add package Tessaro.Optional
```

Everything lives in the `Tessaro` namespace ([ADR 0004](docs/adr/0004-namespace-tessaro.md)):

```csharp
using Tessaro;
```

## Usage

### Create

```csharp
Optional<string> name = Optional.Of("Igor");       // present; Of(null) throws ArgumentNullException
Optional<string> none = Optional.Empty<string>();  // empty
Optional<string> maybe = Optional.OfNullable(possiblyNull); // empty when null

Optional<int> fromNullable = ((int?)42).ToOptional();       // works for Nullable<T> too
Optional<int> alsoEmpty = default;                          // default is the empty optional
```

### Inspect

```csharp
if (name.IsPresent) { /* … */ }
if (none.IsEmpty) { /* … */ }
```

### Transform

Every step is skipped when the optional is empty, so no `if` is needed.

```csharp
Optional<int> length = name.Map(n => n.Length);              // a null result becomes empty
Optional<Address> address = user.FlatMap(u => u.Address);    // mapper returns an Optional
Optional<string> longName = name.Filter(n => n.Length > 3);
```

### Extract

```csharp
string a = name.OrElse("anonymous");
string b = name.OrElseGet(() => LoadDefaultName());          // supplier runs only when empty
string c = name.OrElseThrow();                                // InvalidOperationException when empty
string d = name.OrElseThrow(() => new UserNotFoundException(id));
Optional<string> e = name.Or(() => FindNickname(id));         // fall back to another optional
string? f = none.OrElse(null);                                // bridge back to nullable code
```

### Side effects

```csharp
name.IfPresent(n => Console.WriteLine($"Hello, {n}"));
name.IfPresentOrElse(
    n => Console.WriteLine($"Hello, {n}"),
    () => Console.WriteLine("Hello, stranger"));
```

### .NET idioms

```csharp
string greeting = name.Match(
    some: n => $"Hello, {n}",
    none: () => "Hello, stranger");

if (name.TryGetValue(out var value))
{
    Console.WriteLine(value);
}

IEnumerable<string> names = name.AsEnumerable();             // zero or one element
```

LINQ query syntax works through `Select`, `Where` and `SelectMany`, which are aliases of `Map`,
`Filter` and `FlatMap`:

```csharp
Optional<int> area =
    from width in ParseInt(widthText)
    from height in ParseInt(heightText)
    where width > 0 && height > 0
    select width * height;
```

### Equality and text

```csharp
Optional.Of(1) == Optional.Of(1);          // true (default equality comparer of T)
Optional.Empty<int>() == default;          // true
Optional.Of(1.5).ToString();               // "Optional[1.5]" (invariant culture)
Optional.Empty<int>().ToString();          // "Optional.empty"
```

## Java ↔ .NET mapping

| Java `java.util.Optional` | Tessaro.Optional | Notes |
|---|---|---|
| `Optional.empty()` | `Optional.Empty<T>()` or `default` | |
| `Optional.of(v)` | `Optional.Of(v)` | `null` throws `ArgumentNullException` |
| `Optional.ofNullable(v)` | `Optional.OfNullable(v)` or `v.ToOptional()` | Also accepts `Nullable<T>` |
| `isPresent()` / `isEmpty()` | `IsPresent` / `IsEmpty` | Properties |
| `get()` | — | Omitted on purpose; use `OrElseThrow()` |
| `map(f)` | `Map(f)` / `Select(f)` | |
| `flatMap(f)` | `FlatMap(f)` / `SelectMany(f)` | |
| `filter(p)` | `Filter(p)` / `Where(p)` | |
| `or(s)` | `Or(s)` | |
| `orElse(o)` | `OrElse(o)` | `o` may be `null` |
| `orElseGet(s)` | `OrElseGet(s)` | |
| `orElseThrow()` | `OrElseThrow()` | Throws `InvalidOperationException` |
| `orElseThrow(s)` | `OrElseThrow(factory)` | |
| `ifPresent(a)` | `IfPresent(a)` | |
| `ifPresentOrElse(a, e)` | `IfPresentOrElse(a, e)` | |
| `stream()` | `AsEnumerable()` | |
| `equals` / `hashCode` / `toString` | `Equals`, `==`, `!=` / `GetHashCode` / `ToString` | Same text format as Java |
| — | `Match(some, none)`, `TryGetValue(out v)` | .NET additions |

Every deliberate difference from Java, and the reason for it, is listed in
[spec 001 §5](specs/001-optional-core/spec.md#5-deliberate-deviations-from-java). In short: `null`
delegates always throw (fail fast), an optional can never be `null`, and there is no `get()`.

## Known limitations

- **`Map` to a nullable value type** (a mapper returning `int?`) is not supported yet; use
  `FlatMap(x => f(x).ToOptional())`. Planned for v1.1 ([spec 001 §8](specs/001-optional-core/spec.md#8-resolved-questions)).
- **Allocations in LINQ query syntax.** A query with a clause between two `from`s makes the C#
  compiler allocate one small anonymous object per evaluation. The library itself does not
  allocate; on hot paths, use method syntax with a tuple
  ([plan 001 §3.4](specs/001-optional-core/plan.md#34-behaviour-details-not-stated-in-the-spec)).
- `AsEnumerable()` and `ToString()` allocate by nature.

## Project docs

This library is built with Spec-Driven Development: every change starts from a written spec.

- [Constitution](docs/constitution.md) — principles and quality gates
- [Architecture decisions](docs/adr/)
- [Spec 001 — Optional core](specs/001-optional-core/spec.md) · [plan](specs/001-optional-core/plan.md) · [tasks](specs/001-optional-core/tasks.md)

## License

[MIT](LICENSE)
