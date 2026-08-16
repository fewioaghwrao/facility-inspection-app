using FacilityInspection.Domain.Equipments;
using Xunit;

namespace FacilityInspection.Tests.Domain.Equipments;

public sealed class EquipmentTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithValidValues_CreatesEquipment()
    {
        // Arrange
        var locationId =
            Guid.NewGuid();

        var installedOn =
            new DateOnly(
                2025,
                4,
                1);

        var equipmentType =
            GetValidEquipmentType();

        // Act
        var equipment =
            new Equipment(
                locationId,
                "CMP-001",
                "第1コンプレッサー",
                equipmentType,
                manufacturer: "メーカーA",
                modelNumber: "MODEL-001",
                serialNumber: "SN-001",
                installedOn: installedOn,
                notes: "第1工場設備");

        // Assert
        Assert.Equal(
            locationId,
            equipment.LocationId);

        Assert.Equal(
            "CMP-001",
            equipment.EquipmentCode);

        Assert.Equal(
            "第1コンプレッサー",
            equipment.Name);

        Assert.Equal(
            equipmentType,
            equipment.EquipmentType);

        Assert.Equal(
            "メーカーA",
            equipment.Manufacturer);

        Assert.Equal(
            "MODEL-001",
            equipment.ModelNumber);

        Assert.Equal(
            "SN-001",
            equipment.SerialNumber);

        Assert.Equal(
            installedOn,
            equipment.InstalledOn);

        Assert.Equal(
            "第1工場設備",
            equipment.Notes);

        Assert.Equal(
            EquipmentStatus.InService,
            equipment.Status);
    }


    [Fact]
    public void Constructor_WithoutOptionalValues_SetsOptionalValuesToNull()
    {
        // Act
        var equipment =
            CreateEquipment();

        // Assert
        Assert.Null(
            equipment.Manufacturer);

        Assert.Null(
            equipment.ModelNumber);

        Assert.Null(
            equipment.SerialNumber);

        Assert.Null(
            equipment.InstalledOn);

        Assert.Null(
            equipment.Notes);
    }


    // ============================================
    // LocationId
    // ============================================

    [Fact]
    public void Constructor_WithEmptyLocationId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Equipment(
                    Guid.Empty,
                    "CMP-001",
                    "コンプレッサー",
                    GetValidEquipmentType()));

        // Assert
        Assert.Equal(
            "locationId",
            exception.ParamName);
    }


    // ============================================
    // EquipmentCode
    // ============================================

    [Fact]
    public void Constructor_WithEquipmentCode_TrimsAndConvertsToUpperCase()
    {
        // Act
        var equipment =
            new Equipment(
                Guid.NewGuid(),
                "  cmp-001  ",
                "コンプレッサー",
                GetValidEquipmentType());

        // Assert
        Assert.Equal(
            "CMP-001",
            equipment.EquipmentCode);
    }


    [Fact]
    public void Constructor_WithNullEquipmentCode_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new Equipment(
                    Guid.NewGuid(),
                    null!,
                    "コンプレッサー",
                    GetValidEquipmentType()));

        // Assert
        Assert.Equal(
            "equipmentCode",
            exception.ParamName);
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceEquipmentCode_ThrowsArgumentException(
        string equipmentCode)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Equipment(
                    Guid.NewGuid(),
                    equipmentCode,
                    "コンプレッサー",
                    GetValidEquipmentType()));

        // Assert
        Assert.Equal(
            "equipmentCode",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_With30CharacterEquipmentCode_Succeeds()
    {
        // Arrange
        var equipmentCode =
            new string(
                'a',
                30);

        // Act
        var equipment =
            new Equipment(
                Guid.NewGuid(),
                equipmentCode,
                "コンプレッサー",
                GetValidEquipmentType());

        // Assert
        Assert.Equal(
            30,
            equipment.EquipmentCode.Length);

        Assert.Equal(
            equipmentCode.ToUpperInvariant(),
            equipment.EquipmentCode);
    }


    [Fact]
    public void Constructor_With31CharacterEquipmentCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var equipmentCode =
            new string(
                'a',
                31);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Equipment(
                    Guid.NewGuid(),
                    equipmentCode,
                    "コンプレッサー",
                    GetValidEquipmentType()));

        // Assert
        Assert.Equal(
            "equipmentCode",
            exception.ParamName);
    }


    // ============================================
    // Name
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundName_TrimsName()
    {
        // Act
        var equipment =
            new Equipment(
                Guid.NewGuid(),
                "CMP-001",
                "  第1コンプレッサー  ",
                GetValidEquipmentType());

        // Assert
        Assert.Equal(
            "第1コンプレッサー",
            equipment.Name);
    }


    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new Equipment(
                    Guid.NewGuid(),
                    "CMP-001",
                    null!,
                    GetValidEquipmentType()));

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
                () => new Equipment(
                    Guid.NewGuid(),
                    "CMP-001",
                    name,
                    GetValidEquipmentType()));

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
        var equipment =
            new Equipment(
                Guid.NewGuid(),
                "CMP-001",
                name,
                GetValidEquipmentType());

        // Assert
        Assert.Equal(
            100,
            equipment.Name.Length);
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
                () => new Equipment(
                    Guid.NewGuid(),
                    "CMP-001",
                    name,
                    GetValidEquipmentType()));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);
    }


    // ============================================
    // Optional values
    // ============================================

    [Fact]
    public void Constructor_WithSpacesAroundOptionalValues_TrimsValues()
    {
        // Act
        var equipment =
            new Equipment(
                Guid.NewGuid(),
                "CMP-001",
                "コンプレッサー",
                GetValidEquipmentType(),
                manufacturer:
                    "  メーカーA  ",
                modelNumber:
                    "  MODEL-001  ",
                serialNumber:
                    "  SN-001  ",
                notes:
                    "  月次点検対象  ");

        // Assert
        Assert.Equal(
            "メーカーA",
            equipment.Manufacturer);

        Assert.Equal(
            "MODEL-001",
            equipment.ModelNumber);

        Assert.Equal(
            "SN-001",
            equipment.SerialNumber);

        Assert.Equal(
            "月次点検対象",
            equipment.Notes);
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
        var equipment =
            new Equipment(
                Guid.NewGuid(),
                "CMP-001",
                "コンプレッサー",
                GetValidEquipmentType(),
                manufacturer:
                    value,
                modelNumber:
                    value,
                serialNumber:
                    value,
                notes:
                    value);

        // Assert
        Assert.Null(
            equipment.Manufacturer);

        Assert.Null(
            equipment.ModelNumber);

        Assert.Null(
            equipment.SerialNumber);

        Assert.Null(
            equipment.Notes);
    }


    [Fact]
    public void Constructor_With100CharacterManufacturer_Succeeds()
    {
        // Arrange
        var manufacturer =
            new string(
                'あ',
                100);

        // Act
        var equipment =
            new Equipment(
                Guid.NewGuid(),
                "CMP-001",
                "コンプレッサー",
                GetValidEquipmentType(),
                manufacturer:
                    manufacturer);

        // Assert
        Assert.Equal(
            100,
            equipment.Manufacturer!.Length);
    }


    [Fact]
    public void Constructor_With101CharacterManufacturer_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var manufacturer =
            new string(
                'あ',
                101);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Equipment(
                Guid.NewGuid(),
                "CMP-001",
                "コンプレッサー",
                GetValidEquipmentType(),
                manufacturer:
                    manufacturer));
    }


    [Fact]
    public void Constructor_With101CharacterModelNumber_ThrowsArgumentOutOfRangeException()
    {
        var modelNumber =
            new string(
                'a',
                101);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Equipment(
                Guid.NewGuid(),
                "CMP-001",
                "コンプレッサー",
                GetValidEquipmentType(),
                modelNumber:
                    modelNumber));
    }


    [Fact]
    public void Constructor_With101CharacterSerialNumber_ThrowsArgumentOutOfRangeException()
    {
        var serialNumber =
            new string(
                'a',
                101);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Equipment(
                Guid.NewGuid(),
                "CMP-001",
                "コンプレッサー",
                GetValidEquipmentType(),
                serialNumber:
                    serialNumber));
    }


    [Fact]
    public void Constructor_With1000CharacterNotes_Succeeds()
    {
        // Arrange
        var notes =
            new string(
                'あ',
                1000);

        // Act
        var equipment =
            new Equipment(
                Guid.NewGuid(),
                "CMP-001",
                "コンプレッサー",
                GetValidEquipmentType(),
                notes:
                    notes);

        // Assert
        Assert.Equal(
            1000,
            equipment.Notes!.Length);
    }


    [Fact]
    public void Constructor_With1001CharacterNotes_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var notes =
            new string(
                'あ',
                1001);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Equipment(
                    Guid.NewGuid(),
                    "CMP-001",
                    "コンプレッサー",
                    GetValidEquipmentType(),
                    notes:
                        notes));

        // Assert
        Assert.Equal(
            "value",
            exception.ParamName);
    }


    // ============================================
    // Update
    // ============================================

    [Fact]
    public void Update_WithValidValues_UpdatesEquipment()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        var installedOn =
            new DateOnly(
                2026,
                1,
                1);

        var equipmentType =
            GetValidEquipmentType();

        // Act
        equipment.Update(
            "  cmp-002  ",
            "  第2コンプレッサー  ",
            equipmentType,
            "  メーカーB  ",
            "  MODEL-002  ",
            "  SN-002  ",
            installedOn,
            "  更新後  ");

        // Assert
        Assert.Equal(
            "CMP-002",
            equipment.EquipmentCode);

        Assert.Equal(
            "第2コンプレッサー",
            equipment.Name);

        Assert.Equal(
            equipmentType,
            equipment.EquipmentType);

        Assert.Equal(
            "メーカーB",
            equipment.Manufacturer);

        Assert.Equal(
            "MODEL-002",
            equipment.ModelNumber);

        Assert.Equal(
            "SN-002",
            equipment.SerialNumber);

        Assert.Equal(
            installedOn,
            equipment.InstalledOn);

        Assert.Equal(
            "更新後",
            equipment.Notes);
    }


    [Fact]
    public void Update_WithEmptyOptionalValues_ClearsOptionalValues()
    {
        // Arrange
        var equipment =
            new Equipment(
                Guid.NewGuid(),
                "CMP-001",
                "コンプレッサー",
                GetValidEquipmentType(),
                manufacturer:
                    "メーカーA",
                modelNumber:
                    "MODEL-001",
                serialNumber:
                    "SN-001",
                installedOn:
                    new DateOnly(
                        2025,
                        1,
                        1),
                notes:
                    "備考あり");

        // Act
        equipment.Update(
            "CMP-001",
            "コンプレッサー",
            GetValidEquipmentType(),
            null,
            null,
            null,
            null,
            null);

        // Assert
        Assert.Null(
            equipment.Manufacturer);

        Assert.Null(
            equipment.ModelNumber);

        Assert.Null(
            equipment.SerialNumber);

        Assert.Null(
            equipment.InstalledOn);

        Assert.Null(
            equipment.Notes);
    }


    // ============================================
    // ChangeLocation
    // ============================================

    [Fact]
    public void ChangeLocation_WithValidLocationId_ChangesLocationId()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        var newLocationId =
            Guid.NewGuid();

        // Act
        equipment.ChangeLocation(
            newLocationId);

        // Assert
        Assert.Equal(
            newLocationId,
            equipment.LocationId);
    }


    [Fact]
    public void ChangeLocation_WithEmptyLocationId_ThrowsArgumentException()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => equipment.ChangeLocation(
                    Guid.Empty));

        // Assert
        Assert.Equal(
            "locationId",
            exception.ParamName);
    }


    // ============================================
    // Status
    // ============================================

    [Fact]
    public void Constructor_SetsStatusToInService()
    {
        // Act
        var equipment =
            CreateEquipment();

        // Assert
        Assert.Equal(
            EquipmentStatus.InService,
            equipment.Status);
    }


    [Fact]
    public void StartMaintenance_ChangesStatusToUnderMaintenance()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        // Act
        equipment.StartMaintenance();

        // Assert
        Assert.Equal(
            EquipmentStatus.UnderMaintenance,
            equipment.Status);
    }


    [Fact]
    public void ResumeOperation_ChangesStatusToInService()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        equipment.StartMaintenance();

        // Act
        equipment.ResumeOperation();

        // Assert
        Assert.Equal(
            EquipmentStatus.InService,
            equipment.Status);
    }


    [Fact]
    public void Suspend_ChangesStatusToSuspended()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        // Act
        equipment.Suspend();

        // Assert
        Assert.Equal(
            EquipmentStatus.Suspended,
            equipment.Status);
    }


    [Fact]
    public void Retire_ChangesStatusToRetired()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        // Act
        equipment.Retire();

        // Assert
        Assert.Equal(
            EquipmentStatus.Retired,
            equipment.Status);
    }


    [Fact]
    public void ResumeOperation_WhenRetired_ThrowsInvalidOperationException()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        equipment.Retire();

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => equipment.ResumeOperation());

        // Assert
        Assert.Equal(
            "廃止済み設備を直接稼働状態へ戻すことはできません。",
            exception.Message);

        Assert.Equal(
            EquipmentStatus.Retired,
            equipment.Status);
    }


    [Fact]
    public void StartMaintenance_WhenRetired_ThrowsInvalidOperationException()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        equipment.Retire();

        // Act
        Assert.Throws<InvalidOperationException>(
            () => equipment.StartMaintenance());

        // Assert
        Assert.Equal(
            EquipmentStatus.Retired,
            equipment.Status);
    }


    [Fact]
    public void Suspend_WhenRetired_ThrowsInvalidOperationException()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        equipment.Retire();

        // Act
        Assert.Throws<InvalidOperationException>(
            () => equipment.Suspend());

        // Assert
        Assert.Equal(
            EquipmentStatus.Retired,
            equipment.Status);
    }


    [Fact]
    public void Retire_WhenAlreadyRetired_DoesNotThrow()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        equipment.Retire();

        // Act
        var exception =
            Record.Exception(
                () => equipment.Retire());

        // Assert
        Assert.Null(
            exception);

        Assert.Equal(
            EquipmentStatus.Retired,
            equipment.Status);
    }


    [Fact]
    public void StartMaintenance_WhenAlreadyUnderMaintenance_DoesNotThrow()
    {
        // Arrange
        var equipment =
            CreateEquipment();

        equipment.StartMaintenance();

        // Act
        var exception =
            Record.Exception(
                () => equipment.StartMaintenance());

        // Assert
        Assert.Null(
            exception);

        Assert.Equal(
            EquipmentStatus.UnderMaintenance,
            equipment.Status);
    }


    // ============================================
    // Test Helpers
    // ============================================

    private static Equipment CreateEquipment()
    {
        return new Equipment(
            Guid.NewGuid(),
            "CMP-001",
            "第1コンプレッサー",
            GetValidEquipmentType());
    }


    private static EquipmentType GetValidEquipmentType()
    {
        return Enum
            .GetValues<EquipmentType>()
            .First();
    }
}