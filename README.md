# Tessaro.Optional

> Java's `Optional`, the .NET way — model absence explicitly and replace `if/else` null checks
> with expressive, fluent code.

**Status:** 🚧 design phase — see [spec 001](specs/001-optional-core/spec.md). Not published yet.

```csharp
string city = FindUser(id)               // Optional<User>
    .FlatMap(u => u.Address)             // Optional<Address>
    .Map(a => a.City)                    // Optional<string>
    .Filter(c => c.Length > 0)
    .OrElse("Unknown");
```

## Highlights (planned)

- Full Java 8–11 `Optional` API: `Of`, `OfNullable`, `Empty`, `Map`, `FlatMap`, `Filter`,
  `OrElse`, `OrElseGet`, `OrElseThrow`, `Or`, `IfPresent`, `IfPresentOrElse`, `IsPresent`, `IsEmpty`
- `readonly struct` — an optional can never be `null`, and it doesn't allocate
- LINQ query syntax support (`Select`, `SelectMany`, `Where`)
- .NET idioms: `Match`, `TryGetValue`, `AsEnumerable`
- Zero dependencies

## Project docs

- [Constitution](docs/constitution.md) — principles and quality gates
- [Architecture decisions](docs/adr/)
- [Specs](specs/)

## License

MIT
