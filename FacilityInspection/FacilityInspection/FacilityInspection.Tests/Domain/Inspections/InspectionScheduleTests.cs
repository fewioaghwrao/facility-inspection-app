using FacilityInspection.Domain.Inspections;
using Xunit;

namespace FacilityInspection.Tests.Domain.Inspections;

public sealed class InspectionScheduleTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithValidValues_CreatesInspectionSchedule()
    {
        // Arrange
        var scheduledDate =
            new DateOnly(
                2026,
                8,
                20);

        var equipmentId =
            Guid.NewGuid();

        var inspectionTemplateId =
            Guid.NewGuid();

        var assignedOperatorId =
            Guid.NewGuid();

        // Act
        var schedule =
            new InspectionSchedule(
                scheduledDate,
                equipmentId,
                inspectionTemplateId,
                assignedOperatorId,
                "月次点検");

        // Assert
        Assert.Equal(
            scheduledDate,
            schedule.ScheduledDate);

        Assert.Equal(
            equipmentId,
            schedule.EquipmentId);

        Assert.Equal(
            inspectionTemplateId,
            schedule.InspectionTemplateId);

        Assert.Equal(
            assignedOperatorId,
            schedule.AssignedOperatorId);

        Assert.Equal(
            "月次点検",
            schedule.Notes);

        Assert.False(
            schedule.IsCancelled);

        Assert.Null(
            schedule.Inspection);
    }


    [Fact]
    public void Constructor_WithDefaultScheduledDate_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionSchedule(
                    default,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid()));

        // Assert
        Assert.Equal(
            "scheduledDate",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithEmptyEquipmentId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionSchedule(
                    new DateOnly(
                        2026,
                        8,
                        20),
                    Guid.Empty,
                    Guid.NewGuid(),
                    Guid.NewGuid()));

        // Assert
        Assert.Equal(
            "equipmentId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithEmptyInspectionTemplateId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionSchedule(
                    new DateOnly(
                        2026,
                        8,
                        20),
                    Guid.NewGuid(),
                    Guid.Empty,
                    Guid.NewGuid()));

        // Assert
        Assert.Equal(
            "inspectionTemplateId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithEmptyAssignedOperatorId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionSchedule(
                    new DateOnly(
                        2026,
                        8,
                        20),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.Empty));

        // Assert
        Assert.Equal(
            "assignedOperatorId",
            exception.ParamName);
    }


    // ============================================
    // Notes
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundNotes_TrimsNotes()
    {
        // Act
        var schedule =
            CreateSchedule(
                "  月次点検です  ");

        // Assert
        Assert.Equal(
            "月次点検です",
            schedule.Notes);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyNotes_NormalizesToNull(
        string? notes)
    {
        // Act
        var schedule =
            CreateSchedule(
                notes);

        // Assert
        Assert.Null(
            schedule.Notes);
    }


    [Fact]
    public void Constructor_With500CharacterNotes_Succeeds()
    {
        // Arrange
        var notes =
            new string(
                'あ',
                500);

        // Act
        var schedule =
            CreateSchedule(
                notes);

        // Assert
        Assert.NotNull(
            schedule.Notes);

        Assert.Equal(
            500,
            schedule.Notes.Length);
    }


    [Fact]
    public void Constructor_With501CharacterNotes_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var notes =
            new string(
                'あ',
                501);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSchedule(
                    notes));

        // Assert
        Assert.Equal(
            "notes",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithSpacesAnd500CharacterNotes_SucceedsAfterTrim()
    {
        // Arrange
        var notes =
            "  " +
            new string(
                'あ',
                500) +
            "  ";

        // Act
        var schedule =
            CreateSchedule(
                notes);

        // Assert
        Assert.NotNull(
            schedule.Notes);

        Assert.Equal(
            500,
            schedule.Notes.Length);
    }


    // ============================================
    // Update
    // ============================================

    [Fact]
    public void Update_WhenNotCancelled_UpdatesAllValues()
    {
        // Arrange
        var schedule =
            CreateSchedule(
                "変更前");

        var newScheduledDate =
            new DateOnly(
                2026,
                9,
                10);

        var newEquipmentId =
            Guid.NewGuid();

        var newTemplateId =
            Guid.NewGuid();

        var newOperatorId =
            Guid.NewGuid();

        // Act
        schedule.Update(
            newScheduledDate,
            newEquipmentId,
            newTemplateId,
            newOperatorId,
            "変更後");

        // Assert
        Assert.Equal(
            newScheduledDate,
            schedule.ScheduledDate);

        Assert.Equal(
            newEquipmentId,
            schedule.EquipmentId);

        Assert.Equal(
            newTemplateId,
            schedule.InspectionTemplateId);

        Assert.Equal(
            newOperatorId,
            schedule.AssignedOperatorId);

        Assert.Equal(
            "変更後",
            schedule.Notes);
    }


    [Fact]
    public void Update_WithSpacesAroundNotes_TrimsNotes()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        // Act
        schedule.Update(
            new DateOnly(
                2026,
                9,
                10),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  更新後の備考  ");

        // Assert
        Assert.Equal(
            "更新後の備考",
            schedule.Notes);
    }


    [Fact]
    public void Update_WithEmptyNotes_SetsNotesToNull()
    {
        // Arrange
        var schedule =
            CreateSchedule(
                "変更前");

        // Act
        schedule.Update(
            new DateOnly(
                2026,
                9,
                10),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "   ");

        // Assert
        Assert.Null(
            schedule.Notes);
    }


    [Fact]
    public void Update_WithDefaultScheduledDate_ThrowsArgumentException()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => schedule.Update(
                    default,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    null));

        // Assert
        Assert.Equal(
            "scheduledDate",
            exception.ParamName);
    }


    [Fact]
    public void Update_WithEmptyEquipmentId_ThrowsArgumentException()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => schedule.Update(
                    new DateOnly(
                        2026,
                        9,
                        10),
                    Guid.Empty,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    null));

        // Assert
        Assert.Equal(
            "equipmentId",
            exception.ParamName);
    }


    [Fact]
    public void Update_WithEmptyInspectionTemplateId_ThrowsArgumentException()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => schedule.Update(
                    new DateOnly(
                        2026,
                        9,
                        10),
                    Guid.NewGuid(),
                    Guid.Empty,
                    Guid.NewGuid(),
                    null));

        // Assert
        Assert.Equal(
            "inspectionTemplateId",
            exception.ParamName);
    }


    [Fact]
    public void Update_WithEmptyAssignedOperatorId_ThrowsArgumentException()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => schedule.Update(
                    new DateOnly(
                        2026,
                        9,
                        10),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.Empty,
                    null));

        // Assert
        Assert.Equal(
            "assignedOperatorId",
            exception.ParamName);
    }


    [Fact]
    public void Update_With501CharacterNotes_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        var notes =
            new string(
                'あ',
                501);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => schedule.Update(
                    new DateOnly(
                        2026,
                        9,
                        10),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    notes));

        // Assert
        Assert.Equal(
            "notes",
            exception.ParamName);
    }


    // ============================================
    // Cancel
    // ============================================

    [Fact]
    public void Cancel_WhenNotCancelled_SetsIsCancelledToTrue()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        // Act
        schedule.Cancel();

        // Assert
        Assert.True(
            schedule.IsCancelled);
    }


    [Fact]
    public void Cancel_WhenAlreadyCancelled_DoesNotThrow()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        schedule.Cancel();

        // Act
        var exception =
            Record.Exception(
                () => schedule.Cancel());

        // Assert
        Assert.Null(
            exception);

        Assert.True(
            schedule.IsCancelled);
    }


    [Fact]
    public void Update_WhenCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        schedule.Cancel();

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => schedule.Update(
                    new DateOnly(
                        2026,
                        9,
                        10),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "更新"));

        // Assert
        Assert.Equal(
            "取消済みの点検予定は変更できません。",
            exception.Message);
    }


    // ============================================
    // AttachInspection
    // ============================================

    [Fact]
    public void AttachInspection_WithMatchingScheduleId_AttachesInspection()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        var inspection =
            new Inspection(
                schedule.Id);

        // Act
        schedule.AttachInspection(
            inspection);

        // Assert
        Assert.Same(
            inspection,
            schedule.Inspection);

        Assert.Equal(
            schedule.Id,
            schedule.Inspection.InspectionScheduleId);
    }


    [Fact]
    public void AttachInspection_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => schedule.AttachInspection(
                    null!));

        // Assert
        Assert.Equal(
            "inspection",
            exception.ParamName);
    }


    [Fact]
    public void AttachInspection_WithDifferentScheduleId_ThrowsInvalidOperationException()
    {
        // Arrange
        var schedule =
            CreateSchedule();

        var inspection =
            new Inspection(
                Guid.NewGuid());

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => schedule.AttachInspection(
                    inspection));

        // Assert
        Assert.Equal(
            "点検予定と点検記録のIDが一致しません。",
            exception.Message);

        Assert.Null(
            schedule.Inspection);
    }


    // ============================================
    // Test Helper
    // ============================================

    private static InspectionSchedule CreateSchedule(
        string? notes = null)
    {
        return new InspectionSchedule(
            new DateOnly(
                2026,
                8,
                20),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            notes);
    }
}
