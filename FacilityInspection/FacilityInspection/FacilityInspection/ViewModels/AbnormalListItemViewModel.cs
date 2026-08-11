using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using System;

namespace FacilityInspection.ViewModels;

public sealed partial class AbnormalListItemViewModel
{
    private readonly Action<Guid>
        _openDetailRequested;

    public AbnormalListItemViewModel(
        AbnormalResultListData source,
        Action<Guid> openDetailRequested)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            openDetailRequested);

        _openDetailRequested =
            openDetailRequested;

        ScheduleId = source.ScheduleId;
        InspectionId = source.InspectionId;
        ResultId = source.ResultId;
        ScheduledDate = source.ScheduledDate;
        FactorySiteName = source.FactorySiteName;
        LocationName = source.LocationName;
        EquipmentCode = source.EquipmentCode;
        EquipmentName = source.EquipmentName;
        TemplateName = source.TemplateName;
        OperatorName = source.OperatorName;
        InspectionStatus = source.InspectionStatus;

        ItemName = source.ItemName;
        InputType = source.InputType;
        CheckValue = source.CheckValue;
        NumericValue = source.NumericValue;
        TextValue = source.TextValue;
        Unit = source.Unit;
        Comment = source.Comment ?? string.Empty;
        PhotoCount = source.PhotoCount;
    }

    public Guid ScheduleId { get; }

    public Guid InspectionId { get; }

    public Guid ResultId { get; }

    public DateOnly ScheduledDate { get; }

    public string FactorySiteName { get; }

    public string LocationName { get; }

    public string EquipmentCode { get; }

    public string EquipmentName { get; }

    public string TemplateName { get; }

    public string OperatorName { get; }

    public InspectionStatus InspectionStatus { get; }

    public string ItemName { get; }

    public InspectionInputType InputType { get; }

    public bool? CheckValue { get; }

    public decimal? NumericValue { get; }

    public string? TextValue { get; }

    public string? Unit { get; }

    public string Comment { get; }

    public int PhotoCount { get; }

    public string ScheduledDateText =>
        ScheduledDate.ToString(
            "yyyy/MM/dd");

    public string LocationDisplayName =>
        $"{FactorySiteName} / {LocationName}";

    public string EquipmentDisplayName =>
        $"{EquipmentCode}  {EquipmentName}";

    public string PhotoCountText =>
        $"{PhotoCount}枚";

    public bool HasComment =>
        !string.IsNullOrWhiteSpace(
            Comment);

    public string ValueText =>
        InputType switch
        {
            InspectionInputType.NormalAbnormal =>
                CheckValue switch
                {
                    true => "正常",
                    false => "異常",
                    null => "未入力"
                },

            InspectionInputType.DoneNotDone =>
                CheckValue switch
                {
                    true => "実施",
                    false => "未実施",
                    null => "未入力"
                },

            InspectionInputType.Numeric =>
                NumericValue.HasValue
                    ? string.IsNullOrWhiteSpace(Unit)
                        ? NumericValue.Value.ToString()
                        : $"{NumericValue.Value} {Unit}"
                    : "未入力",

            InspectionInputType.Text =>
                string.IsNullOrWhiteSpace(TextValue)
                    ? "未入力"
                    : TextValue,

            _ =>
                "-"
        };

    [RelayCommand]
    private void OpenDetail()
    {
        _openDetailRequested(
            ScheduleId);
    }
}