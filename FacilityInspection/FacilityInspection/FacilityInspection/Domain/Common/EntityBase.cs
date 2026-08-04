using System;

namespace FacilityInspection.Domain.Common;

public abstract class EntityBase
{
    public Guid Id { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    protected EntityBase()
        : this(null)
    {
    }

    protected EntityBase(Guid? id)
    {
        Id = id ?? Guid.NewGuid();

        var now = DateTime.UtcNow;

        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    protected void MarkUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}