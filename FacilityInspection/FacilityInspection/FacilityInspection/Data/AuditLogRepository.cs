using FacilityInspection.Domain.AuditLogs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data;


// ============================================
// 操作履歴 一覧DTO
// ============================================

public sealed record AuditLogListData(
    Guid AuditLogId,
    DateTime OccurredAtUtc,
    Guid OperatorId,
    string OperatorName,
    AuditActionType ActionType,
    AuditEntityType EntityType,
    Guid EntityId,
    string? Reason);


// ============================================
// 操作履歴 詳細DTO
// ============================================

public sealed record AuditLogDetailData(
    Guid AuditLogId,
    DateTime OccurredAtUtc,
    Guid OperatorId,
    string OperatorName,
    AuditActionType ActionType,
    AuditEntityType EntityType,
    Guid EntityId,
    string? BeforeValue,
    string? AfterValue,
    string? Reason);


// ============================================
// Repository
// ============================================

public sealed class AuditLogRepository
{
    private readonly InspectionDbContextFactory
        _dbContextFactory;


    // ============================================
    // Constructor
    // ============================================

    public AuditLogRepository(
        InspectionDbContextFactory dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(
            dbContextFactory);

        _dbContextFactory =
            dbContextFactory;
    }


    // ============================================
    // Add
    // ============================================

    public async Task AddAsync(
        AuditLog auditLog,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            auditLog);

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        dbContext.AuditLogs.Add(
            auditLog);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }


    // ============================================
    // Add
    //
    // 呼び出し側でAuditLogをnewしなくても
    // 登録できる簡易メソッド。
    // ============================================

    public async Task AddAsync(
        Guid operatorId,
        AuditActionType actionType,
        AuditEntityType entityType,
        Guid entityId,
        string? beforeValue = null,
        string? afterValue = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog =
            new AuditLog(
                operatorId,
                actionType,
                entityType,
                entityId,
                beforeValue,
                afterValue,
                reason);

        await AddAsync(
            auditLog,
            cancellationToken);
    }


    // ============================================
    // 一覧取得
    // ============================================

    public async Task<
        IReadOnlyList<AuditLogListData>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var rows =
            await dbContext.AuditLogs
                .AsNoTracking()
                .OrderByDescending(x =>
                    x.OccurredAtUtc)
                .ThenByDescending(x =>
                    x.CreatedAtUtc)
                .Select(x =>
                    new
                    {
                        AuditLogId =
                            x.Id,

                        x.OccurredAtUtc,

                        x.OperatorId,

                        OperatorName =
                            x.Operator.DisplayName,

                        x.ActionType,

                        x.EntityType,

                        x.EntityId,

                        x.Reason
                    })
                .ToListAsync(
                    cancellationToken);

        return rows
            .Select(x =>
                new AuditLogListData(
                    x.AuditLogId,
                    x.OccurredAtUtc,
                    x.OperatorId,
                    x.OperatorName,
                    x.ActionType,
                    x.EntityType,
                    x.EntityId,
                    x.Reason))
            .ToList();
    }


    // ============================================
    // 詳細取得
    // ============================================

    public async Task<AuditLogDetailData?>
        GetDetailAsync(
            Guid auditLogId,
            CancellationToken cancellationToken = default)
    {
        if (auditLogId == Guid.Empty)
        {
            return null;
        }

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var row =
            await dbContext.AuditLogs
                .AsNoTracking()
                .Where(x =>
                    x.Id == auditLogId)
                .Select(x =>
                    new
                    {
                        AuditLogId =
                            x.Id,

                        x.OccurredAtUtc,

                        x.OperatorId,

                        OperatorName =
                            x.Operator.DisplayName,

                        x.ActionType,

                        x.EntityType,

                        x.EntityId,

                        x.BeforeValue,

                        x.AfterValue,

                        x.Reason
                    })
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new AuditLogDetailData(
            row.AuditLogId,
            row.OccurredAtUtc,
            row.OperatorId,
            row.OperatorName,
            row.ActionType,
            row.EntityType,
            row.EntityId,
            row.BeforeValue,
            row.AfterValue,
            row.Reason);
    }
}