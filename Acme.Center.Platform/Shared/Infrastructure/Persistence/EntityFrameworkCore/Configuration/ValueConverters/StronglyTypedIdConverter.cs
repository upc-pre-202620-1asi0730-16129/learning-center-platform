using Acme.Center.Platform.Shared.Domain.Model;
using Acme.Center.Platform.Shared.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Acme.Center.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;

/// <summary>
///     Value converter for strongly-typed identifiers to and from their primitive representation.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
/// <typeparam name="TValue">The underlying primitive type.</typeparam>
public class StronglyTypedIdConverter<TId, TValue>(
    Func<TId, TValue> convertToProvider,
    Func<TValue, TId> convertFromProvider)
    : ValueConverter<TId, TValue>(
        id => convertToProvider(id),
        val => convertFromProvider(val))
    where TValue : struct
{
    public StronglyTypedIdConverter(Func<TValue, TId> factory)
        : this(id => ((IEntityId<TValue>)id!).Value, factory)
    {
    }
}
