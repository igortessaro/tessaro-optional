using System.Diagnostics.CodeAnalysis;

namespace Tessaro.Tests;

// NFR-2 — no heap allocation for creating, chaining or extracting optionals.
// Every lambda is `static` (cached by the compiler), so any allocation measured comes from the library.
// Excluded by design: AsEnumerable (an exit to collections), ToString (returns a new string) and
// exception paths (plan 001 §3.4).
public class AllocationTests
{
    private const int Iterations = 1_000;

    // Keeps the JIT from discarding results: written on purpose, never read.
    [SuppressMessage("Style", "IDE0052:Remove unread private member", Justification = "Write-only sink that keeps the JIT from discarding results.")]
    private static long _sink;

    private static long AllocatedBytes(Action scenario)
    {
        scenario(); // warm-up: JIT compilation and static initialisation happen here, not in the measurement

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Iterations; i++)
        {
            scenario();
        }

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact]
    [Trait("NFR", "NFR-2")]
    public void Create_DoesNotAllocate() =>
        AllocatedBytes(static () =>
        {
            _sink += Optional.Of(42).IsPresent ? 1 : 0;
            _sink += Optional.Of("value").IsPresent ? 1 : 0;
            _sink += Optional.OfNullable((int?)42).IsPresent ? 1 : 0;
            _sink += Optional.OfNullable((string?)null).IsEmpty ? 1 : 0;
            _sink += ((int?)7).ToOptional().IsPresent ? 1 : 0;
            _sink += Optional.Empty<string>().IsEmpty ? 1 : 0;
        }).ShouldBe(0);

    [Fact]
    [Trait("NFR", "NFR-2")]
    public void Chain_ValueType_DoesNotAllocate() =>
        AllocatedBytes(static () =>
            _sink += Optional.Of(21)
                .Map(static n => n * 2)
                .Filter(static n => n > 0)
                .FlatMap(static n => Optional.Of(n + 1))
                .Or(static () => Optional.Of(0))
                .OrElse(-1)).ShouldBe(0);

    [Fact]
    [Trait("NFR", "NFR-2")]
    public void Chain_ReferenceType_DoesNotAllocate() =>
        AllocatedBytes(static () =>
            _sink += Optional.Of("value")
                .Filter(static s => s.Length > 0)
                .Map(static s => s.Length)
                .OrElseGet(static () => 0)).ShouldBe(0);

    [Fact]
    [Trait("NFR", "NFR-2")]
    public void Chain_EmptyPath_DoesNotAllocate() =>
        AllocatedBytes(static () =>
            _sink += Optional.Empty<string>()
                .Map(static s => s.Length)
                .Filter(static n => n > 0)
                .FlatMap(static n => Optional.Of(n))
                .OrElse(-1)).ShouldBe(0);

    [Fact]
    [Trait("NFR", "NFR-2")]
    public void Extract_DoesNotAllocate() =>
        AllocatedBytes(static () =>
        {
            var present = Optional.Of(42);
            _sink += present.OrElseThrow();
            _sink += present.OrElseThrow(static () => new InvalidOperationException());
            _sink += present.Match(static n => n, static () => 0);
            _sink += present.TryGetValue(out var value) ? value : 0;
            _sink += Optional.Empty<int>().Match(static n => n, static () => 1);
        }).ShouldBe(0);

    [Fact]
    [Trait("NFR", "NFR-2")]
    public void SideEffects_DoNotAllocate() =>
        AllocatedBytes(static () =>
        {
            Optional.Of(1).IfPresent(static n => _sink += n);
            Optional.Empty<int>().IfPresentOrElse(static n => _sink += n, static () => _sink++);
        }).ShouldBe(0);

    // Query syntax is allocation-free as long as the C# translation needs no "transparent identifier".
    // A clause between two `from`s (e.g. `from a … from b … where … select …`) makes the compiler pass
    // `(a, b) => new { a, b }` to SelectMany: that anonymous object (24 bytes, measured 2026-11-08) is the
    // language's cost, not the library's, and IEnumerable LINQ pays it too. The method-syntax test below
    // proves the same pipeline through the library itself allocates nothing (plan 001 §3.4).

    [Fact]
    [Trait("NFR", "NFR-2")]
    public void LinqQuerySyntax_TwoFromsAndSelect_DoesNotAllocate() =>
        AllocatedBytes(static () =>
        {
            var query = from a in Optional.Of(2)
                        from b in Optional.Of(3)
                        select a * b;
            _sink += query.OrElse(0);
        }).ShouldBe(0);

    [Fact]
    [Trait("NFR", "NFR-2")]
    public void LinqQuerySyntax_FromWhereSelect_DoesNotAllocate() =>
        AllocatedBytes(static () =>
        {
            var query = from a in Optional.Of(2)
                        where a > 0
                        select a * 2;
            _sink += query.OrElse(0);
        }).ShouldBe(0);

    [Fact]
    [Trait("NFR", "NFR-2")]
    public void LinqMethodSyntax_SelectManyWhereSelect_DoesNotAllocate() =>
        AllocatedBytes(static () =>
            _sink += Optional.Of(2)
                .SelectMany(static _ => Optional.Of(3), static (a, b) => (a, b))
                .Where(static t => t.a < t.b)
                .Select(static t => t.a * t.b)
                .OrElse(0)).ShouldBe(0);

    [Fact]
    [Trait("NFR", "NFR-2")]
    public void Equality_DoesNotAllocate() =>
        AllocatedBytes(static () =>
        {
            _sink += Optional.Of(1) == Optional.Of(1) ? 1 : 0;
            _sink += Optional.Of("a") != Optional.Empty<string>() ? 1 : 0;
            _sink += Optional.Of(1).Equals(Optional.Of(2)) ? 1 : 0;
            _sink += Optional.Of("a").GetHashCode();
        }).ShouldBe(0);
}
