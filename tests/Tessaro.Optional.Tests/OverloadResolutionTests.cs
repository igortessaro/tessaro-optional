namespace Tessaro.Tests;

// Plan 001, risk R1: OfNullable/ToOptional have a `T? where T : notnull` and a `T? where T : struct`
// overload. These tests pin the intended binding: each local is explicitly typed, so a wrong
// binding fails to compile, and the assertion checks the runtime behaviour of the chosen overload.
public class OverloadResolutionTests
{
    [Fact]
    [Trait("AC", "AC-1.5")]
    public void OfNullable_NullableReference_BindsToReferenceOverload()
    {
        string? value = "value";

        Optional<string> optional = Optional.OfNullable(value);

        optional.IsPresent.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.6")]
    public void OfNullable_NullableStruct_BindsToStructOverloadAndUnwraps()
    {
        int? value = null;

        Optional<int> optional = Optional.OfNullable(value);

        optional.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.5")]
    public void OfNullable_NonNullableStruct_IsAlwaysPresent()
    {
        var value = 0;

        Optional<int> optional = Optional.OfNullable(value);

        optional.IsPresent.ShouldBeTrue();
    }

    [Fact]
    [Trait("AC", "AC-1.6")]
    public void OfNullable_FromUnconstrainedGenericContext_BindsToReferenceOverload() =>
        Wrap<string>(null).IsEmpty.ShouldBeTrue();

    [Fact]
    [Trait("AC", "AC-1.7")]
    public void ToOptional_NullableStruct_BindsToStructOverloadAndUnwraps()
    {
        int? value = 42;

        Optional<int> optional = value.ToOptional();

        optional.IsPresent.ShouldBeTrue();
    }

    private static Optional<T> Wrap<T>(T? value) where T : notnull => Optional.OfNullable(value);
}
