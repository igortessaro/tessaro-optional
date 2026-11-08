namespace Tessaro.Tests;

// US-1 — Create optionals.
// "present with v" is asserted through IsPresent here; the value itself is asserted in
// EqualityTests (T04), once equality exists.
public class CreationTests
{
    [Fact]
    [Trait("AC", "AC-1.1")]
    public void Empty_ReferenceType_IsEmpty() => Optional.Empty<string>().IsEmpty.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-1.1")]
    public void Empty_ValueType_IsEmpty() => Optional.Empty<int>().IsEmpty.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-1.2")]
    public void Default_ReferenceType_IsEmpty() => default(Optional<string>).IsEmpty.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-1.2")]
    public void Default_ValueType_IsEmpty() => default(Optional<int>).IsEmpty.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-1.2")]
    public void ParameterlessConstructor_IsEmpty() => new Optional<string>().IsEmpty.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-1.3")]
    public void Of_ReferenceType_IsPresent() => Optional.Of("value").IsPresent.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-1.3")]
    public void Of_ValueType_IsPresent() => Optional.Of(42).IsPresent.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-1.3")]
    public void Of_DefaultValueTypeValue_IsPresent() => Optional.Of(0).IsPresent.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-1.4")]
    public void Of_Null_ThrowsArgumentNullException()
    {
        string value = null!;

        var exception = Should.Throw<ArgumentNullException>(() => Optional.Of(value));

        exception.ParamName.ShouldBe("value");
    }

    [Fact]
    [Trait("AC", "AC-1.5")]
    public void OfNullable_NonNullReference_IsPresent()
    {
        string? value = "value";

        Optional.OfNullable(value).IsPresent.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.5")]
    public void OfNullable_NullableStructWithValue_IsPresent()
    {
        int? value = 42;

        Optional.OfNullable(value).IsPresent.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.6")]
    public void OfNullable_NullReference_IsEmpty()
    {
        string? value = null;

        Optional.OfNullable(value).IsEmpty.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.6")]
    public void OfNullable_NullableStructWithoutValue_IsEmpty()
    {
        int? value = null;

        Optional.OfNullable(value).IsEmpty.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.7")]
    public void ToOptional_NonNullReference_IsPresent()
    {
        string? value = "value";

        value.ToOptional().IsPresent.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.7")]
    public void ToOptional_NullReference_IsEmpty()
    {
        string? value = null;

        value.ToOptional().IsEmpty.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.7")]
    public void ToOptional_NullableStructWithValue_IsPresent()
    {
        int? value = 42;

        value.ToOptional().IsPresent.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.7")]
    public void ToOptional_NullableStructWithoutValue_IsEmpty()
    {
        int? value = null;

        value.ToOptional().IsEmpty.ShouldBeTrue();
    }
}
