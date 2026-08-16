using FacilityInspection.Domain.Locations;
using Xunit;

namespace FacilityInspection.Tests.Domain.Locations;

public sealed class LocationTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithValidValues_CreatesLocation()
    {
        // Arrange
        var factorySiteId =
            Guid.NewGuid();

        // Act
        var location =
            new Location(
                factorySiteId,
                "AREA-01",
                "コンプレッサー室",
                "1F",
                "第1工場北側");

        // Assert
        Assert.Equal(
            factorySiteId,
            location.FactorySiteId);

        Assert.Equal(
            "AREA-01",
            location.Code);

        Assert.Equal(
            "コンプレッサー室",
            location.Name);

        Assert.Equal(
            "1F",
            location.Floor);

        Assert.Equal(
            "第1工場北側",
            location.Description);

        Assert.True(
            location.IsActive);

        Assert.Empty(
            location.Equipments);
    }


    [Fact]
    public void Constructor_WithoutOptionalValues_SetsOptionalValuesToNull()
    {
        // Act
        var location =
            CreateLocation();

        // Assert
        Assert.Null(
            location.Floor);

        Assert.Null(
            location.Description);
    }


    // ============================================
    // FactorySiteId
    // ============================================

    [Fact]
    public void Constructor_WithEmptyFactorySiteId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Location(
                    Guid.Empty,
                    "AREA-01",
                    "コンプレッサー室"));

        // Assert
        Assert.Equal(
            "factorySiteId",
            exception.ParamName);
    }


    // ============================================
    // Code
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundCode_TrimsCode()
    {
        // Act
        var location =
            new Location(
                Guid.NewGuid(),
                "  AREA-01  ",
                "コンプレッサー室");

        // Assert
        Assert.Equal(
            "AREA-01",
            location.Code);
    }


    [Fact]
    public void Constructor_WithLowercaseCode_ConvertsCodeToUpperCase()
    {
        // Act
        var location =
            new Location(
                Guid.NewGuid(),
                "area-01",
                "コンプレッサー室");

        // Assert
        Assert.Equal(
            "AREA-01",
            location.Code);
    }


    [Fact]
    public void Constructor_WithNullCode_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new Location(
                    Guid.NewGuid(),
                    null!,
                    "コンプレッサー室"));

        // Assert
        Assert.Equal(
            "code",
            exception.ParamName);
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceCode_ThrowsArgumentException(
        string code)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Location(
                    Guid.NewGuid(),
                    code,
                    "コンプレッサー室"));

        // Assert
        Assert.Equal(
            "code",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_With30CharacterCode_Succeeds()
    {
        // Arrange
        var code =
            new string(
                'a',
                30);

        // Act
        var location =
            new Location(
                Guid.NewGuid(),
                code,
                "コンプレッサー室");

        // Assert
        Assert.Equal(
            30,
            location.Code.Length);

        Assert.Equal(
            code.ToUpperInvariant(),
            location.Code);
    }


    [Fact]
    public void Constructor_With31CharacterCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var code =
            new string(
                'a',
                31);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Location(
                    Guid.NewGuid(),
                    code,
                    "コンプレッサー室"));

        // Assert
        Assert.Equal(
            "code",
            exception.ParamName);
    }


    // ============================================
    // Name
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundName_TrimsName()
    {
        // Act
        var location =
            new Location(
                Guid.NewGuid(),
                "AREA-01",
                "  コンプレッサー室  ");

        // Assert
        Assert.Equal(
            "コンプレッサー室",
            location.Name);
    }


    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new Location(
                    Guid.NewGuid(),
                    "AREA-01",
                    null!));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceName_ThrowsArgumentException(
        string name)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Location(
                    Guid.NewGuid(),
                    "AREA-01",
                    name));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_With100CharacterName_Succeeds()
    {
        // Arrange
        var name =
            new string(
                'あ',
                100);

        // Act
        var location =
            new Location(
                Guid.NewGuid(),
                "AREA-01",
                name);

        // Assert
        Assert.Equal(
            100,
            location.Name.Length);
    }


    [Fact]
    public void Constructor_With101CharacterName_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var name =
            new string(
                'あ',
                101);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Location(
                    Guid.NewGuid(),
                    "AREA-01",
                    name));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);
    }


    // ============================================
    // Floor / Description
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundOptionalValues_TrimsValues()
    {
        // Act
        var location =
            new Location(
                Guid.NewGuid(),
                "AREA-01",
                "コンプレッサー室",
                "  1F  ",
                "  第1工場北側  ");

        // Assert
        Assert.Equal(
            "1F",
            location.Floor);

        Assert.Equal(
            "第1工場北側",
            location.Description);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOptionalValues_NormalizesToNull(
        string? value)
    {
        // Act
        var location =
            new Location(
                Guid.NewGuid(),
                "AREA-01",
                "コンプレッサー室",
                value,
                value);

        // Assert
        Assert.Null(
            location.Floor);

        Assert.Null(
            location.Description);
    }


    // ============================================
    // Update
    // ============================================

    [Fact]
    public void Update_WithValidValues_UpdatesLocation()
    {
        // Arrange
        var location =
            CreateLocation();

        // Act
        location.Update(
            "  area-02  ",
            "  ポンプ室  ",
            "  B1F  ",
            "  地下設備エリア  ");

        // Assert
        Assert.Equal(
            "AREA-02",
            location.Code);

        Assert.Equal(
            "ポンプ室",
            location.Name);

        Assert.Equal(
            "B1F",
            location.Floor);

        Assert.Equal(
            "地下設備エリア",
            location.Description);
    }


    [Fact]
    public void Update_WithEmptyOptionalValues_ClearsOptionalValues()
    {
        // Arrange
        var location =
            new Location(
                Guid.NewGuid(),
                "AREA-01",
                "コンプレッサー室",
                "1F",
                "第1工場北側");

        // Act
        location.Update(
            "AREA-01",
            "コンプレッサー室",
            null,
            "   ");

        // Assert
        Assert.Null(
            location.Floor);

        Assert.Null(
            location.Description);
    }


    [Fact]
    public void Update_WithInvalidCode_ThrowsArgumentException()
    {
        // Arrange
        var location =
            CreateLocation();

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => location.Update(
                    "",
                    "コンプレッサー室",
                    null,
                    null));

        // Assert
        Assert.Equal(
            "code",
            exception.ParamName);
    }


    [Fact]
    public void Update_WithInvalidName_ThrowsArgumentException()
    {
        // Arrange
        var location =
            CreateLocation();

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => location.Update(
                    "AREA-01",
                    "",
                    null,
                    null));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);
    }


    [Fact]
    public void Update_With31CharacterCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var location =
            CreateLocation();

        var code =
            new string(
                'a',
                31);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => location.Update(
                    code,
                    "コンプレッサー室",
                    null,
                    null));

        // Assert
        Assert.Equal(
            "code",
            exception.ParamName);
    }


    [Fact]
    public void Update_With101CharacterName_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var location =
            CreateLocation();

        var name =
            new string(
                'あ',
                101);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => location.Update(
                    "AREA-01",
                    name,
                    null,
                    null));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);
    }


    // ============================================
    // Active / Inactive
    // ============================================

    [Fact]
    public void Constructor_SetsIsActiveToTrue()
    {
        // Act
        var location =
            CreateLocation();

        // Assert
        Assert.True(
            location.IsActive);
    }


    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveToFalse()
    {
        // Arrange
        var location =
            CreateLocation();

        // Act
        location.Deactivate();

        // Assert
        Assert.False(
            location.IsActive);
    }


    [Fact]
    public void Activate_WhenInactive_SetsIsActiveToTrue()
    {
        // Arrange
        var location =
            CreateLocation();

        location.Deactivate();

        // Act
        location.Activate();

        // Assert
        Assert.True(
            location.IsActive);
    }


    [Fact]
    public void Deactivate_WhenAlreadyInactive_DoesNotThrow()
    {
        // Arrange
        var location =
            CreateLocation();

        location.Deactivate();

        // Act
        var exception =
            Record.Exception(
                () => location.Deactivate());

        // Assert
        Assert.Null(
            exception);

        Assert.False(
            location.IsActive);
    }


    [Fact]
    public void Activate_WhenAlreadyActive_DoesNotThrow()
    {
        // Arrange
        var location =
            CreateLocation();

        // Act
        var exception =
            Record.Exception(
                () => location.Activate());

        // Assert
        Assert.Null(
            exception);

        Assert.True(
            location.IsActive);
    }


    // ============================================
    // Equipments
    // ============================================

    [Fact]
    public void Constructor_InitializesEquipmentsAsEmptyCollection()
    {
        // Act
        var location =
            CreateLocation();

        // Assert
        Assert.NotNull(
            location.Equipments);

        Assert.Empty(
            location.Equipments);
    }


    // ============================================
    // Test Helper
    // ============================================

    private static Location CreateLocation()
    {
        return new Location(
            Guid.NewGuid(),
            "AREA-01",
            "コンプレッサー室");
    }
}