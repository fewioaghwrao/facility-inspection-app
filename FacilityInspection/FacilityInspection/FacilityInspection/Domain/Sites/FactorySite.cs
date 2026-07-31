using FacilityInspection.Domain.Common;
using System;
using System.Collections.Generic;
using FacilityLocation =
    FacilityInspection.Domain.Locations.Location;

namespace FacilityInspection.Domain.Sites;

public sealed class FactorySite : EntityBase
{
    private readonly List<FacilityLocation> _locations = [];

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<FacilityLocation> Locations =>
        _locations.AsReadOnly();

    // EF Core用
    private FactorySite()
    {
    }

    public FactorySite(
        string code,
        string name,
        string? description = null)
        : base()
    {
        SetCode(code);
        SetName(name);

        Description = NormalizeOptional(description);
        IsActive = true;
    }

    public void Update(
        string code,
        string name,
        string? description)
    {
        SetCode(code);
        SetName(name);

        Description = NormalizeOptional(description);
        MarkUpdated();
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        MarkUpdated();
    }

    private void SetCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length > 20)
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                "工場コードは20文字以内で入力してください。");
        }

        Code = normalized;
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalized = name.Trim();

        if (normalized.Length > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(name),
                "工場名は100文字以内で入力してください。");
        }

        Name = normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}