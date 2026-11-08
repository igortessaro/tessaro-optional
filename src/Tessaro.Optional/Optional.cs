namespace Tessaro;

/// <summary>Creates <see cref="Optional{T}"/> instances.</summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "The Java-compatible name is the purpose of this library (ADR 0004). VB.NET callers can escape it as [Optional].")]
public static class Optional
{
    /// <summary>Returns an empty optional.</summary>
    /// <typeparam name="T">The type of the value the optional could hold.</typeparam>
    /// <returns>An optional that holds no value.</returns>
    public static Optional<T> Empty<T>() where T : notnull => default;

    /// <summary>Returns an optional holding <paramref name="value"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to hold. It must not be <see langword="null"/>.</param>
    /// <returns>A present optional holding <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static Optional<T> Of<T>(T value) where T : notnull =>
        value is null ? throw new ArgumentNullException(nameof(value)) : new Optional<T>(value);

    /// <summary>
    /// Returns an optional holding <paramref name="value"/>, or an empty optional when
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to hold, or <see langword="null"/>.</param>
    /// <returns>A present optional, or an empty one when <paramref name="value"/> is <see langword="null"/>.</returns>
    public static Optional<T> OfNullable<T>(T? value) where T : notnull =>
        value is null ? default : new Optional<T>(value);

    /// <summary>
    /// Returns an optional holding the value of <paramref name="value"/>, or an empty optional when
    /// <paramref name="value"/> has no value.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="value">The nullable value to unwrap.</param>
    /// <returns>A present optional, or an empty one when <paramref name="value"/> has no value.</returns>
    public static Optional<T> OfNullable<T>(T? value) where T : struct =>
        value.HasValue ? new Optional<T>(value.GetValueOrDefault()) : default;
}
