using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class InspectionEntryPhotoViewModelTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithBlankFileName_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentException>(
                () =>
                    new InspectionEntryPhotoViewModel(
                        "   ",
                        "photos/test.jpg",
                        DateTime.UtcNow,
                        _ =>
                        {
                        }));

        // Assert
        Assert.Equal(
            "fileName",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithBlankRelativePath_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentException>(
                () =>
                    new InspectionEntryPhotoViewModel(
                        "test.jpg",
                        "   ",
                        DateTime.UtcNow,
                        _ =>
                        {
                        }));

        // Assert
        Assert.Equal(
            "relativePath",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullRemoveRequested_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new InspectionEntryPhotoViewModel(
                        "test.jpg",
                        "photos/test.jpg",
                        DateTime.UtcNow,
                        null!));

        // Assert
        Assert.Equal(
            "removeRequested",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_TrimsValuesAndSetsProperties()
    {
        // Arrange
        var capturedAtUtc =
            new DateTime(
                2026,
                8,
                19,
                1,
                30,
                0,
                DateTimeKind.Utc);

        // Act
        var sut =
            new InspectionEntryPhotoViewModel(
                "  pressure.jpg  ",
                "  photos/pressure.jpg  ",
                capturedAtUtc,
                _ =>
                {
                });

        // Assert
        Assert.Equal(
            "pressure.jpg",
            sut.FileName);

        Assert.Equal(
            "photos/pressure.jpg",
            sut.RelativePath);

        Assert.Equal(
            capturedAtUtc,
            sut.CapturedAtUtc);

        Assert.NotNull(
            sut.RemoveCommand);
    }


    // ============================================
    // Completion Data
    // ============================================

    [Fact]
    public void ToCompletionData_ReturnsExpectedData()
    {
        // Arrange
        var capturedAtUtc =
            new DateTime(
                2026,
                8,
                19,
                2,
                0,
                0,
                DateTimeKind.Utc);

        var sut =
            new InspectionEntryPhotoViewModel(
                "test.jpg",
                "photos/test.jpg",
                capturedAtUtc,
                _ =>
                {
                });

        // Act
        var result =
            sut.ToCompletionData();

        // Assert
        Assert.Equal(
            "photos/test.jpg",
            result.RelativePath);

        Assert.Equal(
            capturedAtUtc,
            result.CapturedAtUtc);
    }


    // ============================================
    // Remove
    // ============================================

    [Fact]
    public void RemoveCommand_RequestsRemovalWithSelf()
    {
        // Arrange
        InspectionEntryPhotoViewModel?
            removedItem = null;

        var sut =
            new InspectionEntryPhotoViewModel(
                "test.jpg",
                "photos/test.jpg",
                DateTime.UtcNow,
                item =>
                    removedItem =
                        item);

        // Act
        sut.RemoveCommand
            .Execute(null);

        // Assert
        Assert.Same(
            sut,
            removedItem);
    }
}