using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;

namespace FacilityInspection.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly EquipmentRepository _repository;
    private bool _initialized;

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

    public ObservableCollection<Equipment> Equipments { get; } = [];

    public string DatabasePath { get; }

    [ObservableProperty]
    private string newEquipmentName = string.Empty;

    [ObservableProperty]
    private string statusMessage =
        "「再読込」を押してSQLiteを初期化してください。";

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var equipmentName = NewEquipmentName.Trim();

            if (string.IsNullOrWhiteSpace(equipmentName))
            {
                StatusMessage = "設備名を入力してください。";
                return;
            }

            await EnsureInitializedAsync();

            await _repository.AddAsync(equipmentName);

            NewEquipmentName = string.Empty;

            await ReloadCoreAsync();

            StatusMessage =
                $"保存成功：現在{Equipments.Count}件です。";
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