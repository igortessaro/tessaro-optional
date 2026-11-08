namespace Tessaro.Tests;

// US-3 — Transform without `if`.
public class TransformationTests
{
    // A delegate that fails the test if the code under test ever calls it.
    private static TResult NotInvoked<TArg, TResult>(TArg _) =>
        throw new InvalidOperationException("The delegate must not be invoked.");

    [Fact]
    [Trait("AC", "AC-3.1")]
    public void Map_Present_ReturnsPresentWithMappedValue()
    {
        Optional.Of("value").Map(s => s.Length).ShouldBe(Optional.Of(5));
        Optional.Of(42).Map(n => n.ToString(System.Globalization.CultureInfo.InvariantCulture)).ShouldBe(Optional.Of("42"));
    }

    [Fact]
    [Trait("AC", "AC-3.1")]
    public void Map_PresentToDefaultValueType_IsPresent() =>
        Optional.Of("value").Map(_ => 0).ShouldBe(Optional.Of(0));

    [Fact]
    [Trait("AC", "AC-3.2")]
    public void Map_MapperReturnsNull_ReturnsEmpty() =>
        Optional.Of("value").Map(_ => (string?)null).IsEmpty.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-3.3")]
    public void Map_Empty_ReturnsEmptyWithoutInvokingMapper() =>
        Optional.Empty<string>().Map(NotInvoked<string, int>).IsEmpty.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-3.4")]
    public void FlatMap_Present_ReturnsTheMapperResult()
    {
        Optional.Of("42").FlatMap(s => Optional.Of(s.Length)).ShouldBe(Optional.Of(2));
        Optional.Of("value").FlatMap(_ => Optional.Empty<int>()).IsEmpty.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-3.5")]
    public void FlatMap_Empty_ReturnsEmptyWithoutInvokingMapper() =>
        Optional.Empty<string>().FlatMap(NotInvoked<string, Optional<int>>).IsEmpty.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-3.6")]
    public void Filter_PresentAndPredicateTrue_ReturnsSameOptional()
    {
        var optional = Optional.Of("value");

        optional.Filter(s => s.Length == 5).ShouldBe(optional);
    }

    [Fact]
    [Trait("AC", "AC-3.7")]
    public void Filter_PresentAndPredicateFalse_ReturnsEmpty() =>
        Optional.Of(42).Filter(n => n < 0).IsEmpty.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-3.8")]
    public void Filter_Empty_ReturnsEmptyWithoutInvokingPredicate() =>
        Optional.Empty<int>().Filter(NotInvoked<int, bool>).IsEmpty.ShouldBeTrue();

    [Theory]
    [Trait("AC", "AC-3.9")]
    [InlineData(true)]
    [InlineData(false)]
    public void NullDelegate_InAnyState_ThrowsArgumentNullException(bool present)
    {
        var optional = present ? Optional.Of("value") : Optional.Empty<string>();

        Should.Throw<ArgumentNullException>(() => optional.Map<int>(null!)).ParamName.ShouldBe("mapper");
        Should.Throw<ArgumentNullException>(() => optional.FlatMap<int>(null!)).ParamName.ShouldBe("mapper");
        Should.Throw<ArgumentNullException>(() => optional.Filter(null!)).ParamName.ShouldBe("predicate");
    }

    [Fact]
    [Trait("AC", "AC-3.1")]
    [Trait("AC", "AC-3.4")]
    [Trait("AC", "AC-3.6")]
    public void Chain_LikeTheSpecExample_SkipsStepsOnceEmpty()
    {
        var user = new User(new Address("Lisbon"));
        var homeless = new User(null);

        City(user).ShouldBe(Optional.Of("Lisbon"));
        City(homeless).IsEmpty.ShouldBeTrue();

        static Optional<string> City(User u) => Optional.Of(u)
            .FlatMap(x => x.Address.ToOptional())
            .Map(a => a.City)
            .Filter(c => c.Length > 0);
    }

    private sealed record Address(string City);

    private sealed record User(Address? Address);
}
