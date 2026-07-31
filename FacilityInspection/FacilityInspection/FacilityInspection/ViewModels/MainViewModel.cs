using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Equipments;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using DomainEquipment =
    FacilityInspection.Domain.Equipments.Equipment;

namespace FacilityInspection.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly EquipmentRepository _repository;

    private bool _initialized;
    private Guid _defaultLocationId;

    public MainViewModel()
    {
        var localApplicationData =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        DatabasePath = Path.Combine(
            localApplicationData,
            "FacilityInspection",
            "facility-inspection.db");

        _repository = new EquipmentRepository(DatabasePath);
    }

    public ObservableCollection<DomainEquipment> Equipments { get; } = [];

    public IReadOnlyList<EquipmentType> EquipmentTypes { get; } =
        Enum.GetValues<EquipmentType>();

    public string DatabasePath { get; }

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

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var equipmentCode = NewEquipmentCode.Trim();
            var equipmentName = NewEquipmentName.Trim();

            if (string.IsNullOrWhiteSpace(equipmentCode))
            {
                StatusMessage = "設備コードを入力してください。";
                return;
            }

            if (string.IsNullOrWhiteSpace(equipmentName))
            {
                StatusMessage = "設備名を入力してください。";
                return;
            }

            await EnsureInitializedAsync();

            await _repository.AddAsync(
                locationId: _defaultLocationId,
                equipmentCode: equipmentCode,
                equipmentName: equipmentName,
                equipmentType: SelectedEquipmentType);

            NewEquipmentCode = string.Empty;
            NewEquipmentName = string.Empty;

            await ReloadCoreAsync();

            StatusMessage =
                $"保存成功：現在{Equipments.Count}件です。";
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"保存できません：{ex.Message}";
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
            await ReloadCoreAsync();

            StatusMessage =
                $"再読込成功：{Equipments.Count}件取得しました。";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"再読込失敗：{ex.GetType().Name} - {ex.Message}";
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _repository.InitializeAsync();

        _defaultLocationId =
            await _repository.GetDefaultLocationIdAsync();

        _initialized = true;
    }

    private async Task ReloadCoreAsync()
    {
        var equipments = await _repository.GetAllAsync();

        Equipments.Clear();

        foreach (var equipment in equipments)
        {
            Equipments.Add(equipment);
        }
    }
}