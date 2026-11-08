namespace Tessaro.Tests;

// US-4 — Extract a value with a fallback.
public class ExtractionTests
{
    private static TResult NotInvoked<TResult>() =>
        throw new InvalidOperationException("The delegate must not be invoked.");

    [Fact]
    [Trait("AC", "AC-4.1")]
    public void OrElse_Present_ReturnsValue()
    {
        Optional.Of("value").OrElse("other").ShouldBe("value");
        Optional.Of(42).OrElse(0).ShouldBe(42);
    }

    [Fact]
    [Trait("AC", "AC-4.2")]
    public void OrElse_Empty_ReturnsOther()
    {
        Optional.Empty<string>().OrElse("other").ShouldBe("other");
        Optional.Empty<int>().OrElse(-1).ShouldBe(-1);
    }

    [Fact]
    [Trait("AC", "AC-4.12")]
    public void OrElse_EmptyWithNullFallback_ReturnsNull() =>
        Optional.Empty<string>().OrElse(null).ShouldBeNull();

    [Fact]
    [Trait("AC", "AC-4.12")]
    public void OrElse_PresentWithNullFallback_ReturnsValue() =>
        Optional.Of("value").OrElse(null).ShouldBe("value");

    [Fact]
    [Trait("AC", "AC-4.3")]
    public void OrElseGet_Present_ReturnsValueWithoutInvokingSupplier() =>
        Optional.Of("value").OrElseGet(NotInvoked<string>).ShouldBe("value");

    [Fact]
    [Trait("AC", "AC-4.4")]
    public void OrElseGet_Empty_ReturnsSupplierResult() =>
        Optional.Empty<int>().OrElseGet(() => 7).ShouldBe(7);

    [Fact]
    [Trait("AC", "AC-4.5")]
    public void OrElseThrow_Present_ReturnsValue() => Optional.Of("value").OrElseThrow().ShouldBe("value");

    [Fact]
    [Trait("AC", "AC-4.6")]
    public void OrElseThrow_Empty_ThrowsInvalidOperationException() =>
        Should.Throw<InvalidOperationException>(() => Optional.Empty<string>().OrElseThrow())
            .Message.ShouldBe("No value present");

    [Fact]
    [Trait("AC", "AC-4.7")]
    public void OrElseThrowWithFactory_Present_ReturnsValueWithoutInvokingFactory() =>
        Optional.Of(42).OrElseThrow(NotInvoked<KeyNotFoundException>).ShouldBe(42);

    [Fact]
    [Trait("AC", "AC-4.8")]
    public void OrElseThrowWithFactory_Empty_ThrowsTheFactoryException()
    {
        var expected = new KeyNotFoundException("user 42");

        var thrown = Should.Throw<KeyNotFoundException>(() => Optional.Empty<string>().OrElseThrow(() => expected));

        thrown.ShouldBeSameAs(expected);
    }

    [Fact]
    [Trait("AC", "AC-4.8")]
    public void OrElseThrowWithFactory_FactoryReturnsNull_ThrowsInvalidOperationException() =>
        Should.Throw<InvalidOperationException>(() => Optional.Empty<string>().OrElseThrow<KeyNotFoundException>(() => null!))
            .Message.ShouldBe("The exception factory returned null.");

    [Fact]
    [Trait("AC", "AC-4.9")]
    public void Or_Present_ReturnsSameOptionalWithoutInvokingSupplier()
    {
        var optional = Optional.Of("value");

        optional.Or(NotInvoked<Optional<string>>).ShouldBe(optional);
    }

    [Fact]
    [Trait("AC", "AC-4.10")]
    public void Or_Empty_ReturnsSupplierOptional()
    {
        Optional.Empty<string>().Or(() => Optional.Of("fallback")).ShouldBe(Optional.Of("fallback"));
        Optional.Empty<string>().Or(Optional.Empty<string>).IsEmpty.ShouldBeTrue();
    }

    [Theory]
    [Trait("AC", "AC-4.11")]
    [InlineData(true)]
    [InlineData(false)]
    public void NullDelegate_InAnyState_ThrowsArgumentNullException(bool present)
    {
        var optional = present ? Optional.Of("value") : Optional.Empty<string>();

        Should.Throw<ArgumentNullException>(() => optional.OrElseGet(null!)).ParamName.ShouldBe("supplier");
        Should.Throw<ArgumentNullException>(() => optional.OrElseThrow<Exception>(null!)).ParamName.ShouldBe("exceptionFactory");
        Should.Throw<ArgumentNullException>(() => optional.Or(null!)).ParamName.ShouldBe("supplier");
    }
}
