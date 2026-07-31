using FacilityInspection.Data;
using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Sites;
using System;
using System.Collections.Generic;

namespace FacilityInspection.Domain.Locations;

public sealed class Location : EntityBase
{
    private readonly List<Equipment> _equipments = [];

    public Guid FactorySiteId { get; private set; }

    public FactorySite FactorySite { get; private set; } = null!;

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Floor { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<Equipment> Equipments =>
        _equipments.AsReadOnly();

    // EF Core用
    private Location()
    {
    }

    public Location(
        Guid factorySiteId,
        string code,
        string name,
        string? floor = null,
        string? description = null)
        : base()
    {
        if (factorySiteId == Guid.Empty)
        {
            throw new ArgumentException(
                "工場IDを指定してください。",
                nameof(factorySiteId));
        }

        FactorySiteId = factorySiteId;

        SetCode(code);
        SetName(name);

        Floor = NormalizeOptional(floor);
        Description = NormalizeOptional(description);
        IsActive = true;
    }

    public void Update(
        string code,
        string name,
        string? floor,
        string? description)
    {
        SetCode(code);
        SetName(name);

        Floor = NormalizeOptional(floor);
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

        if (normalized.Length > 30)
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                "設置場所コードは30文字以内で入力してください。");
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
                "設置場所名は100文字以内で入力してください。");
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