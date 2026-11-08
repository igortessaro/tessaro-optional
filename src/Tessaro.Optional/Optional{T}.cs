namespace Tessaro;

/// <summary>
/// A container that either holds a non-null value of type <typeparamref name="T"/> (it is
/// <em>present</em>) or holds nothing (it is <em>empty</em>).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Optional{T}"/> is a value type, so an optional can never be <see langword="null"/>:
/// <c>default(Optional&lt;T&gt;)</c> is the empty optional.
/// </para>
/// <para>Create instances with <see cref="Optional"/> or <see cref="OptionalExtensions"/>.</para>
/// </remarks>
/// <typeparam name="T">The type of the value. It cannot be a nullable type.</typeparam>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "The Java-compatible name is the purpose of this library (ADR 0004). VB.NET callers can escape it as [Optional].")]
public readonly partial struct Optional<T> where T : notnull
{
    private readonly T _value;

    internal Optional(T value)
    {
        _value = value;
        IsPresent = true;
    }

    /// <summary>Gets a value indicating whether this optional holds a value.</summary>
    public bool IsPresent { get; }

    /// <summary>Gets a value indicating whether this optional holds no value.</summary>
    public bool IsEmpty => !IsPresent;
}
