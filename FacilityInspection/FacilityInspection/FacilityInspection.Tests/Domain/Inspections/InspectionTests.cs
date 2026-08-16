using FacilityInspection.Domain.Inspections;

namespace FacilityInspection.Tests.Domain.Inspections;

public sealed class InspectionTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithValidScheduleId_CreatesNotStartedInspection()
    {
        // Arrange
        var scheduleId =
            Guid.NewGuid();

        // Act
        var inspection =
            new Inspection(
                scheduleId);

        // Assert
        Assert.Equal(
            scheduleId,
            inspection.InspectionScheduleId);

        Assert.Equal(
            InspectionStatus.NotStarted,
            inspection.Status);

        Assert.Null(
            inspection.PerformedByOperatorId);

        Assert.Null(
            inspection.StartedAtUtc);

        Assert.Null(
            inspection.CompletedAtUtc);

        Assert.Null(
            inspection.ReviewedAtUtc);

        Assert.Null(
            inspection.ReturnReason);

        Assert.Empty(
            inspection.Results);

        Assert.Empty(
            inspection.Photos);
    }


    [Fact]
    public void Constructor_WithEmptyScheduleId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Inspection(
                    Guid.Empty));

        // Assert
        Assert.Equal(
            "inspectionScheduleId",
            exception.ParamName);
    }


    // ============================================
    // Start
    // ============================================

    [Fact]
    public void Start_WhenNotStarted_ChangesStatusToInProgress()
    {
        // Arrange
        var inspection =
            new Inspection(
                Guid.NewGuid());

        var operatorId =
            Guid.NewGuid();

        var startedAtUtc =
            CreateUtcDate(
                2026,
                8,
                17,
                0);

        // Act
        inspection.Start(
            operatorId,
            startedAtUtc);

        // Assert
        Assert.Equal(
            InspectionStatus.InProgress,
            inspection.Status);

        Assert.Equal(
            operatorId,
            inspection.PerformedByOperatorId);

        Assert.Equal(
            startedAtUtc,
            inspection.StartedAtUtc);

        Assert.Null(
            inspection.CompletedAtUtc);

        Assert.Null(
            inspection.ReviewedAtUtc);

        Assert.Null(
            inspection.ReturnReason);
    }


    [Fact]
    public void Start_WithEmptyOperatorId_ThrowsArgumentException()
    {
        // Arrange
        var inspection =
            new Inspection(
                Guid.NewGuid());

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => inspection.Start(
                    Guid.Empty,
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        0)));

        // Assert
        Assert.Equal(
            "performedByOperatorId",
            exception.ParamName);
    }


    [Fact]
    public void Start_WhenAlreadyInProgress_ThrowsInvalidOperationException()
    {
        // Arrange
        var inspection =
            CreateInProgressInspection();

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => inspection.Start(
                    Guid.NewGuid(),
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        1)));

        // Assert
        Assert.Equal(
            "未実施または差し戻し状態の点検だけ開始できます。",
            exception.Message);
    }


    [Fact]
    public void Start_WhenReturned_RestartsInspection()
    {
        // Arrange
        var inspection =
            CreateCompletedInspection();

        inspection.Return(
            "写真を撮り直してください。",
            CreateUtcDate(
                2026,
                8,
                17,
                2));

        var newOperatorId =
            Guid.NewGuid();

        var restartedAtUtc =
            CreateUtcDate(
                2026,
                8,
                18,
                0);

        // Act
        inspection.Start(
            newOperatorId,
            restartedAtUtc);

        // Assert
        Assert.Equal(
            InspectionStatus.InProgress,
            inspection.Status);

        Assert.Equal(
            newOperatorId,
            inspection.PerformedByOperatorId);

        Assert.Equal(
            restartedAtUtc,
            inspection.StartedAtUtc);

        Assert.Null(
            inspection.CompletedAtUtc);

        Assert.Null(
            inspection.ReviewedAtUtc);

        Assert.Null(
            inspection.ReturnReason);
    }


    [Fact]
    public void Start_WhenApproved_ThrowsInvalidOperationException()
    {
        // Arrange
        var inspection =
            CreateCompletedInspection();

        inspection.Approve(
            CreateUtcDate(
                2026,
                8,
                17,
                2));

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => inspection.Start(
                    Guid.NewGuid(),
                    CreateUtcDate(
                        2026,
                        8,
                        18,
                        0)));

        // Assert
        Assert.Equal(
            "未実施または差し戻し状態の点検だけ開始できます。",
            exception.Message);
    }


    // ============================================
    // Complete
    // ============================================

    [Fact]
    public void Complete_WhenInProgress_ChangesStatusToCompleted()
    {
        // Arrange
        var inspection =
            CreateInProgressInspection();

        var completedAtUtc =
            CreateUtcDate(
                2026,
                8,
                17,
                1);

        // Act
        inspection.Complete(
            completedAtUtc);

        // Assert
        Assert.Equal(
            InspectionStatus.Completed,
            inspection.Status);

        Assert.Equal(
            completedAtUtc,
            inspection.CompletedAtUtc);
    }


    [Fact]
    public void Complete_WhenNotStarted_ThrowsInvalidOperationException()
    {
        // Arrange
        var inspection =
            new Inspection(
                Guid.NewGuid());

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => inspection.Complete(
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        1)));

        // Assert
        Assert.Equal(
            "実施中の点検だけ完了できます。",
            exception.Message);
    }


    // ============================================
    // Return
    // ============================================

    [Fact]
    public void Return_WhenCompleted_ChangesStatusToReturned()
    {
        // Arrange
        var inspection =
            CreateCompletedInspection();

        var reviewedAtUtc =
            CreateUtcDate(
                2026,
                8,
                17,
                2);

        // Act
        inspection.Return(
            "写真を再確認してください。",
            reviewedAtUtc);

        // Assert
        Assert.Equal(
            InspectionStatus.Returned,
            inspection.Status);

        Assert.Equal(
            "写真を再確認してください。",
            inspection.ReturnReason);

        Assert.Equal(
            reviewedAtUtc,
            inspection.ReviewedAtUtc);
    }


    [Fact]
    public void Return_WithLeadingAndTrailingSpaces_TrimsReason()
    {
        // Arrange
        var inspection =
            CreateCompletedInspection();

        // Act
        inspection.Return(
            "  写真を再確認してください。  ",
            CreateUtcDate(
                2026,
                8,
                17,
                2));

        // Assert
        Assert.Equal(
            "写真を再確認してください。",
            inspection.ReturnReason);
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Return_WithEmptyOrWhitespaceReason_ThrowsArgumentException(
        string reason)
    {
        // Arrange
        var inspection =
            CreateCompletedInspection();

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => inspection.Return(
                reason,
                CreateUtcDate(
                    2026,
                    8,
                    17,
                    2)));
    }


    [Fact]
    public void Return_With500CharacterReason_Succeeds()
    {
        // Arrange
        var inspection =
            CreateCompletedInspection();

        var reason =
            new string(
                'あ',
                500);

        // Act
        inspection.Return(
            reason,
            CreateUtcDate(
                2026,
                8,
                17,
                2));

        // Assert
        Assert.Equal(
            InspectionStatus.Returned,
            inspection.Status);

        Assert.Equal(
            500,
            inspection.ReturnReason!.Length);
    }


    [Fact]
    public void Return_With501CharacterReason_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var inspection =
            CreateCompletedInspection();

        var reason =
            new string(
                'あ',
                501);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => inspection.Return(
                    reason,
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        2)));

        // Assert
        Assert.Equal(
            "reason",
            exception.ParamName);
    }


    [Fact]
    public void Return_WhenNotCompleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var inspection =
            new Inspection(
                Guid.NewGuid());

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => inspection.Return(
                    "再確認してください。",
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        2)));

        // Assert
        Assert.Equal(
            "承認待ちの点検だけ差し戻しできます。",
            exception.Message);
    }


    // ============================================
    // Approve
    // ============================================

    [Fact]
    public void Approve_WhenCompleted_ChangesStatusToApproved()
    {
        // Arrange
        var inspection =
            CreateCompletedInspection();

        var reviewedAtUtc =
            CreateUtcDate(
                2026,
                8,
                17,
                2);

        // Act
        inspection.Approve(
            reviewedAtUtc);

        // Assert
        Assert.Equal(
            InspectionStatus.Approved,
            inspection.Status);

        Assert.Equal(
            reviewedAtUtc,
            inspection.ReviewedAtUtc);

        Assert.Null(
            inspection.ReturnReason);
    }


    [Fact]
    public void Approve_WhenNotCompleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var inspection =
            new Inspection(
                Guid.NewGuid());

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => inspection.Approve(
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        2)));

        // Assert
        Assert.Equal(
            "承認待ちの点検だけ承認できます。",
            exception.Message);
    }


    // ============================================
    // Test Helpers
    // ============================================

    private static Inspection CreateInProgressInspection()
    {
        var inspection =
            new Inspection(
                Guid.NewGuid());

        inspection.Start(
            Guid.NewGuid(),
            CreateUtcDate(
                2026,
                8,
                17,
                0));

        return inspection;
    }


    private static Inspection CreateCompletedInspection()
    {
        var inspection =
            CreateInProgressInspection();

        inspection.Complete(
            CreateUtcDate(
                2026,
                8,
                17,
                1));

        return inspection;
    }


    private static DateTime CreateUtcDate(
        int year,
        int month,
        int day,
        int hour)
    {
        return new DateTime(
            year,
            month,
            day,
            hour,
            0,
            0,
            DateTimeKind.Utc);
    }
}