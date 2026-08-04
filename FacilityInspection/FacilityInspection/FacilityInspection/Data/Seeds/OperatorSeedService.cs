using FacilityInspection.Domain.Operators;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
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

        if (await dbContext.Operators.AnyAsync(cancellationToken))
        {
            return;
        }

        var manager = CreateOperator(
            "manager",
            "保全責任者",
            OperatorRole.MaintenanceManager,
            "Demo1234!");

        var inspector = CreateOperator(
            "inspector",
            "点検担当者1",
            OperatorRole.Inspector,
            "Demo1234!");

        dbContext.Operators.AddRange(manager, inspector);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Operator CreateOperator(
        string loginId,
        string displayName,
        OperatorRole role,
        string password)
    {
        var normalizedLoginId =
            loginId.Trim().ToUpperInvariant();

        var user = new Operator
        {
            LoginId = loginId.Trim(),
            NormalizedLoginId = normalizedLoginId,
            DisplayName = displayName,
            Role = role,
            IsActive = true
        };

        user.PasswordHash =
            passwordHasher.HashPassword(user, password);

        return user;
    }
}