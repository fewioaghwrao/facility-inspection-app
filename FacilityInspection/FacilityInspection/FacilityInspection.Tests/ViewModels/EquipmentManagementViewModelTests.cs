using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Sites;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using DomainEquipment =
    FacilityInspection.Domain.Equipments.Equipment;

namespace FacilityInspection.Tests.ViewModels;

public sealed class EquipmentManagementViewModelTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithNullInitialize_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new EquipmentManagementViewModel(
                        null!,
                        () =>
                            Task.FromResult<
                                IReadOnlyList<
                                    FactorySite>>(
                                []),
                        _ =>
                            Task.FromResult<
                                IReadOnlyList<
                                    Location>>(
                                []),
                        (
                            _,
                            _,
                            _,
                            _) =>
                            Task.CompletedTask,
                        () =>
                            Task.FromResult<
                                IReadOnlyList<
                                    DomainEquipment>>(
                                [])));

        // Assert
        Assert.Equal(
            "initializeAsync",
            exception.ParamName);
    }


    // ============================================
    // Initial State
    // ============================================

    [Fact]
    public void Constructor_SetsInitialState()
    {
        // Arrange & Act
        var sut =
            CreateViewModel();

        // Assert
        Assert.Empty(
            sut.Equipments);

        Assert.Empty(
            sut.FactorySites);

        Assert.Empty(
            sut.Locations);

        Assert.Null(
            sut.SelectedFactorySite);

        Assert.Null(
            sut.SelectedLocation);

        Assert.Equal(
            string.Empty,
            sut.NewEquipmentCode);

        Assert.Equal(
            string.Empty,
            sut.NewEquipmentName);

        Assert.Equal(
            EquipmentType.AirCompressor,
            sut.SelectedEquipmentType);

        Assert.Equal(
            "「再読込」を押してSQLiteを初期化してください。",
            sut.StatusMessage);

        Assert.Equal(
            Enum.GetValues<EquipmentType>(),
            sut.EquipmentTypes);
    }


    // ============================================
    // Save Validation
    // ============================================

    [Fact]
    public async Task SaveCommand_WhenEquipmentCodeIsEmpty_ShowsValidationMessage()
    {
        // Arrange
        var initializeCallCount =
            0;

        var addCallCount =
            0;

        var sut =
            CreateViewModel(
                initializeAsync:
                    () =>
                    {
                        initializeCallCount++;

                        return Task.CompletedTask;
                    },
                addEquipmentAsync:
                    (
                        _,
                        _,
                        _,
                        _) =>
                    {
                        addCallCount++;

                        return Task.CompletedTask;
                    });


        sut.NewEquipmentCode =
            "   ";

        sut.NewEquipmentName =
            "コンプレッサー";


        // Act
        await sut.SaveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "設備コードを入力してください。",
            sut.StatusMessage);

        Assert.Equal(
            0,
            initializeCallCount);

        Assert.Equal(
            0,
            addCallCount);
    }


    [Fact]
    public async Task SaveCommand_WhenEquipmentNameIsEmpty_ShowsValidationMessage()
    {
        // Arrange
        var initializeCallCount =
            0;

        var addCallCount =
            0;

        var sut =
            CreateViewModel(
                initializeAsync:
                    () =>
                    {
                        initializeCallCount++;

                        return Task.CompletedTask;
                    },
                addEquipmentAsync:
                    (
                        _,
                        _,
                        _,
                        _) =>
                    {
                        addCallCount++;

                        return Task.CompletedTask;
                    });


        sut.NewEquipmentCode =
            "EQ-001";

        sut.NewEquipmentName =
            "   ";


        // Act
        await sut.SaveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "設備名を入力してください。",
            sut.StatusMessage);

        Assert.Equal(
            0,
            initializeCallCount);

        Assert.Equal(
            0,
            addCallCount);
    }


    // ============================================
    // Reload
    // ============================================

    [Fact]
    public async Task ReloadCommand_LoadsFactoriesLocationsAndEquipments()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        var location =
            CreateLocation(
                factorySite);

        var equipment =
            CreateEquipment(
                location);


        var sut =
            CreateViewModel(
                factorySites:
                    [
                        factorySite
                    ],
                locations:
                    [
                        location
                    ],
                equipments:
                    [
                        equipment
                    ]);


        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Single(
            sut.FactorySites);

        Assert.Same(
            factorySite,
            sut.SelectedFactorySite);

        Assert.Single(
            sut.Locations);

        Assert.Same(
            location,
            sut.SelectedLocation);

        Assert.Single(
            sut.Equipments);

        Assert.Same(
            equipment,
            sut.Equipments[0]);

        Assert.Equal(
            "再読込成功：1件取得しました。",
            sut.StatusMessage);
    }


    // ============================================
    // Initialize Once
    // ============================================

    [Fact]
    public async Task ReloadCommand_CalledTwice_InitializesOnlyOnce()
    {
        // Arrange
        var initializeCallCount =
            0;

        var factorySite =
            CreateFactorySite();

        var location =
            CreateLocation(
                factorySite);


        var sut =
            CreateViewModel(
                initializeAsync:
                    () =>
                    {
                        initializeCallCount++;

                        return Task.CompletedTask;
                    },
                factorySites:
                    [
                        factorySite
                    ],
                locations:
                    [
                        location
                    ]);


        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);

        await sut.ReloadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            1,
            initializeCallCount);
    }


    // ============================================
    // No Factory Site
    // ============================================

    [Fact]
    public async Task ReloadCommand_WhenNoFactorySites_ShowsError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                factorySites:
                    []);


        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Empty(
            sut.FactorySites);

        Assert.Null(
            sut.SelectedFactorySite);

        Assert.Empty(
            sut.Locations);

        Assert.Null(
            sut.SelectedLocation);

        Assert.Contains(
            "再読込失敗：InvalidOperationException",
            sut.StatusMessage);

        Assert.Contains(
            "有効な工場が登録されていません。",
            sut.StatusMessage);
    }


    [Fact]
    public async Task ReloadCommand_WhenInitializationFails_RetriesInitializationNextTime()
    {
        // Arrange
        var initializeCallCount =
            0;

        var factorySiteCallCount =
            0;

        var factorySite =
            CreateFactorySite();

        var location =
            CreateLocation(
                factorySite);


        var sut =
            CreateViewModel(
                initializeAsync:
                    () =>
                    {
                        initializeCallCount++;

                        return Task.CompletedTask;
                    },

                getFactorySitesAsync:
                    () =>
                    {
                        factorySiteCallCount++;

                        IReadOnlyList<
                            FactorySite> result =
                            factorySiteCallCount == 1
                                ? []
                                : [factorySite];

                        return Task.FromResult(
                            result);
                    },

                getLocationsAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<
                                Location>>(
                            [
                                location
                            ]));


        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);

        await sut.ReloadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            2,
            initializeCallCount);

        Assert.Equal(
            2,
            factorySiteCallCount);

        Assert.Same(
            factorySite,
            sut.SelectedFactorySite);

        Assert.Same(
            location,
            sut.SelectedLocation);
    }


    // ============================================
    // No Locations
    // ============================================

    [Fact]
    public async Task ReloadCommand_WhenFactoryHasNoLocations_ShowsError()
    {
        // Arrange
        var factorySite =
            CreateFactorySite(
                name:
                    "第1工場");


        var sut =
            CreateViewModel(
                factorySites:
                    [
                        factorySite
                    ],
                locations:
                    []);


        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Single(
            sut.FactorySites);

        Assert.Same(
            factorySite,
            sut.SelectedFactorySite);

        Assert.Empty(
            sut.Locations);

        Assert.Null(
            sut.SelectedLocation);

        Assert.Contains(
            "再読込失敗：InvalidOperationException",
            sut.StatusMessage);

        Assert.Contains(
            "「第1工場」には有効な設置場所が登録されていません。",
            sut.StatusMessage);
    }


    // ============================================
    // Load Locations
    // ============================================

    [Fact]
    public async Task LoadLocationsCommand_LoadsLocationsAndSelectsFirst()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        var first =
            CreateLocation(
                factorySite,
                code:
                    "L01",
                name:
                    "コンプレッサー室");

        var second =
            CreateLocation(
                factorySite,
                code:
                    "L02",
                name:
                    "ポンプ室");


        var sut =
            CreateViewModel(
                getLocationsAsync:
                    id =>
                    {
                        Assert.Equal(
                            factorySite.Id,
                            id);

                        return Task.FromResult<
                            IReadOnlyList<Location>>(
                            [
                                first,
                                second
                            ]);
                    });


        // Act
        await sut.LoadLocationsCommand
            .ExecuteAsync(
                factorySite);


        // Assert
        Assert.Equal(
            2,
            sut.Locations.Count);

        Assert.Same(
            first,
            sut.SelectedLocation);

        Assert.Equal(
            $"{factorySite.Name}の設置場所を2件取得しました。",
            sut.StatusMessage);
    }


    [Fact]
    public async Task LoadLocationsCommand_WithNullFactory_ClearsLocations()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        var location =
            CreateLocation(
                factorySite);


        var sut =
            CreateViewModel(
                getLocationsAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<Location>>(
                            [
                                location
                            ]));


        await sut.LoadLocationsCommand
            .ExecuteAsync(
                factorySite);


        Assert.Single(
            sut.Locations);


        // Act
        await sut.LoadLocationsCommand
            .ExecuteAsync(
                null);


        // Assert
        Assert.Empty(
            sut.Locations);

        Assert.Null(
            sut.SelectedLocation);

        Assert.Equal(
            "工場を選択してください。",
            sut.StatusMessage);
    }


    [Fact]
    public async Task LoadLocationsCommand_WhenLoaderThrows_ClearsLocationsAndShowsError()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();


        var sut =
            CreateViewModel(
                getLocationsAsync:
                    _ =>
                        Task.FromException<
                            IReadOnlyList<Location>>(
                            new IOException(
                                "ロケーション読込エラー")));


        // Act
        await sut.LoadLocationsCommand
            .ExecuteAsync(
                factorySite);


        // Assert
        Assert.Empty(
            sut.Locations);

        Assert.Null(
            sut.SelectedLocation);

        Assert.Contains(
            "設置場所の読込失敗：IOException",
            sut.StatusMessage);

        Assert.Contains(
            "ロケーション読込エラー",
            sut.StatusMessage);
    }


    // ============================================
    // Save Success
    // ============================================

    [Fact]
    public async Task SaveCommand_WhenValid_SavesTrimmedValuesAndReloadsEquipments()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        var location =
            CreateLocation(
                factorySite);

        var savedEquipment =
            CreateEquipment(
                location,
                equipmentCode:
                    "EQ-100",
                name:
                    "新規コンプレッサー");


        Guid? actualLocationId =
            null;

        string? actualCode =
            null;

        string? actualName =
            null;

        EquipmentType?
            actualEquipmentType =
                null;


        var sut =
            CreateViewModel(
                factorySites:
                    [
                        factorySite
                    ],
                locations:
                    [
                        location
                    ],
                equipments:
                    [
                        savedEquipment
                    ],

                addEquipmentAsync:
                    (
                        locationId,
                        equipmentCode,
                        equipmentName,
                        equipmentType) =>
                    {
                        actualLocationId =
                            locationId;

                        actualCode =
                            equipmentCode;

                        actualName =
                            equipmentName;

                        actualEquipmentType =
                            equipmentType;

                        return Task.CompletedTask;
                    });


        sut.NewEquipmentCode =
            "  EQ-100  ";

        sut.NewEquipmentName =
            "  新規コンプレッサー  ";

        sut.SelectedEquipmentType =
            EquipmentType.AirCompressor;


        // Act
        await sut.SaveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            location.Id,
            actualLocationId);

        Assert.Equal(
            "EQ-100",
            actualCode);

        Assert.Equal(
            "新規コンプレッサー",
            actualName);

        Assert.Equal(
            EquipmentType.AirCompressor,
            actualEquipmentType);


        Assert.Equal(
            string.Empty,
            sut.NewEquipmentCode);

        Assert.Equal(
            string.Empty,
            sut.NewEquipmentName);


        Assert.Single(
            sut.Equipments);

        Assert.Same(
            savedEquipment,
            sut.Equipments[0]);


        Assert.Equal(
            "保存成功：現在1件です。",
            sut.StatusMessage);
    }


    // ============================================
    // Factory Missing
    // ============================================

    [Fact]
    public async Task SaveCommand_WhenFactorySiteIsNotSelected_DoesNotSave()
    {
        // Arrange
        var addCallCount =
            0;

        var factorySite =
            CreateFactorySite();

        var location =
            CreateLocation(
                factorySite);


        var sut =
            CreateViewModel(
                factorySites:
                    [
                        factorySite
                    ],
                locations:
                    [
                        location
                    ],

                addEquipmentAsync:
                    (
                        _,
                        _,
                        _,
                        _) =>
                    {
                        addCallCount++;

                        return Task.CompletedTask;
                    });


        /*
         * 先に初期化だけ完了させる。
         */
        await sut.ReloadCommand
            .ExecuteAsync(null);


        sut.SelectedFactorySite =
            null;

        await sut.LoadLocationsCommand
            .ExecuteAsync(null);


        sut.NewEquipmentCode =
            "EQ-001";

        sut.NewEquipmentName =
            "コンプレッサー";


        // Act
        await sut.SaveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            0,
            addCallCount);

        Assert.Equal(
            "工場を選択してください。",
            sut.StatusMessage);
    }


    // ============================================
    // Location Missing
    // ============================================

    [Fact]
    public async Task SaveCommand_WhenLocationIsNotSelected_DoesNotSave()
    {
        // Arrange
        var addCallCount =
            0;

        var factorySite =
            CreateFactorySite();

        var location =
            CreateLocation(
                factorySite);


        var sut =
            CreateViewModel(
                factorySites:
                    [
                        factorySite
                    ],
                locations:
                    [
                        location
                    ],

                addEquipmentAsync:
                    (
                        _,
                        _,
                        _,
                        _) =>
                    {
                        addCallCount++;

                        return Task.CompletedTask;
                    });


        await sut.ReloadCommand
            .ExecuteAsync(null);


        sut.SelectedLocation =
            null;

        sut.NewEquipmentCode =
            "EQ-001";

        sut.NewEquipmentName =
            "コンプレッサー";


        // Act
        await sut.SaveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            0,
            addCallCount);

        Assert.Equal(
            "設置場所を選択してください。",
            sut.StatusMessage);
    }


    // ============================================
    // Save InvalidOperationException
    // ============================================

    [Fact]
    public async Task SaveCommand_WhenRepositoryThrowsInvalidOperationException_ShowsBusinessError()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        var location =
            CreateLocation(
                factorySite);


        var sut =
            CreateViewModel(
                factorySites:
                    [
                        factorySite
                    ],
                locations:
                    [
                        location
                    ],

                addEquipmentAsync:
                    (
                        _,
                        _,
                        _,
                        _) =>
                        Task.FromException(
                            new InvalidOperationException(
                                "設備コードが重複しています。")));


        sut.NewEquipmentCode =
            "EQ-001";

        sut.NewEquipmentName =
            "コンプレッサー";


        // Act
        await sut.SaveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "保存できません：設備コードが重複しています。",
            sut.StatusMessage);
    }


    // ============================================
    // Save Unexpected Exception
    // ============================================

    [Fact]
    public async Task SaveCommand_WhenUnexpectedExceptionOccurs_ShowsFailureMessage()
    {
        // Arrange
        var factorySite =
            CreateFactorySite();

        var location =
            CreateLocation(
                factorySite);


        var sut =
            CreateViewModel(
                factorySites:
                    [
                        factorySite
                    ],
                locations:
                    [
                        location
                    ],

                addEquipmentAsync:
                    (
                        _,
                        _,
                        _,
                        _) =>
                        Task.FromException(
                            new IOException(
                                "ファイルアクセスエラー")));


        sut.NewEquipmentCode =
            "EQ-001";

        sut.NewEquipmentName =
            "コンプレッサー";


        // Act
        await sut.SaveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "保存失敗：IOException - ファイルアクセスエラー",
            sut.StatusMessage);
    }


    // ============================================
    // Helpers
    // ============================================

    private static EquipmentManagementViewModel
        CreateViewModel(
            Func<Task>? initializeAsync = null,
            IReadOnlyList<FactorySite>?
                factorySites = null,
            IReadOnlyList<Location>?
                locations = null,
            IReadOnlyList<DomainEquipment>?
                equipments = null,
            Func<Task<IReadOnlyList<FactorySite>>>?
                getFactorySitesAsync = null,
            Func<Guid, Task<IReadOnlyList<Location>>>?
                getLocationsAsync = null,
            Func<
                Guid,
                string,
                string,
                EquipmentType,
                Task>?
                addEquipmentAsync = null,
            Func<Task<IReadOnlyList<DomainEquipment>>>?
                getAllEquipmentsAsync = null)
    {
        factorySites ??=
            [];

        locations ??=
            [];

        equipments ??=
            [];


        return new EquipmentManagementViewModel(
            initializeAsync ??
                (() =>
                    Task.CompletedTask),

            getFactorySitesAsync ??
                (() =>
                    Task.FromResult(
                        factorySites)),

            getLocationsAsync ??
                (_ =>
                    Task.FromResult(
                        locations)),

            addEquipmentAsync ??
                ((
                    _,
                    _,
                    _,
                    _) =>
                    Task.CompletedTask),

            getAllEquipmentsAsync ??
                (() =>
                    Task.FromResult(
                        equipments)));
    }


    private static FactorySite
        CreateFactorySite(
            string code = "F001",
            string name = "第1工場")
    {
        return new FactorySite(
            code,
            name);
    }


    private static Location
        CreateLocation(
            FactorySite factorySite,
            string code = "L001",
            string name = "コンプレッサー室")
    {
        return new Location(
            factorySite.Id,
            code,
            name,
            floor:
                "1F");
    }


    private static DomainEquipment
        CreateEquipment(
            Location location,
            string equipmentCode =
                "EQ-001",
            string name =
                "コンプレッサー")
    {
        return new DomainEquipment(
            location.Id,
            equipmentCode,
            name,
            EquipmentType.AirCompressor);
    }
}