using Acme.Center.Platform.Shared.Domain.Model;
using Acme.Center.Platform.Shared.Domain.Model.ValueObjects;

namespace Acme.Center.Platform.Publishing.Domain.Model.ValueObjects;

/// <summary>
///     Represents the strongly-typed identifier for a category in the Publishing bounded context.
/// </summary>
/// <param name="Value">The underlying identifier value.</param>
public readonly record struct CategoryId(int Value) : IEntityId<int>
{
    public static implicit operator int(CategoryId id) => id.Value;
    public static implicit operator CategoryId(int value) => new(value);

    public override string ToString() => Value.ToString();
}
