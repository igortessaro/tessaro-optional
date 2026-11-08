namespace Tessaro.Tests;

// US-6 — .NET idioms (additions to Java).
public class IdiomTests
{
    private static TResult NotInvoked<TResult>() =>
        throw new InvalidOperationException("The delegate must not be invoked.");

    private static TResult NotInvoked<TArg, TResult>(TArg _) =>
        throw new InvalidOperationException("The delegate must not be invoked.");

    [Fact]
    [Trait("AC", "AC-6.1")]
    public void Match_Present_ReturnsSomeResultWithoutInvokingNone() =>
        Optional.Of(21).Match(n => n * 2, NotInvoked<int>).ShouldBe(42);

    [Fact]
    [Trait("AC", "AC-6.2")]
    public void Match_Empty_ReturnsNoneResultWithoutInvokingSome() =>
        Optional.Empty<int>().Match(NotInvoked<int, string>, () => "none").ShouldBe("none");

    [Theory]
    [Trait("AC", "AC-6.1")]
    [Trait("AC", "AC-6.2")]
    [InlineData(true)]
    [InlineData(false)]
    public void Match_NullDelegate_InAnyState_ThrowsArgumentNullException(bool present)
    {
        var optional = present ? Optional.Of(1) : Optional.Empty<int>();

        Should.Throw<ArgumentNullException>(() => optional.Match(null!, () => 0)).ParamName.ShouldBe("some");
        Should.Throw<ArgumentNullException>(() => optional.Match(n => n, null!)).ParamName.ShouldBe("none");
    }

    [Fact]
    [Trait("AC", "AC-6.3")]
    public void TryGetValue_Present_ReturnsTrueAndTheValue()
    {
        Optional.Of("value").TryGetValue(out var value).ShouldBeTrue();

        value.ShouldBe("value");
    }

    [Fact]
    [Trait("AC", "AC-6.4")]
    public void TryGetValue_Empty_ReturnsFalseAndDefault()
    {
        Optional.Empty<string>().TryGetValue(out var reference).ShouldBeFalse();
        Optional.Empty<int>().TryGetValue(out var number).ShouldBeFalse();

        reference.ShouldBeNull();
        number.ShouldBe(0);
    }

    [Fact]
    [Trait("AC", "AC-6.5")]
    public void AsEnumerable_Present_YieldsTheValueOnce() =>
        Optional.Of(42).AsEnumerable().ShouldBe([42]);

    [Fact]
    [Trait("AC", "AC-6.5")]
    public void AsEnumerable_Empty_YieldsNothing() =>
        Optional.Empty<int>().AsEnumerable().ShouldBeEmpty();

    [Fact]
    [Trait("AC", "AC-6.6")]
    public void QuerySyntax_AllPresent_BehavesLikeTheEquivalentChain()
    {
        var query = from a in Optional.Of(2)
                    from b in Optional.Of(3)
                    where a < b
                    select a * b;

        var chain = Optional.Of(2).FlatMap(a => Optional.Of(3).Filter(b => a < b).Map(b => a * b));

        query.ShouldBe(Optional.Of(6));
        query.ShouldBe(chain);
    }

    [Fact]
    [Trait("AC", "AC-6.6")]
    public void QuerySyntax_AnySourceEmpty_IsEmpty()
    {
        var firstEmpty = from a in Optional.Empty<int>() from b in Optional.Of(3) select a + b;
        var secondEmpty = from a in Optional.Of(2) from b in Optional.Empty<int>() select a + b;

        firstEmpty.IsEmpty.ShouldBeTrue();
        secondEmpty.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-6.6")]
    public void QuerySyntax_WhereFalse_IsEmpty()
    {
        var query = from a in Optional.Of(5) where a < 0 select a;

        query.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-6.6")]
    public void SelectMany_ResultSelectorReturnsNull_IsEmpty()
    {
        var query = from a in Optional.Of("a") from b in Optional.Of("b") select (string?)null;

        query.IsEmpty.ShouldBeTrue();
    }

    [Theory]
    [Trait("AC", "AC-6.7")]
    [InlineData(true)]
    [InlineData(false)]
    public void Aliases_ReturnTheSameAsTheirPrimaryMembers(bool present)
    {
        var optional = present ? Optional.Of(10) : Optional.Empty<int>();

        optional.Select(n => n + 1).ShouldBe(optional.Map(n => n + 1));
        optional.Where(n => n > 5).ShouldBe(optional.Filter(n => n > 5));
        optional.Where(n => n < 5).ShouldBe(optional.Filter(n => n < 5));
        optional.SelectMany(n => Optional.Of(n * 2)).ShouldBe(optional.FlatMap(n => Optional.Of(n * 2)));
    }

    [Theory]
    [Trait("AC", "AC-6.7")]
    [InlineData(true)]
    [InlineData(false)]
    public void Aliases_NullDelegate_InAnyState_ThrowsArgumentNullException(bool present)
    {
        var optional = present ? Optional.Of(1) : Optional.Empty<int>();

        Should.Throw<ArgumentNullException>(() => optional.Select<int>(null!)).ParamName.ShouldBe("selector");
        Should.Throw<ArgumentNullException>(() => optional.Where(null!)).ParamName.ShouldBe("predicate");
        Should.Throw<ArgumentNullException>(() => optional.SelectMany<int>(null!)).ParamName.ShouldBe("selector");
        Should.Throw<ArgumentNullException>(() => optional.SelectMany<int, int>(null!, (a, b) => a + b)).ParamName.ShouldBe("selector");
        Should.Throw<ArgumentNullException>(() => optional.SelectMany<int, int>(Optional.Of, null!)).ParamName.ShouldBe("resultSelector");
    }
}
