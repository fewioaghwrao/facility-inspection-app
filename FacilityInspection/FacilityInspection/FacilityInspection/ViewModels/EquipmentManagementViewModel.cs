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

public partial class EquipmentManagementViewModel : ViewModelBase
{
    private readonly EquipmentRepository _repository;

    private bool _initialized;
    private bool _suppressFactorySiteChanged;

    public EquipmentManagementViewModel()
    {
        var localApplicationData =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        DatabasePath = Path.Combine(
            localApplicationData,
            "FacilityInspection",
            "facility-inspection.db");

        _repository =
            new EquipmentRepository(DatabasePath);


    }

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

    public ObservableCollection<DomainEquipment> Equipments { get; } = [];

    public ObservableCollection<FactorySite> FactorySites { get; } = [];

    public ObservableCollection<Location> Locations { get; } = [];

    public IReadOnlyList<EquipmentType> EquipmentTypes { get; } =
        Enum.GetValues<EquipmentType>();

    public string DatabasePath { get; }

    [ObservableProperty]
    private FactorySite? selectedFactorySite;

    [ObservableProperty]
    private Location? selectedLocation;

    [ObservableProperty]
    private string newEquipmentCode = string.Empty;

    [ObservableProperty]
    private string newEquipmentName = string.Empty;

    [ObservableProperty]
    private EquipmentType selectedEquipmentType =
        EquipmentType.AirCompressor;

    [ObservableProperty]
    private string statusMessage =
        "「再読込」を押してSQLiteを初期化してください。";

    partial void OnSelectedFactorySiteChanged(
        FactorySite? value)
    {
        if (_suppressFactorySiteChanged)
        {
            return;
        }

        LoadLocationsCommand.Execute(value);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var equipmentCode =
                NewEquipmentCode.Trim();

            var equipmentName =
                NewEquipmentName.Trim();

            if (string.IsNullOrWhiteSpace(equipmentCode))
            {
                StatusMessage =
                    "設備コードを入力してください。";

                return;
            }

            if (string.IsNullOrWhiteSpace(equipmentName))
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

            await _repository.AddAsync(
                locationId: SelectedLocation.Id,
                equipmentCode: equipmentCode,
                equipmentName: equipmentName,
                equipmentType: SelectedEquipmentType);

            NewEquipmentCode = string.Empty;
            NewEquipmentName = string.Empty;

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
                $"保存失敗：{ex.GetType().Name} - {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        try
        {
            await EnsureInitializedAsync();
            await ReloadEquipmentsCoreAsync();

            StatusMessage =
                $"再読込成功：{Equipments.Count}件取得しました。";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"再読込失敗：{ex.GetType().Name} - {ex.Message}";
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task LoadLocationsAsync(
        FactorySite? factorySite)
    {
        try
        {
            await LoadLocationsCoreAsync(factorySite);

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
            SelectedLocation = null;

            StatusMessage =
                $"設置場所の読込失敗：" +
                $"{ex.GetType().Name} - {ex.Message}";
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _repository.InitializeAsync();

        _initialized = true;

        try
        {
            await LoadFactorySitesCoreAsync();
        }
        catch
        {
            _initialized = false;
            throw;
        }
    }

    private async Task LoadFactorySitesCoreAsync()
    {
        var factorySites =
            await _repository.GetFactorySitesAsync();

        FactorySites.Clear();

        foreach (var factorySite in factorySites)
        {
            FactorySites.Add(factorySite);
        }

        if (FactorySites.Count == 0)
        {
            SelectedFactorySite = null;

            Locations.Clear();
            SelectedLocation = null;

            throw new InvalidOperationException(
                "有効な工場が登録されていません。");
        }

        _suppressFactorySiteChanged = true;

        try
        {
            SelectedFactorySite =
                FactorySites.FirstOrDefault();
        }
        finally
        {
            _suppressFactorySiteChanged = false;
        }

        await LoadLocationsCoreAsync(
            SelectedFactorySite);
    }

    private async Task LoadLocationsCoreAsync(
        FactorySite? factorySite)
    {
        Locations.Clear();
        SelectedLocation = null;

        if (factorySite is null)
        {
            return;
        }

        var locations =
            await _repository
                .GetLocationsByFactorySiteIdAsync(
                    factorySite.Id);

        foreach (var location in locations)
        {
            Locations.Add(location);
        }

        if (Locations.Count == 0)
        {
            throw new InvalidOperationException(
                $"「{factorySite.Name}」には" +
                "有効な設置場所が登録されていません。");
        }

        SelectedLocation =
            Locations.FirstOrDefault();
    }

    private async Task ReloadEquipmentsCoreAsync()
    {
        var equipments =
            await _repository.GetAllAsync();

        Equipments.Clear();

        foreach (var equipment in equipments)
        {
            Equipments.Add(equipment);
        }
    }
}