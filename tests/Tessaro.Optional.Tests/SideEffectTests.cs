namespace Tessaro.Tests;

// US-5 — Run side effects.
public class SideEffectTests
{
    private static void NotInvoked<TArg>(TArg _) => throw new InvalidOperationException("The action must not be invoked.");

    private static void NotInvoked() => throw new InvalidOperationException("The action must not be invoked.");

    [Fact]
    [Trait("AC", "AC-5.1")]
    public void IfPresent_Present_InvokesActionOnceWithValue()
    {
        var received = new List<string>();

        Optional.Of("value").IfPresent(received.Add);

        received.ShouldBe(["value"]);
    }

    [Fact]
    [Trait("AC", "AC-5.2")]
    public void IfPresent_Empty_DoesNotInvokeAction() =>
        Should.NotThrow(() => Optional.Empty<string>().IfPresent(NotInvoked));

    [Fact]
    [Trait("AC", "AC-5.3")]
    public void IfPresentOrElse_Present_InvokesOnlyTheValueAction()
    {
        var received = new List<int>();

        Optional.Of(42).IfPresentOrElse(received.Add, NotInvoked);

        received.ShouldBe([42]);
    }

    [Fact]
    [Trait("AC", "AC-5.4")]
    public void IfPresentOrElse_Empty_InvokesOnlyTheEmptyAction()
    {
        var emptyCalls = 0;

        Optional.Empty<int>().IfPresentOrElse(NotInvoked, () => emptyCalls++);

        emptyCalls.ShouldBe(1);
    }

    [Theory]
    [Trait("AC", "AC-5.5")]
    [InlineData(true)]
    [InlineData(false)]
    public void NullDelegate_InAnyState_ThrowsArgumentNullException(bool present)
    {
        var optional = present ? Optional.Of("value") : Optional.Empty<string>();

        Should.Throw<ArgumentNullException>(() => optional.IfPresent(null!)).ParamName.ShouldBe("action");
        Should.Throw<ArgumentNullException>(() => optional.IfPresentOrElse(null!, () => { })).ParamName.ShouldBe("action");
        Should.Throw<ArgumentNullException>(() => optional.IfPresentOrElse(_ => { }, null!)).ParamName.ShouldBe("emptyAction");
    }
}
