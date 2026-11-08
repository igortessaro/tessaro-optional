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

    /// <summary>
    /// Applies <paramref name="mapper"/> to the value, if present. A <see langword="null"/> result
    /// becomes an empty optional.
    /// </summary>
    /// <typeparam name="TResult">The type of the mapped value.</typeparam>
    /// <param name="mapper">The function to apply to the value. Not invoked when empty.</param>
    /// <returns>An optional holding the mapped value, or an empty optional.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    public Optional<TResult> Map<TResult>(Func<T, TResult?> mapper) where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return IsPresent ? Optional.OfNullable(mapper(_value)) : default;
    }

    /// <summary>Applies an optional-returning <paramref name="mapper"/> to the value, if present.</summary>
    /// <typeparam name="TResult">The type of the value of the returned optional.</typeparam>
    /// <param name="mapper">The function to apply to the value. Not invoked when empty.</param>
    /// <returns>The optional returned by <paramref name="mapper"/>, or an empty optional.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    public Optional<TResult> FlatMap<TResult>(Func<T, Optional<TResult>> mapper) where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return IsPresent ? mapper(_value) : default;
    }

    /// <summary>Keeps the value only if it matches <paramref name="predicate"/>.</summary>
    /// <param name="predicate">The condition the value must satisfy. Not invoked when empty.</param>
    /// <returns>This optional if it is present and the value matches; otherwise an empty optional.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    public Optional<T> Filter(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return IsPresent && predicate(_value) ? this : default;
    }
}
