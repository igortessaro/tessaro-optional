using System.Diagnostics.CodeAnalysis;

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
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "The Java-compatible name is the purpose of this library (ADR 0004). VB.NET callers can escape it as [Optional].")]
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

    /// <summary>Returns the value if present, otherwise <paramref name="other"/>.</summary>
    /// <remarks>
    /// <paramref name="other"/> may be <see langword="null"/>, which bridges back to nullable-aware code.
    /// </remarks>
    /// <param name="other">The value to return when this optional is empty.</param>
    /// <returns>The value, or <paramref name="other"/>.</returns>
    [return: NotNullIfNotNull(nameof(other))]
    public T? OrElse(T? other) => IsPresent ? _value : other;

    /// <summary>Returns the value if present, otherwise the result of <paramref name="supplier"/>.</summary>
    /// <param name="supplier">Produces the fallback value. Not invoked when present.</param>
    /// <returns>The value, or the supplied fallback.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="supplier"/> is <see langword="null"/>.</exception>
    public T OrElseGet(Func<T> supplier)
    {
        ArgumentNullException.ThrowIfNull(supplier);
        return IsPresent ? _value : supplier();
    }

    /// <summary>Returns the value if present, otherwise throws.</summary>
    /// <returns>The value.</returns>
    /// <exception cref="InvalidOperationException">This optional is empty.</exception>
    public T OrElseThrow() => IsPresent ? _value : throw new InvalidOperationException("No value present");

    /// <summary>Returns the value if present, otherwise throws the exception created by <paramref name="exceptionFactory"/>.</summary>
    /// <typeparam name="TException">The type of the exception to throw.</typeparam>
    /// <param name="exceptionFactory">Creates the exception to throw. Not invoked when present.</param>
    /// <returns>The value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exceptionFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="exceptionFactory"/> returned <see langword="null"/>.</exception>
    public T OrElseThrow<TException>(Func<TException> exceptionFactory) where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(exceptionFactory);
        return IsPresent
            ? _value
            : throw (Exception?)exceptionFactory() ?? new InvalidOperationException("The exception factory returned null.");
    }

    /// <summary>Returns this optional if present, otherwise the optional produced by <paramref name="supplier"/>.</summary>
    /// <param name="supplier">Produces the fallback optional. Not invoked when present.</param>
    /// <returns>This optional, or the supplied one.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="supplier"/> is <see langword="null"/>.</exception>
    public Optional<T> Or(Func<Optional<T>> supplier)
    {
        ArgumentNullException.ThrowIfNull(supplier);
        return IsPresent ? this : supplier();
    }
}
