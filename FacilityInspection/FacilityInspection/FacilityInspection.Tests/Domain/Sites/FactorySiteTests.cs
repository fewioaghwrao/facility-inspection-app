using FacilityInspection.Domain.Sites;
using Xunit;

namespace FacilityInspection.Tests.Domain.Sites;

public sealed class FactorySiteTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithValidValues_CreatesFactorySite()
    {
        // Act
        var factorySite =
            new FactorySite(
                "FACTORY-01",
                "第1工場",
                "主力生産工場");

        // Assert
        Assert.Equal(
            "FACTORY-01",
            factorySite.Code);

        Assert.Equal(
            "第1工場",
            factorySite.Name);

        Assert.Equal(
            "主力生産工場",
            factorySite.Description);

        Assert.True(
            factorySite.IsActive);

        Assert.Empty(
            factorySite.Locations);
    }


    [Fact]
    public void Constructor_WithoutDescription_SetsDescriptionToNull()
    {
        // Act
        var factorySite =
            CreateFactorySite();

        // Assert
        Assert.Null(
            factorySite.Description);
    }


    // ============================================
    // Code
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundCode_TrimsCode()
    {
        // Act
        var factorySite =
            new FactorySite(
                "  FACTORY-01  ",
                "第1工場");

        // Assert
        Assert.Equal(
            "FACTORY-01",
            factorySite.Code);
    }


    [Fact]
    public void Constructor_WithLowercaseCode_ConvertsCodeToUpperCase()
    {
        // Act
        var factorySite =
            new FactorySite(
                "factory-01",
                "第1工場");

        // Assert
        Assert.Equal(
            "FACTORY-01",
            factorySite.Code);
    }


    [Fact]
    public void Constructor_WithNullCode_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new FactorySite(
                    null!,
                    "第1工場"));

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
                () => new FactorySite(
                    code,
                    "第1工場"));

        // Assert
        Assert.Equal(
            "code",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_With20CharacterCode_Succeeds()
    {
        // Arrange
        var code =
            new string(
                'a',
                20);

        // Act
        var factorySite =
            new FactorySite(
                code,
                "第1工場");

        // Assert
        Assert.Equal(
            20,
            factorySite.Code.Length);

        Assert.Equal(
            code.ToUpperInvariant(),
            factorySite.Code);
    }


    [Fact]
    public void Constructor_With21CharacterCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var code =
            new string(
                'a',
                21);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new FactorySite(
                    code,
                    "第1工場"));

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
        var factorySite =
            new FactorySite(
                "FACTORY-01",
                "  第1工場  ");

        // Assert
        Assert.Equal(
            "第1工場",
            factorySite.Name);
    }


    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new FactorySite(
                    "FACTORY-01",
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
                () => new FactorySite(
                    "FACTORY-01",
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
        var factorySite =
            new FactorySite(
                "FACTORY-01",
                name);

        // Assert
        Assert.Equal(
            100,
            factorySite.Name.Length);
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
                () => new FactorySite(
                    "FACTORY-01",
                    name));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);
    }


    // ============================================
    // Description
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundDescription_TrimsDescription()
    {
        // Act
        var factorySite =
            new FactorySite(
                "FACTORY-01",
                "第1工場",
                "  主力生産工場  ");

        // Assert
        Assert.Equal(
            "主力生産工場",
            factorySite.Description);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyDescription_NormalizesToNull(
        string? description)
    {
        // Act
        var factorySite =
            new FactorySite(
                "FACTORY-01",
                "第1工場",
                description);

        // Assert
        Assert.Null(
            factorySite.Description);
    }


    // ============================================
    // Update
    // ============================================

    [Fact]
    public void Update_WithValidValues_UpdatesFactorySite()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        // Act
        factorySite.Update(
            "  factory-02  ",
            "  第2工場  ",
            "  西側工場  ");

        // Assert
        Assert.Equal(
            "FACTORY-02",
            factorySite.Code);

        Assert.Equal(
            "第2工場",
            factorySite.Name);

        Assert.Equal(
            "西側工場",
            factorySite.Description);
    }


    [Fact]
    public void Update_WithEmptyDescription_ClearsDescription()
    {
        // Arrange
        var factorySite =
            new FactorySite(
                "FACTORY-01",
                "第1工場",
                "説明あり");

        // Act
        factorySite.Update(
            "FACTORY-01",
            "第1工場",
            "   ");

        // Assert
        Assert.Null(
            factorySite.Description);
    }


    [Fact]
    public void Update_WithInvalidCode_ThrowsArgumentException()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => factorySite.Update(
                    "",
                    "第1工場",
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
        var factorySite =
            CreateFactorySite();

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => factorySite.Update(
                    "FACTORY-01",
                    "",
                    null));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);
    }


    [Fact]
    public void Update_With21CharacterCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        var code =
            new string(
                'a',
                21);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => factorySite.Update(
                    code,
                    "第1工場",
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
        var factorySite =
            CreateFactorySite();

        var name =
            new string(
                'あ',
                101);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => factorySite.Update(
                    "FACTORY-01",
                    name,
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
        var factorySite =
            CreateFactorySite();

        // Assert
        Assert.True(
            factorySite.IsActive);
    }


    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveToFalse()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        // Act
        factorySite.Deactivate();

        // Assert
        Assert.False(
            factorySite.IsActive);
    }


    [Fact]
    public void Activate_WhenInactive_SetsIsActiveToTrue()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        factorySite.Deactivate();

        // Act
        factorySite.Activate();

        // Assert
        Assert.True(
            factorySite.IsActive);
    }


    [Fact]
    public void Deactivate_WhenAlreadyInactive_DoesNotThrow()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        factorySite.Deactivate();

        // Act
        var exception =
            Record.Exception(
                () => factorySite.Deactivate());

        // Assert
        Assert.Null(
            exception);

        Assert.False(
            factorySite.IsActive);
    }


    [Fact]
    public void Activate_WhenAlreadyActive_DoesNotThrow()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        // Act
        var exception =
            Record.Exception(
                () => factorySite.Activate());

        // Assert
        Assert.Null(
            exception);

        Assert.True(
            factorySite.IsActive);
    }


    // ============================================
    // Locations
    // ============================================

    [Fact]
    public void Constructor_InitializesLocationsAsEmptyCollection()
    {
        // Act
        var factorySite =
            CreateFactorySite();

        // Assert
        Assert.NotNull(
            factorySite.Locations);

        Assert.Empty(
            factorySite.Locations);
    }


    // ============================================
    // Test Helper
    // ============================================

    private static FactorySite CreateFactorySite()
    {
        return new FactorySite(
            "FACTORY-01",
            "第1工場");
    }
}