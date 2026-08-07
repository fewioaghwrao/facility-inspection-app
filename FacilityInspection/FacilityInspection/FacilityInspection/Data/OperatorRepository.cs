using FacilityInspection.Domain.Operators;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data;

public sealed class OperatorRepository
{
    private readonly InspectionDbContextFactory _dbContextFactory;
    private readonly IPasswordHasher<Operator> _passwordHasher;

    public OperatorRepository(
        InspectionDbContextFactory dbContextFactory,
        IPasswordHasher<Operator> passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);
        ArgumentNullException.ThrowIfNull(passwordHasher);

        _dbContextFactory = dbContextFactory;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<Operator>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        return await dbContext.Operators
            .AsNoTracking()
            .OrderBy(x => x.Role)
            .ThenBy(x => x.DisplayName)
            .ThenBy(x => x.LoginId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(
        string loginId,
        string displayName,
        OperatorRole role,
        string password,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loginId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var trimmedLoginId = loginId.Trim();
        var normalizedLoginId =
            NormalizeLoginId(trimmedLoginId);

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var duplicated =
            await dbContext.Operators.AnyAsync(
                x => x.NormalizedLoginId == normalizedLoginId,
                cancellationToken);

        if (duplicated)
        {
            throw new InvalidOperationException(
                "同じログインIDの担当者がすでに登録されています。");
        }

        var operatorEntity = new Operator
        {
            LoginId = trimmedLoginId,
            NormalizedLoginId = normalizedLoginId,
            DisplayName = displayName.Trim(),
            Role = role,
            IsActive = true
        };

        operatorEntity.PasswordHash =
            _passwordHasher.HashPassword(
                operatorEntity,
                password);

        dbContext.Operators.Add(operatorEntity);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return operatorEntity.Id;
    }

    public async Task UpdateAsync(
        Guid operatorId,
        string loginId,
        string displayName,
        OperatorRole role,
        string? newPassword,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loginId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var trimmedLoginId = loginId.Trim();
        var normalizedLoginId =
            NormalizeLoginId(trimmedLoginId);

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var operatorEntity =
            await dbContext.Operators.SingleOrDefaultAsync(
                x => x.Id == operatorId,
                cancellationToken);

        if (operatorEntity is null)
        {
            throw new InvalidOperationException(
                "編集対象の担当者が見つかりません。");
        }

        var duplicated =
            await dbContext.Operators.AnyAsync(
                x =>
                    x.Id != operatorId &&
                    x.NormalizedLoginId == normalizedLoginId,
                cancellationToken);

        if (duplicated)
        {
            throw new InvalidOperationException(
                "同じログインIDの担当者がすでに登録されています。");
        }

        if (operatorEntity.Role ==
                OperatorRole.MaintenanceManager &&
            role != OperatorRole.MaintenanceManager &&
            operatorEntity.IsActive)
        {
            await EnsureAnotherActiveManagerExistsAsync(
                dbContext,
                operatorEntity.Id,
                cancellationToken);
        }

        operatorEntity.LoginId = trimmedLoginId;
        operatorEntity.NormalizedLoginId = normalizedLoginId;
        operatorEntity.DisplayName = displayName.Trim();
        operatorEntity.Role = role;

        if (!string.IsNullOrEmpty(newPassword))
        {
            operatorEntity.PasswordHash =
                _passwordHasher.HashPassword(
                    operatorEntity,
                    newPassword);
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task SetActiveAsync(
        Guid operatorId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var operatorEntity =
            await dbContext.Operators.SingleOrDefaultAsync(
                x => x.Id == operatorId,
                cancellationToken);

        if (operatorEntity is null)
        {
            throw new InvalidOperationException(
                "対象の担当者が見つかりません。");
        }

        if (!isActive &&
            operatorEntity.Role ==
                OperatorRole.MaintenanceManager)
        {
            await EnsureAnotherActiveManagerExistsAsync(
                dbContext,
                operatorEntity.Id,
                cancellationToken);
        }

        operatorEntity.IsActive = isActive;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task
        EnsureAnotherActiveManagerExistsAsync(
            InspectionDbContext dbContext,
            Guid excludedOperatorId,
            CancellationToken cancellationToken)
    {
        var anotherManagerExists =
            await dbContext.Operators.AnyAsync(
                x =>
                    x.Id != excludedOperatorId &&
                    x.Role ==
                        OperatorRole.MaintenanceManager &&
                    x.IsActive,
                cancellationToken);

        if (!anotherManagerExists)
        {
            throw new InvalidOperationException(
                "有効な保全責任者を0人にはできません。");
        }
    }

    private static string NormalizeLoginId(
        string loginId)
    {
        return loginId
            .Trim()
            .ToUpperInvariant();
    }
}
