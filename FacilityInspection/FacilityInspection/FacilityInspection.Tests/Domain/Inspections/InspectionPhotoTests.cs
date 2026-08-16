using FacilityInspection.Domain.Inspections;

namespace FacilityInspection.Tests.Domain.Inspections;

public sealed class InspectionPhotoTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithValidValues_CreatesInspectionPhoto()
    {
        // Arrange
        var inspectionId =
            Guid.NewGuid();

        var inspectionResultId =
            Guid.NewGuid();

        var capturedAtUtc =
            CreateUtcDate(
                2026,
                8,
                17,
                1);

        // Act
        var photo =
            new InspectionPhoto(
                inspectionId,
                "sample/images/resultcomp1.jpg",
                capturedAtUtc,
                2,
                inspectionResultId,
                "点検結果写真");

        // Assert
        Assert.Equal(
            inspectionId,
            photo.InspectionId);

        Assert.Equal(
            inspectionResultId,
            photo.InspectionResultId);

        Assert.Equal(
            "sample/images/resultcomp1.jpg",
            photo.RelativePath);

        Assert.Equal(
            capturedAtUtc,
            photo.CapturedAtUtc);

        Assert.Equal(
            2,
            photo.DisplayOrder);

        Assert.Equal(
            "点検結果写真",
            photo.Caption);
    }


    [Fact]
    public void Constructor_WithNullInspectionResultId_AllowsPhotoForWholeInspection()
    {
        // Arrange
        var inspectionId =
            Guid.NewGuid();

        // Act
        var photo =
            new InspectionPhoto(
                inspectionId,
                "sample/images/resultcomp1.jpg",
                CreateUtcDate(
                    2026,
                    8,
                    17,
                    1));

        // Assert
        Assert.Null(
            photo.InspectionResultId);

        Assert.Equal(
            0,
            photo.DisplayOrder);

        Assert.Null(
            photo.Caption);
    }


    [Fact]
    public void Constructor_WithEmptyInspectionId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionPhoto(
                    Guid.Empty,
                    "sample/images/resultcomp1.jpg",
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        1)));

        // Assert
        Assert.Equal(
            "inspectionId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithEmptyInspectionResultId_ThrowsArgumentException()
    {
        // Arrange
        Guid? inspectionResultId =
            Guid.Empty;

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionPhoto(
                    Guid.NewGuid(),
                    "sample/images/resultcomp1.jpg",
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        1),
                    inspectionResultId:
                        inspectionResultId));

        // Assert
        Assert.Equal(
            "inspectionResultId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNegativeDisplayOrder_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new InspectionPhoto(
                    Guid.NewGuid(),
                    "sample/images/resultcomp1.jpg",
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        1),
                    displayOrder:
                        -1));

        // Assert
        Assert.Equal(
            "displayOrder",
            exception.ParamName);
    }


    // ============================================
    // RelativePath
    // ============================================

    [Fact]
    public void Constructor_WithBackslashPath_ConvertsToForwardSlash()
    {
        // Act
        var photo =
            new InspectionPhoto(
                Guid.NewGuid(),
                @"sample\images\resultcomp1.jpg",
                CreateUtcDate(
                    2026,
                    8,
                    17,
                    1));

        // Assert
        Assert.Equal(
            "sample/images/resultcomp1.jpg",
            photo.RelativePath);
    }


    [Fact]
    public void Constructor_WithPathContainingSpaces_TrimsPath()
    {
        // Act
        var photo =
            new InspectionPhoto(
                Guid.NewGuid(),
                "  sample/images/resultcomp1.jpg  ",
                CreateUtcDate(
                    2026,
                    8,
                    17,
                    1));

        // Assert
        Assert.Equal(
            "sample/images/resultcomp1.jpg",
            photo.RelativePath);
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespacePath_ThrowsArgumentException(
        string relativePath)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new InspectionPhoto(
                Guid.NewGuid(),
                relativePath,
                CreateUtcDate(
                    2026,
                    8,
                    17,
                    1)));
    }


    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new InspectionPhoto(
                    Guid.NewGuid(),
                    null!,
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        1)));

        // Assert
        Assert.Equal(
            "relativePath",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_With500CharacterPath_Succeeds()
    {
        // Arrange
        var relativePath =
            new string(
                'a',
                500);

        // Act
        var photo =
            new InspectionPhoto(
                Guid.NewGuid(),
                relativePath,
                CreateUtcDate(
                    2026,
                    8,
                    17,
                    1));

        // Assert
        Assert.Equal(
            500,
            photo.RelativePath.Length);
    }


    [Fact]
    public void Constructor_With501CharacterPath_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var relativePath =
            new string(
                'a',
                501);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new InspectionPhoto(
                    Guid.NewGuid(),
                    relativePath,
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        1)));

        // Assert
        Assert.Equal(
            "relativePath",
            exception.ParamName);
    }


    [Theory]
    [InlineData("/sample/images/photo.jpg")]
    [InlineData("C:/sample/images/photo.jpg")]
    [InlineData("C:\\sample\\images\\photo.jpg")]
    [InlineData("../images/photo.jpg")]
    [InlineData("sample/../images/photo.jpg")]
    [InlineData("sample/images/..")]
    public void Constructor_WithInvalidRelativePath_ThrowsArgumentException(
        string relativePath)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionPhoto(
                    Guid.NewGuid(),
                    relativePath,
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        1)));

        // Assert
        Assert.Equal(
            "relativePath",
            exception.ParamName);
    }


    // ============================================
    // Caption
    // ============================================

    [Fact]
    public void Constructor_WithCaptionContainingSpaces_TrimsCaption()
    {
        // Act
        var photo =
            new InspectionPhoto(
                Guid.NewGuid(),
                "sample/images/photo.jpg",
                CreateUtcDate(
                    2026,
                    8,
                    17,
                    1),
                caption:
                    "  異常箇所の写真  ");

        // Assert
        Assert.Equal(
            "異常箇所の写真",
            photo.Caption);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyCaption_NormalizesToNull(
        string? caption)
    {
        // Act
        var photo =
            new InspectionPhoto(
                Guid.NewGuid(),
                "sample/images/photo.jpg",
                CreateUtcDate(
                    2026,
                    8,
                    17,
                    1),
                caption:
                    caption);

        // Assert
        Assert.Null(
            photo.Caption);
    }


    [Fact]
    public void Constructor_With200CharacterCaption_Succeeds()
    {
        // Arrange
        var caption =
            new string(
                'あ',
                200);

        // Act
        var photo =
            new InspectionPhoto(
                Guid.NewGuid(),
                "sample/images/photo.jpg",
                CreateUtcDate(
                    2026,
                    8,
                    17,
                    1),
                caption:
                    caption);

        // Assert
        Assert.Equal(
            200,
            photo.Caption!.Length);
    }


    [Fact]
    public void Constructor_With201CharacterCaption_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var caption =
            new string(
                'あ',
                201);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new InspectionPhoto(
                    Guid.NewGuid(),
                    "sample/images/photo.jpg",
                    CreateUtcDate(
                        2026,
                        8,
                        17,
                        1),
                    caption:
                        caption));

        // Assert
        Assert.Equal(
            "caption",
            exception.ParamName);
    }


    // ============================================
    // ChangeCaption
    // ============================================

    [Fact]
    public void ChangeCaption_WithValidCaption_ChangesCaption()
    {
        // Arrange
        var photo =
            CreatePhoto();

        // Act
        photo.ChangeCaption(
            "異常箇所を拡大した写真");

        // Assert
        Assert.Equal(
            "異常箇所を拡大した写真",
            photo.Caption);
    }


    [Fact]
    public void ChangeCaption_WithSpaces_TrimsCaption()
    {
        // Arrange
        var photo =
            CreatePhoto();

        // Act
        photo.ChangeCaption(
            "  異常箇所を拡大した写真  ");

        // Assert
        Assert.Equal(
            "異常箇所を拡大した写真",
            photo.Caption);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangeCaption_WithEmptyCaption_ChangesCaptionToNull(
        string? caption)
    {
        // Arrange
        var photo =
            CreatePhoto();

        // Act
        photo.ChangeCaption(
            caption);

        // Assert
        Assert.Null(
            photo.Caption);
    }


    [Fact]
    public void ChangeCaption_With201CharacterCaption_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var photo =
            CreatePhoto();

        var caption =
            new string(
                'あ',
                201);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => photo.ChangeCaption(
                    caption));

        // Assert
        Assert.Equal(
            "caption",
            exception.ParamName);
    }


    // ============================================
    // ChangeDisplayOrder
    // ============================================

    [Fact]
    public void ChangeDisplayOrder_WithValidValue_ChangesDisplayOrder()
    {
        // Arrange
        var photo =
            CreatePhoto();

        // Act
        photo.ChangeDisplayOrder(
            5);

        // Assert
        Assert.Equal(
            5,
            photo.DisplayOrder);
    }


    [Fact]
    public void ChangeDisplayOrder_WithZero_Succeeds()
    {
        // Arrange
        var photo =
            CreatePhoto();

        photo.ChangeDisplayOrder(
            5);

        // Act
        photo.ChangeDisplayOrder(
            0);

        // Assert
        Assert.Equal(
            0,
            photo.DisplayOrder);
    }


    [Fact]
    public void ChangeDisplayOrder_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var photo =
            CreatePhoto();

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => photo.ChangeDisplayOrder(
                    -1));

        // Assert
        Assert.Equal(
            "displayOrder",
            exception.ParamName);
    }


    // ============================================
    // Test Helpers
    // ============================================

    private static InspectionPhoto CreatePhoto()
    {
        return new InspectionPhoto(
            Guid.NewGuid(),
            "sample/images/photo.jpg",
            CreateUtcDate(
                2026,
                8,
                17,
                1),
            displayOrder:
                0,
            caption:
                "点検写真");
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