using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class CalendarDayViewModelTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var date =
            new DateOnly(
                2026,
                8,
                19);

        // Act
        var sut =
            new CalendarDayViewModel(
                date,
                true,
                5,
                2,
                3,
                _ =>
                {
                });

        // Assert
        Assert.Equal(
            date,
            sut.Date);

        Assert.Equal(
            19,
            sut.DayNumber);

        Assert.True(
            sut.IsCurrentMonth);

        Assert.Equal(
            5,
            sut.TotalScheduleCount);

        Assert.Equal(
            2,
            sut.OverdueCount);

        Assert.Equal(
            3,
            sut.CompletedCount);

        Assert.False(
            sut.IsSelected);
    }


    [Fact]
    public void Constructor_WithNullSelected_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new CalendarDayViewModel(
                        new DateOnly(
                            2026,
                            8,
                            19),
                        true,
                        0,
                        0,
                        0,
                        null!));

        // Assert
        Assert.Equal(
            "selected",
            exception.ParamName);
    }


    // ============================================
    // Today
    // ============================================

    [Fact]
    public void Constructor_WithToday_SetsIsTodayTrue()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                DateTime.Today);

        // Act
        var sut =
            CreateViewModel(
                date:
                    today);

        // Assert
        Assert.True(
            sut.IsToday);
    }


    [Fact]
    public void Constructor_WithDifferentDate_SetsIsTodayFalse()
    {
        // Arrange
        var yesterday =
            DateOnly.FromDateTime(
                DateTime.Today)
                .AddDays(-1);

        // Act
        var sut =
            CreateViewModel(
                date:
                    yesterday);

        // Assert
        Assert.False(
            sut.IsToday);
    }


    // ============================================
    // Schedule Count
    // ============================================

    [Theory]
    [InlineData(
        0,
        false,
        "")]
    [InlineData(
        1,
        true,
        "1件")]
    [InlineData(
        5,
        true,
        "5件")]
    public void ScheduleProperties_ReturnExpectedValues(
        int totalScheduleCount,
        bool expectedHasSchedules,
        string expectedText)
    {
        // Arrange
        var sut =
            CreateViewModel(
                totalScheduleCount:
                    totalScheduleCount);

        // Assert
        Assert.Equal(
            expectedHasSchedules,
            sut.HasSchedules);

        Assert.Equal(
            expectedText,
            sut.ScheduleCountText);
    }


    // ============================================
    // Summary
    // ============================================

    [Theory]
    [InlineData(
        5,
        2,
        3,
        "期限超過 2")]
    [InlineData(
        5,
        0,
        3,
        "完了 3")]
    [InlineData(
        5,
        0,
        0,
        "点検予定")]
    [InlineData(
        0,
        0,
        0,
        "")]
    public void SummaryText_ReturnsExpectedText(
        int totalScheduleCount,
        int overdueCount,
        int completedCount,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                totalScheduleCount:
                    totalScheduleCount,
                overdueCount:
                    overdueCount,
                completedCount:
                    completedCount);

        // Assert
        Assert.Equal(
            expected,
            sut.SummaryText);
    }


    // ============================================
    // Background
    // ============================================

    [Theory]
    [InlineData(
        true,
        false,
        "#FFFFFF")]
    [InlineData(
        false,
        false,
        "#F8FAFC")]
    [InlineData(
        true,
        true,
        "#DBEAFE")]
    [InlineData(
        false,
        true,
        "#DBEAFE")]
    public void BackgroundColor_ReturnsExpectedColor(
        bool isCurrentMonth,
        bool isSelected,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                isCurrentMonth:
                    isCurrentMonth);

        // Act
        sut.IsSelected =
            isSelected;

        // Assert
        Assert.Equal(
            expected,
            sut.BackgroundColor);
    }


    // ============================================
    // Border
    // ============================================

    [Fact]
    public void BorderColor_WhenSelected_ReturnsSelectedColor()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.IsSelected =
            true;

        // Assert
        Assert.Equal(
            "#2563EB",
            sut.BorderColor);
    }


    [Fact]
    public void BorderColor_WhenTodayAndNotSelected_ReturnsTodayColor()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                DateTime.Today);

        var sut =
            CreateViewModel(
                date:
                    today);

        // Assert
        Assert.False(
            sut.IsSelected);

        Assert.True(
            sut.IsToday);

        Assert.Equal(
            "#60A5FA",
            sut.BorderColor);
    }


    [Fact]
    public void BorderColor_WhenNormalDay_ReturnsDefaultColor()
    {
        // Arrange
        var date =
            DateOnly.FromDateTime(
                DateTime.Today)
                .AddDays(-1);

        var sut =
            CreateViewModel(
                date:
                    date);

        // Assert
        Assert.False(
            sut.IsToday);

        Assert.False(
            sut.IsSelected);

        Assert.Equal(
            "#E2E8F0",
            sut.BorderColor);
    }


    // ============================================
    // Foreground
    // ============================================

    [Theory]
    [InlineData(
        true,
        "#0F172A")]
    [InlineData(
        false,
        "#94A3B8")]
    public void DayForeground_ReturnsExpectedColor(
        bool isCurrentMonth,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                isCurrentMonth:
                    isCurrentMonth);

        // Assert
        Assert.Equal(
            expected,
            sut.DayForeground);
    }


    [Theory]
    [InlineData(
        1,
        1,
        "#DC2626")]
    [InlineData(
        0,
        1,
        "#15803D")]
    [InlineData(
        0,
        0,
        "#2563EB")]
    public void SummaryForeground_ReturnsExpectedColor(
        int overdueCount,
        int completedCount,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                overdueCount:
                    overdueCount,
                completedCount:
                    completedCount);

        // Assert
        Assert.Equal(
            expected,
            sut.SummaryForeground);
    }


    // ============================================
    // IsSelected
    // ============================================

    [Fact]
    public void IsSelected_WhenChanged_UpdatesDisplayColors()
    {
        // Arrange
        var date =
            DateOnly.FromDateTime(
                DateTime.Today)
                .AddDays(-1);

        var sut =
            CreateViewModel(
                date:
                    date,
                isCurrentMonth:
                    true);

        Assert.Equal(
            "#FFFFFF",
            sut.BackgroundColor);

        Assert.Equal(
            "#E2E8F0",
            sut.BorderColor);

        // Act
        sut.IsSelected =
            true;

        // Assert
        Assert.Equal(
            "#DBEAFE",
            sut.BackgroundColor);

        Assert.Equal(
            "#2563EB",
            sut.BorderColor);
    }


    // ============================================
    // Select
    // ============================================

    [Fact]
    public void SelectCommand_PassesItselfToSelectedCallback()
    {
        // Arrange
        CalendarDayViewModel?
            selectedItem = null;

        var sut =
            new CalendarDayViewModel(
                new DateOnly(
                    2026,
                    8,
                    19),
                true,
                3,
                0,
                1,
                item =>
                    selectedItem =
                        item);

        // Act
        sut.SelectCommand
            .Execute(null);

        // Assert
        Assert.Same(
            sut,
            selectedItem);
    }


    // ============================================
    // Helper
    // ============================================

    private static CalendarDayViewModel
        CreateViewModel(
            DateOnly? date = null,
            bool isCurrentMonth = true,
            int totalScheduleCount = 0,
            int overdueCount = 0,
            int completedCount = 0)
    {
        return new CalendarDayViewModel(
            date ??
                new DateOnly(
                    2026,
                    8,
                    19),

            isCurrentMonth,

            totalScheduleCount,

            overdueCount,

            completedCount,

            _ =>
            {
            });
    }
}