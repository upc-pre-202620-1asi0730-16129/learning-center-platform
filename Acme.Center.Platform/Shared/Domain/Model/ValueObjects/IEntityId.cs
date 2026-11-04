namespace Acme.Center.Platform.Shared.Domain.Model.ValueObjects;

/// <summary>
///     Represents a strongly-typed entity identifier contract.
/// </summary>
/// <typeparam name="T">The underlying primitive identifier type.</typeparam>
public interface IEntityId<T> where T : struct
{
    /// <summary>
    ///     Gets the underlying primitive value of the identifier.
    /// </summary>
    T Value { get; }
}
