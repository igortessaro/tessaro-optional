namespace Tessaro;

/// <summary>Extension methods that convert nullable values to <see cref="Optional{T}"/>.</summary>
public static class OptionalExtensions
{
    /// <summary>Converts a possibly-null value to an optional.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value, or <see langword="null"/>.</param>
    /// <returns>A present optional, or an empty one when <paramref name="value"/> is <see langword="null"/>.</returns>
    public static Optional<T> ToOptional<T>(this T? value) where T : notnull => Optional.OfNullable(value);

    /// <summary>Converts a nullable value type to an optional of its underlying type.</summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>A present optional, or an empty one when <paramref name="value"/> has no value.</returns>
    public static Optional<T> ToOptional<T>(this T? value) where T : struct => Optional.OfNullable(value);
}
