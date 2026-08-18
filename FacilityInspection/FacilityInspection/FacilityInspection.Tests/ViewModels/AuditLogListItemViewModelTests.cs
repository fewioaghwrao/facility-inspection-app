using FacilityInspection.Data;
using FacilityInspection.Domain.AuditLogs;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class AuditLogListItemViewModelTests
{
    private static readonly Guid AuditLogId =
        Guid.Parse(
            "11111111-2222-3333-4444-555555555555");

    private static readonly Guid OperatorId =
        Guid.Parse(
            "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");

    private static readonly Guid EntityId =
        Guid.Parse(
            "12345678-1234-5678-9012-345678901234");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var occurredAtUtc =
            new DateTime(
                2026,
                8,
                19,
                1,
                30,
                45,
                DateTimeKind.Utc);

        var source =
            CreateSource(
                occurredAtUtc:
                    occurredAtUtc,
                actionType:
                    AuditActionType.Approve,
                entityType:
                    AuditEntityType.Inspection,
                reason:
                    "点検内容を確認");

        // Act
        var sut =
            new AuditLogListItemViewModel(
                source,
                _ =>
                {
                });

        // Assert
        Assert.Equal(
            AuditLogId,
            sut.AuditLogId);

        Assert.Equal(
            occurredAtUtc,
            sut.OccurredAtUtc);

        Assert.Equal(
            OperatorId,
            sut.OperatorId);

        Assert.Equal(
            "保全管理者A",
            sut.OperatorName);

        Assert.Equal(
            AuditActionType.Approve,
            sut.ActionType);

        Assert.Equal(
            AuditEntityType.Inspection,
            sut.EntityType);

        Assert.Equal(
            EntityId,
            sut.EntityId);

        Assert.Equal(
            "点検内容を確認",
            sut.Reason);
    }


    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new AuditLogListItemViewModel(
                        null!,
                        _ =>
                        {
                        }));

        // Assert
        Assert.Equal(
            "source",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullDetailAction_ThrowsArgumentNullException()
    {
        // Arrange
        var source =
            CreateSource();

        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new AuditLogListItemViewModel(
                        source,
                        null!));

        // Assert
        Assert.Equal(
            "openDetailRequested",
            exception.ParamName);
    }


    // ============================================
    // Reason
    // ============================================

    [Fact]
    public void Constructor_WhenReasonIsNull_UsesEmptyString()
    {
        // Arrange
        var source =
            CreateSource(
                reason:
                    null);

        // Act
        var sut =
            CreateViewModel(
                source);

        // Assert
        Assert.Equal(
            string.Empty,
            sut.Reason);

        Assert.False(
            sut.HasReason);
    }


    [Theory]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("   ", false)]
    [InlineData("点検結果を承認", true)]
    public void HasReason_ReturnsExpectedValue(
        string reason,
        bool expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                CreateSource(
                    reason:
                        reason));

        // Assert
        Assert.Equal(
            expected,
            sut.HasReason);
    }


    // ============================================
    // Occurred At
    // ============================================

    [Fact]
    public void OccurredAtText_ReturnsLocalTimeFormattedText()
    {
        // Arrange
        var occurredAtUtc =
            new DateTime(
                2026,
                8,
                19,
                1,
                30,
                45,
                DateTimeKind.Utc);

        var sut =
            CreateViewModel(
                CreateSource(
                    occurredAtUtc:
                        occurredAtUtc));

        var expected =
            occurredAtUtc
                .ToLocalTime()
                .ToString(
                    "yyyy/MM/dd HH:mm:ss");

        // Act
        var actual =
            sut.OccurredAtText;

        // Assert
        Assert.Equal(
            expected,
            actual);
    }


    // ============================================
    // Action Type
    // ============================================

    [Theory]
    [InlineData(
        AuditActionType.Create,
        "登録")]
    [InlineData(
        AuditActionType.Update,
        "更新")]
    [InlineData(
        AuditActionType.Delete,
        "削除")]
    [InlineData(
        AuditActionType.Cancel,
        "取消")]
    [InlineData(
        AuditActionType.InspectionStart,
        "点検開始")]
    [InlineData(
        AuditActionType.InspectionComplete,
        "点検完了")]
    [InlineData(
        AuditActionType.Approve,
        "承認")]
    [InlineData(
        AuditActionType.ReturnForCorrection,
        "差し戻し")]
    [InlineData(
        AuditActionType.Login,
        "ログイン")]
    [InlineData(
        AuditActionType.Logout,
        "ログアウト")]
    [InlineData(
        AuditActionType.Backup,
        "バックアップ")]
    [InlineData(
        AuditActionType.Restore,
        "復元")]
    public void GetActionTypeText_ReturnsExpectedText(
        AuditActionType actionType,
        string expected)
    {
        // Act
        var actual =
            AuditLogListItemViewModel
                .GetActionTypeText(
                    actionType);

        // Assert
        Assert.Equal(
            expected,
            actual);
    }


    // ============================================
    // Entity Type
    // ============================================

    [Theory]
    [InlineData(
        AuditEntityType.Inspection,
        "点検")]
    [InlineData(
        AuditEntityType.InspectionSchedule,
        "点検予定")]
    [InlineData(
        AuditEntityType.Equipment,
        "設備")]
    [InlineData(
        AuditEntityType.InspectionTemplate,
        "点検票テンプレート")]
    [InlineData(
        AuditEntityType.Operator,
        "担当者")]
    [InlineData(
        AuditEntityType.System,
        "システム")]
    public void GetEntityTypeText_ReturnsExpectedText(
        AuditEntityType entityType,
        string expected)
    {
        // Act
        var actual =
            AuditLogListItemViewModel
                .GetEntityTypeText(
                    entityType);

        // Assert
        Assert.Equal(
            expected,
            actual);
    }


    // ============================================
    // Entity ID
    // ============================================

    [Fact]
    public void EntityIdText_ReturnsFullGuidText()
    {
        // Arrange
        var sut =
            CreateViewModel();

        // Assert
        Assert.Equal(
            EntityId.ToString(),
            sut.EntityIdText);
    }


    [Fact]
    public void ShortEntityIdText_ReturnsFirstEightCharacters()
    {
        // Arrange
        var sut =
            CreateViewModel();

        // Assert
        Assert.Equal(
            EntityId
                .ToString("N")[..8],
            sut.ShortEntityIdText);

        Assert.Equal(
            8,
            sut.ShortEntityIdText.Length);
    }


    // ============================================
    // Detail
    // ============================================

    [Fact]
    public void OpenDetailCommand_RequestsDetailWithAuditLogId()
    {
        // Arrange
        Guid? requestedId =
            null;

        var sut =
            new AuditLogListItemViewModel(
                CreateSource(),
                id =>
                    requestedId =
                        id);

        // Act
        sut.OpenDetailCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            AuditLogId,
            requestedId);
    }


    [Fact]
    public void OpenDetailCommand_WhenAuditLogIdIsEmpty_DoesNotRequestDetail()
    {
        // Arrange
        var callCount =
            0;

        var sut =
            new AuditLogListItemViewModel(
                CreateSource(
                    auditLogId:
                        Guid.Empty),
                _ =>
                    callCount++);

        // Act
        sut.OpenDetailCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            0,
            callCount);
    }


    // ============================================
    // Helpers
    // ============================================

    private static AuditLogListItemViewModel
        CreateViewModel(
            AuditLogListData? source = null)
    {
        return new AuditLogListItemViewModel(
            source ??
                CreateSource(),
            _ =>
            {
            });
    }


    private static AuditLogListData
        CreateSource(
            Guid? auditLogId = null,
            DateTime? occurredAtUtc = null,
            Guid? operatorId = null,
            string operatorName =
                "保全管理者A",
            AuditActionType actionType =
                AuditActionType.Approve,
            AuditEntityType entityType =
                AuditEntityType.Inspection,
            Guid? entityId = null,
            string? reason =
                "点検内容を確認")
    {
        return new AuditLogListData(
            AuditLogId:
                auditLogId ??
                AuditLogId,

            OccurredAtUtc:
                occurredAtUtc ??
                new DateTime(
                    2026,
                    8,
                    19,
                    1,
                    30,
                    45,
                    DateTimeKind.Utc),

            OperatorId:
                operatorId ??
                OperatorId,

            OperatorName:
                operatorName,

            ActionType:
                actionType,

            EntityType:
                entityType,

            EntityId:
                entityId ??
                EntityId,

            Reason:
                reason);
    }
}