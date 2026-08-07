using FacilityInspection.Domain.Operators;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data.Seeds;

public sealed class OperatorSeedService(
    IDbContextFactory<InspectionDbContext> dbContextFactory,
    IPasswordHasher<Operator> passwordHasher)
{
    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var seedOperators =
            new List<OperatorSeedData>
            {
                new(
                    "manager",
                    "保全責任者",
                    OperatorRole.MaintenanceManager,
                    "Demo1234!"),

                new(
                    "inspector",
                    "点検担当者1",
                    OperatorRole.Inspector,
                    "Demo1234!"),

                new(
                    "inspector2",
                    "点検担当者2",
                    OperatorRole.Inspector,
                    "Demo1234!"),

                new(
                    "inspector3",
                    "点検担当者3",
                    OperatorRole.Inspector,
                    "Demo1234!"),

                new(
                    "inspector4",
                    "点検担当者4",
                    OperatorRole.Inspector,
                    "Demo1234!"),

                new(
                    "inspector5",
                    "点検担当者5",
                    OperatorRole.Inspector,
                    "Demo1234!")
            };

        var existingNormalizedLoginIds =
            await dbContext.Operators
                .AsNoTracking()
                .Select(x => x.NormalizedLoginId)
                .ToHashSetAsync(
                    cancellationToken);

        var newOperators =
            seedOperators
                .Where(x =>
                    !existingNormalizedLoginIds.Contains(
                        NormalizeLoginId(x.LoginId)))
                .Select(x =>
                    CreateOperator(
                        x.LoginId,
                        x.DisplayName,
                        x.Role,
                        x.Password))
                .ToList();

        if (newOperators.Count == 0)
        {
            return;
        }

        dbContext.Operators.AddRange(
            newOperators);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private Operator CreateOperator(
        string loginId,
        string displayName,
        OperatorRole role,
        string password)
    {
        var trimmedLoginId =
            loginId.Trim();

        var user = new Operator
        {
            LoginId = trimmedLoginId,

            NormalizedLoginId =
                NormalizeLoginId(trimmedLoginId),

            DisplayName =
                displayName.Trim(),

            Role = role,

            IsActive = true
        };

        user.PasswordHash =
            passwordHasher.HashPassword(
                user,
                password);

        return user;
    }

    private static string NormalizeLoginId(
        string loginId)
    {
        return loginId
            .Trim()
            .ToUpperInvariant();
    }

    private sealed record OperatorSeedData(
        string LoginId,
        string DisplayName,
        OperatorRole Role,
        string Password);
}