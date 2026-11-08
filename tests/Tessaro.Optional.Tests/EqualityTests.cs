using System.Globalization;

namespace Tessaro.Tests;

// US-7 — Equality and text. Also completes the value/equality parts of US-1 deferred from T03.
public class EqualityTests
{
    [Fact]
    [Trait("AC", "AC-7.1")]
    public void Equals_TwoEmptyReferenceOptionals_AreEqual()
    {
        var left = Optional.Empty<string>();
        var right = Optional.Empty<string>();

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
    }

    [Fact]
    [Trait("AC", "AC-7.1")]
    public void Equals_TwoEmptyValueOptionals_AreEqual() =>
        (Optional.Empty<int>() == Optional.Empty<int>()).ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-1.2")]
    public void Default_EqualsEmpty()
    {
        (default(Optional<string>) == Optional.Empty<string>()).ShouldBeTrue();
        (default(Optional<int>) == Optional.Empty<int>()).ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-7.2")]
    public void Equals_PresentWithEqualReferenceValues_AreEqual()
    {
        var left = Optional.Of("value");
        var right = Optional.Of(new string("value".AsSpan())); // equal, but a different instance

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-7.2")]
    public void Equals_PresentWithEqualValueTypes_AreEqual() =>
        (Optional.Of(42) == Optional.Of(42)).ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-7.3")]
    public void Equals_PresentAndEmpty_AreNotEqual()
    {
        var present = Optional.Of("value");
        var empty = Optional.Empty<string>();

        present.Equals(empty).ShouldBeFalse();
        empty.Equals(present).ShouldBeFalse();
        (present == empty).ShouldBeFalse();
        (present != empty).ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-7.3")]
    public void Equals_PresentWithDifferentValues_AreNotEqual()
    {
        (Optional.Of("a") == Optional.Of("b")).ShouldBeFalse();
        (Optional.Of(1) != Optional.Of(2)).ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-7.3")]
    public void PresentWithDefaultValueTypeValue_IsNotEqualToEmpty() =>
        (Optional.Of(0) == Optional.Empty<int>()).ShouldBeFalse();

    [Fact]
    [Trait("AC", "AC-7.2")]
    public void EqualsObject_BoxedEqualOptional_IsTrue() =>
        Optional.Of(42).Equals((object)Optional.Of(42)).ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-7.3")]
    public void EqualsObject_NullOrOtherType_IsFalse()
    {
        var optional = Optional.Of(42);

        optional.Equals(null).ShouldBeFalse();
        optional.Equals(42).ShouldBeFalse();
        optional.Equals(Optional.Of(42L)).ShouldBeFalse();
    }

    [Fact]
    [Trait("AC", "AC-7.4")]
    public void GetHashCode_EqualOptionals_HaveEqualHashCodes()
    {
        Optional.Of("value").GetHashCode().ShouldBe(Optional.Of(new string("value".AsSpan())).GetHashCode());
        Optional.Of(42).GetHashCode().ShouldBe(Optional.Of(42).GetHashCode());
        Optional.Empty<string>().GetHashCode().ShouldBe(default(Optional<string>).GetHashCode());
    }

    [Fact]
    [Trait("AC", "AC-7.4")]
    public void GetHashCode_Empty_IsZero() => Optional.Empty<string>().GetHashCode().ShouldBe(0);

    [Fact]
    [Trait("AC", "AC-7.5")]
    public void ToString_Present_IsOptionalWithValue()
    {
        Optional.Of("value").ToString().ShouldBe("Optional[value]");
        Optional.Of(42).ToString().ShouldBe("Optional[42]");
    }

    [Fact]
    [Trait("AC", "AC-7.5")]
    public void ToString_Empty_IsOptionalEmpty()
    {
        Optional.Empty<string>().ToString().ShouldBe("Optional.empty");
        default(Optional<int>).ToString().ShouldBe("Optional.empty");
    }

    [Fact]
    [Trait("AC", "AC-7.5")]
    public void ToString_IsCultureInvariant()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");

            Optional.Of(1.5).ToString().ShouldBe("Optional[1.5]");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    // Value parts of US-1 deferred from T03: the optional holds exactly the value it was created with.

    [Fact]
    [Trait("AC", "AC-1.3")]
    public void Of_HoldsTheGivenValue()
    {
        (Optional.Of("value") == Optional.Of("value")).ShouldBeTrue();
        (Optional.Of(42) == Optional.Of(43)).ShouldBeFalse();
    }

    [Fact]
    [Trait("AC", "AC-1.5")]
    public void OfNullable_WithValue_HoldsTheSameValueAsOf()
    {
        string? reference = "value";
        int? number = 42;

        (Optional.OfNullable(reference) == Optional.Of("value")).ShouldBeTrue();
        (Optional.OfNullable(number) == Optional.Of(42)).ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.7")]
    public void ToOptional_WithValue_HoldsTheSameValueAsOf()
    {
        string? reference = "value";
        int? number = 42;

        (reference.ToOptional() == Optional.Of("value")).ShouldBeTrue();
        (number.ToOptional() == Optional.Of(42)).ShouldBeTrue();
    }
}
