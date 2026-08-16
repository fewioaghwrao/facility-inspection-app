using FacilityInspection.Domain.AuditLogs;
using Xunit;

namespace FacilityInspection.Tests.Domain.AuditLogs;

public sealed class AuditLogTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithValidValues_CreatesAuditLog()
    {
        // Arrange
        var operatorId =
            Guid.NewGuid();

        var entityId =
            Guid.NewGuid();

        var occurredAtUtc =
            new DateTime(
                2026,
                8,
                17,
                1,
                30,
                0,
                DateTimeKind.Utc);

        // Act
        var auditLog =
            new AuditLog(
                operatorId,
                AuditActionType.Update,
                AuditEntityType.Equipment,
                entityId,
                beforeValue:
                    "変更前",
                afterValue:
                    "変更後",
                reason:
                    "設備情報訂正",
                occurredAtUtc:
                    occurredAtUtc);

        // Assert
        Assert.Equal(
            operatorId,
            auditLog.OperatorId);

        Assert.Equal(
            AuditActionType.Update,
            auditLog.ActionType);

        Assert.Equal(
            AuditEntityType.Equipment,
            auditLog.EntityType);

        Assert.Equal(
            entityId,
            auditLog.EntityId);

        Assert.Equal(
            "変更前",
            auditLog.BeforeValue);

        Assert.Equal(
            "変更後",
            auditLog.AfterValue);

        Assert.Equal(
            "設備情報訂正",
            auditLog.Reason);

        Assert.Equal(
            occurredAtUtc,
            auditLog.OccurredAtUtc);
    }


    // ============================================
    // OperatorId
    // ============================================

    [Fact]
    public void Constructor_WithEmptyOperatorId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new AuditLog(
                    Guid.Empty,
                    AuditActionType.Update,
                    AuditEntityType.Equipment,
                    Guid.NewGuid()));

        // Assert
        Assert.Equal(
            "operatorId",
            exception.ParamName);

        Assert.Contains(
            "操作者IDを指定してください。",
            exception.Message);
    }


    // ============================================
    // EntityId
    // ============================================

    [Fact]
    public void Constructor_WithEmptyEntityId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new AuditLog(
                    Guid.NewGuid(),
                    AuditActionType.Update,
                    AuditEntityType.Equipment,
                    Guid.Empty));

        // Assert
        Assert.Equal(
            "entityId",
            exception.ParamName);

        Assert.Contains(
            "操作対象IDを指定してください。",
            exception.Message);
    }


    // ============================================
    // ActionType
    // ============================================

    [Fact]
    public void Constructor_WithUndefinedActionType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidActionType =
            (AuditActionType)9999;

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AuditLog(
                    Guid.NewGuid(),
                    invalidActionType,
                    AuditEntityType.Equipment,
                    Guid.NewGuid()));

        // Assert
        Assert.Equal(
            "actionType",
            exception.ParamName);

        Assert.Contains(
            "有効な操作種別を指定してください。",
            exception.Message);
    }


    // ============================================
    // EntityType
    // ============================================

    [Fact]
    public void Constructor_WithUndefinedEntityType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidEntityType =
            (AuditEntityType)9999;

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AuditLog(
                    Guid.NewGuid(),
                    AuditActionType.Update,
                    invalidEntityType,
                    Guid.NewGuid()));

        // Assert
        Assert.Equal(
            "entityType",
            exception.ParamName);

        Assert.Contains(
            "有効な対象種別を指定してください。",
            exception.Message);
    }


    // ============================================
    // BeforeValue
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundBeforeValue_TrimsBeforeValue()
    {
        // Act
        var auditLog =
            CreateAuditLog(
                beforeValue:
                    "  変更前データ  ");

        // Assert
        Assert.Equal(
            "変更前データ",
            auditLog.BeforeValue);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyBeforeValue_NormalizesToNull(
        string? beforeValue)
    {
        // Act
        var auditLog =
            CreateAuditLog(
                beforeValue:
                    beforeValue);

        // Assert
        Assert.Null(
            auditLog.BeforeValue);
    }


    // ============================================
    // AfterValue
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundAfterValue_TrimsAfterValue()
    {
        // Act
        var auditLog =
            CreateAuditLog(
                afterValue:
                    "  変更後データ  ");

        // Assert
        Assert.Equal(
            "変更後データ",
            auditLog.AfterValue);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyAfterValue_NormalizesToNull(
        string? afterValue)
    {
        // Act
        var auditLog =
            CreateAuditLog(
                afterValue:
                    afterValue);

        // Assert
        Assert.Null(
            auditLog.AfterValue);
    }


    // ============================================
    // Reason
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundReason_TrimsReason()
    {
        // Act
        var auditLog =
            CreateAuditLog(
                reason:
                    "  入力内容を訂正したため  ");

        // Assert
        Assert.Equal(
            "入力内容を訂正したため",
            auditLog.Reason);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyReason_NormalizesToNull(
        string? reason)
    {
        // Act
        var auditLog =
            CreateAuditLog(
                reason:
                    reason);

        // Assert
        Assert.Null(
            auditLog.Reason);
    }


    // ============================================
    // JSON
    // ============================================

    [Fact]
    public void Constructor_WithJsonValues_PreservesJsonText()
    {
        // Arrange
        const string beforeValue =
            """
            {"status":"Completed"}
            """;

        const string afterValue =
            """
            {"status":"Approved"}
            """;

        // Act
        var auditLog =
            CreateAuditLog(
                beforeValue:
                    beforeValue,
                afterValue:
                    afterValue);

        // Assert
        Assert.Equal(
            """{"status":"Completed"}""",
            auditLog.BeforeValue);

        Assert.Equal(
            """{"status":"Approved"}""",
            auditLog.AfterValue);
    }


    // ============================================
    // OccurredAtUtc
    // ============================================

    [Fact]
    public void Constructor_WithOccurredAtUtc_SetsSpecifiedDateTime()
    {
        // Arrange
        var occurredAtUtc =
            new DateTime(
                2026,
                8,
                17,
                2,
                15,
                0,
                DateTimeKind.Utc);

        // Act
        var auditLog =
            CreateAuditLog(
                occurredAtUtc:
                    occurredAtUtc);

        // Assert
        Assert.Equal(
            occurredAtUtc,
            auditLog.OccurredAtUtc);
    }


    [Fact]
    public void Constructor_WithoutOccurredAtUtc_SetsCurrentUtcTime()
    {
        // Arrange
        var before =
            DateTime.UtcNow;

        // Act
        var auditLog =
            new AuditLog(
                Guid.NewGuid(),
                AuditActionType.Update,
                AuditEntityType.Equipment,
                Guid.NewGuid());

        var after =
            DateTime.UtcNow;

        // Assert
        Assert.InRange(
            auditLog.OccurredAtUtc,
            before,
            after);
    }

    // ============================================
    // Optional values
    // ============================================

    [Fact]
    public void Constructor_WithoutOptionalValues_CreatesAuditLogWithNullValues()
    {
        // Act
        var auditLog =
            new AuditLog(
                Guid.NewGuid(),
                AuditActionType.Create,
                AuditEntityType.Equipment,
                Guid.NewGuid(),
                occurredAtUtc:
                    CreateUtcDate());

        // Assert
        Assert.Null(
            auditLog.BeforeValue);

        Assert.Null(
            auditLog.AfterValue);

        Assert.Null(
            auditLog.Reason);
    }


    // ============================================
    // Test Helpers
    // ============================================

    private static AuditLog CreateAuditLog(
        string? beforeValue = null,
        string? afterValue = null,
        string? reason = null,
        DateTime? occurredAtUtc = null)
    {
        return new AuditLog(
            Guid.NewGuid(),
            AuditActionType.Update,
            AuditEntityType.Equipment,
            Guid.NewGuid(),
            beforeValue,
            afterValue,
            reason,
            occurredAtUtc ??
            CreateUtcDate());
    }


    private static DateTime CreateUtcDate()
    {
        return new DateTime(
            2026,
            8,
            17,
            1,
            0,
            0,
            DateTimeKind.Utc);
    }
}
