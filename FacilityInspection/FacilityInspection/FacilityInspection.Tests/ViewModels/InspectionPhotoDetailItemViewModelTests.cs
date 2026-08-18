using FacilityInspection.Data;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class InspectionPhotoDetailItemViewModelTests
{
    private static readonly Guid
        PhotoId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");

    private static readonly Guid
        InspectionResultId =
            Guid.Parse(
                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new InspectionPhotoDetailItemViewModel(
                        null!));


        // Assert
        Assert.Equal(
            "source",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_SetsProperties()
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

        var source =
            CreateData(
                photoId:
                    PhotoId,
                inspectionResultId:
                    InspectionResultId,
                relativePath:
                    CreateMissingPath(),
                caption:
                    "圧力計の写真",
                displayOrder:
                    2,
                capturedAtUtc:
                    capturedAtUtc);


        // Act
        var sut =
            new InspectionPhotoDetailItemViewModel(
                source);


        // Assert
        Assert.Equal(
            PhotoId,
            sut.PhotoId);

        Assert.Equal(
            InspectionResultId,
            sut.InspectionResultId);

        Assert.Equal(
            source.RelativePath,
            sut.RelativePath);

        Assert.Equal(
            "圧力計の写真",
            sut.Caption);

        Assert.Equal(
            2,
            sut.DisplayOrder);

        Assert.Equal(
            capturedAtUtc,
            sut.CapturedAtUtc);
    }


    // ============================================
    // Caption
    // ============================================

    [Fact]
    public void Constructor_WhenCaptionIsNull_UsesEmptyString()
    {
        // Arrange
        var source =
            CreateData(
                caption:
                    null);


        // Act
        var sut =
            new InspectionPhotoDetailItemViewModel(
                source);


        // Assert
        Assert.Equal(
            string.Empty,
            sut.Caption);

        Assert.False(
            sut.HasCaption);
    }


    [Fact]
    public void HasCaption_ReturnsExpectedValue()
    {
        // Arrange
        var withCaption =
            new InspectionPhotoDetailItemViewModel(
                CreateData(
                    caption:
                        "異常箇所"));

        var withoutCaption =
            new InspectionPhotoDetailItemViewModel(
                CreateData(
                    caption:
                        "   "));


        // Assert
        Assert.True(
            withCaption.HasCaption);

        Assert.False(
            withoutCaption.HasCaption);
    }


    // ============================================
    // General Photo
    // ============================================

    [Fact]
    public void IsGeneralPhoto_WhenInspectionResultIdIsNull_ReturnsTrue()
    {
        // Arrange
        var sut =
            new InspectionPhotoDetailItemViewModel(
                CreateData(
                    inspectionResultId:
                        null));


        // Assert
        Assert.True(
            sut.IsGeneralPhoto);
    }


    [Fact]
    public void IsGeneralPhoto_WhenInspectionResultIdExists_ReturnsFalse()
    {
        // Arrange
        var sut =
            new InspectionPhotoDetailItemViewModel(
                CreateData(
                    inspectionResultId:
                        InspectionResultId));


        // Assert
        Assert.False(
            sut.IsGeneralPhoto);
    }


    // ============================================
    // Captured Time
    // ============================================

    [Fact]
    public void CapturedAtText_ReturnsLocalTimeFormattedText()
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

        var sut =
            new InspectionPhotoDetailItemViewModel(
                CreateData(
                    capturedAtUtc:
                        capturedAtUtc));


        var expected =
            capturedAtUtc
                .ToLocalTime()
                .ToString(
                    "yyyy/MM/dd HH:mm");


        // Assert
        Assert.Equal(
            expected,
            sut.CapturedAtText);
    }


    // ============================================
    // Photo Source
    // ============================================

    [Fact]
    public void PhotoSource_WhenFileDoesNotExist_ReturnsNull()
    {
        // Arrange
        var missingPath =
            CreateMissingPath();

        var source =
            CreateData(
                relativePath:
                    missingPath);


        // Act
        var sut =
            new InspectionPhotoDetailItemViewModel(
                source);


        // Assert
        Assert.Null(
            sut.PhotoSource);
    }


    // ============================================
    // Helper
    // ============================================

    private static InspectionPhotoDetailData
        CreateData(
            Guid? photoId = null,
            Guid? inspectionResultId = null,
            string? relativePath = null,
            string? caption =
                "点検写真",
            int displayOrder = 1,
            DateTime? capturedAtUtc = null)
    {
        return new InspectionPhotoDetailData(
            PhotoId:
                photoId ??
                PhotoId,

            InspectionResultId:
                inspectionResultId,

            RelativePath:
                relativePath ??
                CreateMissingPath(),

            Caption:
                caption,

            DisplayOrder:
                displayOrder,

            CapturedAtUtc:
                capturedAtUtc ??
                new DateTime(
                    2026,
                    8,
                    19,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc));
    }


    private static string
        CreateMissingPath()
    {
        /*
         * AppContext.BaseDirectory配下に存在しない
         * ランダムな相対パスを生成する。
         *
         * 実ファイルの作成・削除は行わない。
         */
        return
            $"__test_missing_photos__/" +
            $"{Guid.NewGuid():N}.jpg";
    }
}
