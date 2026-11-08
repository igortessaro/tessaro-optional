namespace Tessaro;

// LINQ aliases (ADR 0002): they enable query syntax and delegate to the primary members.
// Where a parameter name differs from the primary member, the alias validates it first so the
// exception names the parameter the caller passed.
public readonly partial struct Optional<T>
{
    /// <summary>LINQ alias of <see cref="Map{TResult}(Func{T, TResult})"/>.</summary>
    /// <typeparam name="TResult">The type of the projected value.</typeparam>
    /// <param name="selector">The projection to apply to the value. Not invoked when empty.</param>
    /// <returns>An optional holding the projected value, or an empty optional.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
    public Optional<TResult> Select<TResult>(Func<T, TResult?> selector) where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        return Map(selector);
    }

    /// <summary>LINQ alias of <see cref="Filter(Func{T, bool})"/>.</summary>
    /// <param name="predicate">The condition the value must satisfy. Not invoked when empty.</param>
    /// <returns>This optional if it is present and the value matches; otherwise an empty optional.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    public Optional<T> Where(Func<T, bool> predicate) => Filter(predicate);

    /// <summary>LINQ alias of <see cref="FlatMap{TResult}(Func{T, Optional{TResult}})"/>.</summary>
    /// <typeparam name="TResult">The type of the value of the returned optional.</typeparam>
    /// <param name="selector">The function to apply to the value. Not invoked when empty.</param>
    /// <returns>The optional returned by <paramref name="selector"/>, or an empty optional.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
    public Optional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> selector) where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        return FlatMap(selector);
    }

    /// <summary>
    /// Binds to a second optional and combines both values; enables LINQ query syntax with multiple
    /// <c>from</c> clauses. A <see langword="null"/> combined result becomes an empty optional.
    /// </summary>
    /// <typeparam name="TIntermediate">The type of the value of the intermediate optional.</typeparam>
    /// <typeparam name="TResult">The type of the combined value.</typeparam>
    /// <param name="selector">Produces the intermediate optional from the value. Not invoked when empty.</param>
    /// <param name="resultSelector">Combines the value and the intermediate value. Invoked only when both are present.</param>
    /// <returns>An optional holding the combined value, or an empty optional.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="selector"/> or <paramref name="resultSelector"/> is <see langword="null"/>.
    /// </exception>
    public Optional<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Optional<TIntermediate>> selector,
        Func<T, TIntermediate, TResult?> resultSelector)
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        if (IsEmpty)
        {
            return default;
        }

        // Written out instead of FlatMap + Map to avoid allocating a closure per call (NFR-2).
        var intermediate = selector(_value);
        return intermediate.IsPresent
            ? Optional.OfNullable(resultSelector(_value, intermediate._value))
            : default;
    }
}
