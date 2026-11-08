using System.Globalization;

namespace Tessaro;

public readonly partial struct Optional<T> : IEquatable<Optional<T>>
{
    /// <summary>
    /// Indicates whether this optional equals <paramref name="other"/>: both are empty, or both are
    /// present with values that are equal according to <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <param name="other">The optional to compare with.</param>
    /// <returns><see langword="true"/> if the optionals are equal; otherwise <see langword="false"/>.</returns>
    public bool Equals(Optional<T> other) =>
        IsPresent == other.IsPresent && (IsEmpty || EqualityComparer<T>.Default.Equals(_value, other._value));

    /// <inheritdoc cref="Equals(Optional{T})"/>
    /// <param name="obj">The object to compare with.</param>
    public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

    /// <summary>Returns the hash code of the value, or <c>0</c> when this optional is empty.</summary>
    /// <returns>A hash code consistent with <see cref="Equals(Optional{T})"/>.</returns>
    public override int GetHashCode() => IsPresent ? EqualityComparer<T>.Default.GetHashCode(_value) : 0;

    /// <summary>
    /// Returns <c>Optional[value]</c> when present and <c>Optional.empty</c> when empty, as in Java.
    /// Values are formatted with the invariant culture.
    /// </summary>
    /// <returns>A text representation of this optional.</returns>
    public override string ToString() =>
        IsPresent ? string.Create(CultureInfo.InvariantCulture, $"Optional[{_value}]") : "Optional.empty";

    /// <summary>Indicates whether two optionals are equal.</summary>
    /// <param name="left">The first optional.</param>
    /// <param name="right">The second optional.</param>
    /// <returns><see langword="true"/> if the optionals are equal; otherwise <see langword="false"/>.</returns>
    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

    /// <summary>Indicates whether two optionals are not equal.</summary>
    /// <param name="left">The first optional.</param>
    /// <param name="right">The second optional.</param>
    /// <returns><see langword="true"/> if the optionals are not equal; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);
}
