using FacilityInspection.Domain.Operators;
using System;

namespace FacilityInspection.Services.Authentication;

public sealed record SignedInOperator(
    Guid Id,
    string LoginId,
    string DisplayName,
    OperatorRole Role);