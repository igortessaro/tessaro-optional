namespace Tessaro.Tests;

// US-2 — Inspect state.
public class StateTests
{
    [Fact]
    [Trait("AC", "AC-2.1")]
    public void Present_IsPresentTrueAndIsEmptyFalse()
    {
        var optional = Optional.Of("value");

        optional.IsPresent.ShouldBeTrue();
        optional.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    [Trait("AC", "AC-2.2")]
    public void Empty_IsPresentFalseAndIsEmptyTrue()
    {
        var optional = Optional.Empty<string>();

        optional.IsPresent.ShouldBeFalse();
        optional.IsEmpty.ShouldBeTrue();
    }
}
