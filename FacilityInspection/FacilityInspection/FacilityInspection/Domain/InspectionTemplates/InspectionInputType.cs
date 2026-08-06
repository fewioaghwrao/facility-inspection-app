namespace FacilityInspection.Domain.InspectionTemplates;

public enum InspectionInputType
{
    /// <summary>
    /// 正常・異常で入力する項目。
    /// </summary>
    NormalAbnormal = 1,

    /// <summary>
    /// 実施・未実施で入力する項目。
    /// </summary>
    DoneNotDone = 2,

    /// <summary>
    /// 圧力や電流などの数値入力。
    /// </summary>
    Numeric = 3,

    /// <summary>
    /// 備考などの自由入力。
    /// </summary>
    Text = 4
}