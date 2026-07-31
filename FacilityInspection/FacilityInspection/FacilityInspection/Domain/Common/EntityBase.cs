using System;

namespace FacilityInspection.Domain.Common;

public abstract class EntityBase
{
    public Guid Id { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    // EF Core用
    protected EntityBase()
    {
    }

    protected EntityBase(Guid? id = null)
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