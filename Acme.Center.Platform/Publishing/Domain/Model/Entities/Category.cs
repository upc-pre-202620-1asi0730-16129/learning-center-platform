using Acme.Center.Platform.Publishing.Domain.Model.Commands;
using Acme.Center.Platform.Publishing.Domain.Model.Events;
using Acme.Center.Platform.Publishing.Domain.Model.ValueObjects;
using Acme.Center.Platform.Shared.Domain.Model;
using Acme.Center.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace Acme.Center.Platform.Publishing.Domain.Model.Entities;

/// <summary>
///     Represents a category in the ACME Learning Center Platform.
/// </summary>
public class Category : AggregateRoot
{
    /// <summary>
    ///     Default constructor for the category entity
    /// </summary>
    public Category()
    {
        Name = string.Empty;
    }

    /// <summary>
    ///     Constructor for the category entity
    /// </summary>
    /// <param name="name">
    ///     The name of the category
    /// </param>
    public Category(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        AddDomainEvent(new CategoryCreatedEvent(name));
    }

    public Category(CreateCategoryCommand command) : this(command.Name)
    {
    }

    public CategoryId Id { get; private set; }
    public string Name { get; private set; }

    public Category UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        return this;
    }
}