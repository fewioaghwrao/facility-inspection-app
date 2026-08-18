using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Sites;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DomainEquipment =
    FacilityInspection.Domain.Equipments.Equipment;

namespace FacilityInspection.ViewModels;

public partial class EquipmentManagementViewModel
    : ViewModelBase
{
    private readonly Func<Task>
        _initializeAsync;

    private readonly Func<
        Task<IReadOnlyList<FactorySite>>>
        _getFactorySitesAsync;

    private readonly Func<
        Guid,
        Task<IReadOnlyList<Location>>>
        _getLocationsByFactorySiteIdAsync;

    private readonly Func<
        Guid,
        string,
        string,
        EquipmentType,
        Task>
        _addEquipmentAsync;

    private readonly Func<
        Task<IReadOnlyList<DomainEquipment>>>
        _getAllEquipmentsAsync;


    private bool _initialized;

    private bool _suppressFactorySiteChanged;


    // ============================================
    // Constructor
    // 本番用
    // ============================================

    public EquipmentManagementViewModel()
    {
        var localApplicationData =
            Environment.GetFolderPath(
                Environment.SpecialFolder
                    .LocalApplicationData);

        DatabasePath =
            Path.Combine(
                localApplicationData,
                "FacilityInspection",
                "facility-inspection.db");


        var repository =
            new EquipmentRepository(
                DatabasePath);


        _initializeAsync =
            () =>
                repository
                    .InitializeAsync();


        _getFactorySitesAsync =
            () =>
                repository
                    .GetFactorySitesAsync();


        _getLocationsByFactorySiteIdAsync =
            factorySiteId =>
                repository
                    .GetLocationsByFactorySiteIdAsync(
                        factorySiteId);


        _addEquipmentAsync =
            (
                locationId,
                equipmentCode,
                equipmentName,
                equipmentType) =>
                repository
                    .AddAsync(
                        locationId:
                            locationId,
                        equipmentCode:
                            equipmentCode,
                        equipmentName:
                            equipmentName,
                        equipmentType:
                            equipmentType);


        _getAllEquipmentsAsync =
            () =>
                repository
                    .GetAllAsync();
    }


    // ============================================
    // Constructor
    // 既存互換用
    // ============================================

    public EquipmentManagementViewModel(
        string operatorName,
        Action openDashboard,
        Action openEquipmentManagement,
        Action openScheduleCalendar,
        Action logout)
        : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            operatorName);

        ArgumentNullException.ThrowIfNull(
            openDashboard);

        ArgumentNullException.ThrowIfNull(
            openEquipmentManagement);

        ArgumentNullException.ThrowIfNull(
            openScheduleCalendar);

        ArgumentNullException.ThrowIfNull(
            logout);
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal EquipmentManagementViewModel(
        Func<Task>
            initializeAsync,
        Func<Task<IReadOnlyList<FactorySite>>>
            getFactorySitesAsync,
        Func<Guid, Task<IReadOnlyList<Location>>>
            getLocationsByFactorySiteIdAsync,
        Func<
            Guid,
            string,
            string,
            EquipmentType,
            Task>
            addEquipmentAsync,
        Func<Task<IReadOnlyList<DomainEquipment>>>
            getAllEquipmentsAsync)
    {
        ArgumentNullException.ThrowIfNull(
            initializeAsync);

        ArgumentNullException.ThrowIfNull(
            getFactorySitesAsync);

        ArgumentNullException.ThrowIfNull(
            getLocationsByFactorySiteIdAsync);

        ArgumentNullException.ThrowIfNull(
            addEquipmentAsync);

        ArgumentNullException.ThrowIfNull(
            getAllEquipmentsAsync);


        /*
         * テストでは実DBを使用しないため、
         * 実際のDBパスは不要。
         */
        DatabasePath =
            string.Empty;


        _initializeAsync =
            initializeAsync;

        _getFactorySitesAsync =
            getFactorySitesAsync;

        _getLocationsByFactorySiteIdAsync =
            getLocationsByFactorySiteIdAsync;

        _addEquipmentAsync =
            addEquipmentAsync;

        _getAllEquipmentsAsync =
            getAllEquipmentsAsync;
    }


    // ============================================
    // Collections
    // ============================================

    public ObservableCollection<DomainEquipment>
        Equipments
    {
        get;
    } = [];


    public ObservableCollection<FactorySite>
        FactorySites
    {
        get;
    } = [];


    public ObservableCollection<Location>
        Locations
    {
        get;
    } = [];


    public IReadOnlyList<EquipmentType>
        EquipmentTypes
    {
        get;
    } =
        Enum.GetValues<EquipmentType>();


    public string DatabasePath
    {
        get;
    }


    // ============================================
    // Selection
    // ============================================

    [ObservableProperty]
    private FactorySite?
        selectedFactorySite;


    [ObservableProperty]
    private Location?
        selectedLocation;


    partial void OnSelectedFactorySiteChanged(
        FactorySite? value)
    {
        if (_suppressFactorySiteChanged)
        {
            return;
        }

        LoadLocationsCommand.Execute(
            value);
    }


    // ============================================
    // Editor
    // ============================================

    [ObservableProperty]
    private string newEquipmentCode =
        string.Empty;


    [ObservableProperty]
    private string newEquipmentName =
        string.Empty;


    [ObservableProperty]
    private EquipmentType
        selectedEquipmentType =
            EquipmentType.AirCompressor;


    // ============================================
    // Status
    // ============================================

    [ObservableProperty]
    private string statusMessage =
        "「再読込」を押してSQLiteを初期化してください。";


    // ============================================
    // Save
    // ============================================

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var equipmentCode =
                NewEquipmentCode.Trim();

            var equipmentName =
                NewEquipmentName.Trim();


            if (string.IsNullOrWhiteSpace(
                    equipmentCode))
            {
                StatusMessage =
                    "設備コードを入力してください。";

                return;
            }


            if (string.IsNullOrWhiteSpace(
                    equipmentName))
            {
                StatusMessage =
                    "設備名を入力してください。";

                return;
            }


            await EnsureInitializedAsync();


            if (SelectedFactorySite is null)
            {
                StatusMessage =
                    "工場を選択してください。";

                return;
            }


            if (SelectedLocation is null)
            {
                StatusMessage =
                    "設置場所を選択してください。";

                return;
            }


            await _addEquipmentAsync(
                SelectedLocation.Id,
                equipmentCode,
                equipmentName,
                SelectedEquipmentType);


            NewEquipmentCode =
                string.Empty;

            NewEquipmentName =
                string.Empty;


            await ReloadEquipmentsCoreAsync();


            StatusMessage =
                $"保存成功：現在{Equipments.Count}件です。";
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage =
                $"保存できません：{ex.Message}";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"保存失敗：" +
                $"{ex.GetType().Name} - " +
                $"{ex.Message}";
        }
    }


    // ============================================
    // Reload
    // ============================================

    [RelayCommand]
    private async Task ReloadAsync()
    {
        try
        {
            await EnsureInitializedAsync();

            await ReloadEquipmentsCoreAsync();


            StatusMessage =
                $"再読込成功：" +
                $"{Equipments.Count}件取得しました。";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"再読込失敗：" +
                $"{ex.GetType().Name} - " +
                $"{ex.Message}";
        }
    }


    // ============================================
    // Load Locations
    // ============================================

    [RelayCommand(
        AllowConcurrentExecutions = false)]
    private async Task LoadLocationsAsync(
        FactorySite? factorySite)
    {
        try
        {
            await LoadLocationsCoreAsync(
                factorySite);


            if (factorySite is null)
            {
                StatusMessage =
                    "工場を選択してください。";

                return;
            }


            StatusMessage =
                $"{factorySite.Name}の設置場所を" +
                $"{Locations.Count}件取得しました。";
        }
        catch (Exception ex)
        {
            Locations.Clear();

            SelectedLocation =
                null;


            StatusMessage =
                "設置場所の読込失敗：" +
                $"{ex.GetType().Name} - " +
                $"{ex.Message}";
        }
    }


    // ============================================
    // Initialize
    // ============================================

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
        {
            return;
        }


        await _initializeAsync();


        _initialized =
            true;


        try
        {
            await LoadFactorySitesCoreAsync();
        }
        catch
        {
            /*
             * 工場・設置場所のロードまで
             * 正常終了して初期化完了とする。
             */
            _initialized =
                false;

            throw;
        }
    }


    // ============================================
    // Factory Sites
    // ============================================

    private async Task LoadFactorySitesCoreAsync()
    {
        var factorySites =
            await _getFactorySitesAsync();


        FactorySites.Clear();


        foreach (var factorySite
                 in factorySites)
        {
            FactorySites.Add(
                factorySite);
        }


        if (FactorySites.Count == 0)
        {
            SelectedFactorySite =
                null;

            Locations.Clear();

            SelectedLocation =
                null;


            throw new InvalidOperationException(
                "有効な工場が登録されていません。");
        }


        /*
         * 初期工場を設定するときは、
         * OnSelectedFactorySiteChangedから
         * 二重ロードされないよう抑止する。
         */
        _suppressFactorySiteChanged =
            true;

        try
        {
            SelectedFactorySite =
                FactorySites
                    .FirstOrDefault();
        }
        finally
        {
            _suppressFactorySiteChanged =
                false;
        }


        await LoadLocationsCoreAsync(
            SelectedFactorySite);
    }


    // ============================================
    // Locations
    // ============================================

    private async Task LoadLocationsCoreAsync(
        FactorySite? factorySite)
    {
        Locations.Clear();

        SelectedLocation =
            null;


        if (factorySite is null)
        {
            return;
        }


        var locations =
            await _getLocationsByFactorySiteIdAsync(
                factorySite.Id);


        foreach (var location
                 in locations)
        {
            Locations.Add(
                location);
        }


        if (Locations.Count == 0)
        {
            throw new InvalidOperationException(
                $"「{factorySite.Name}」には" +
                "有効な設置場所が登録されていません。");
        }


        SelectedLocation =
            Locations
                .FirstOrDefault();
    }


    // ============================================
    // Equipments
    // ============================================

    private async Task ReloadEquipmentsCoreAsync()
    {
        var equipments =
            await _getAllEquipmentsAsync();


        Equipments.Clear();


        foreach (var equipment
                 in equipments)
        {
            Equipments.Add(
                equipment);
        }
    }
}