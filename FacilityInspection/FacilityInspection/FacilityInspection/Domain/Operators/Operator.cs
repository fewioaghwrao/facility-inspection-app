using FacilityInspection.Domain.Common;
using System;

namespace FacilityInspection.Domain.Operators;

public sealed class Operator : EntityBase
{
    public string LoginId { get; set; } = string.Empty;

    public string NormalizedLoginId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public OperatorRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastLoginAt { get; set; }

    public void RecordLogin(DateTimeOffset loginAt)
    {
        LastLoginAt = loginAt;
    }
}