namespace Cdsqg.Core.Enums
{
    public enum AgencyTypeEnum
    {
        Ministry = 1,  // Bộ / Ngành
        Province = 2,  // Tỉnh / Thành phố
        Internal = 3,  // Đơn vị nội bộ
        Other = 4      // Khác
    }

    public enum UnitDataTypeEnum
    {
        Integer = 1,
        Decimal = 2,
        Boolean = 3,
        Text_Status = 4
    }

    public enum TimeResolutionEnum
    {
        SpecificYear = 1,
        Range = 2,        // e.g. 2026-2030
        Continuous = 3   // Thường xuyên
    }

    public enum ItemTypeEnum
    {
        Goal = 1, // Level 1A: Mục tiêu
        Task = 2  // Level 1B: Nhiệm vụ
    }

    public enum EvaluationTypeEnum
    {
        Quantitative = 1, // Định lượng (Số)
        Qualitative = 2   // Định tính (Trạng thái văn bản)
    }

    public enum CalculationMethodEnum
    {
        Cumulative = 1,  // Cộng dồn
        LatestValue = 2  // Ghi đè (Giá trị mới nhất)
    }

    public enum TextStatusEnum
    {
        NotStarted = 1, // Chưa thực hiện (Not Started)
        Drafting = 2,   // Đang xây dựng / Soạn thảo (Drafting)
        Reviewing = 3,  // Đang xin ý kiến / Đánh giá (Reviewing)
        Completed = 4   // Đã hoàn thành (Completed)
    }

    public enum AlertStatusEnum
    {
        Green = 1,  // On track (Đúng tiến độ)
        Yellow = 2, // Warning (Cảnh báo tiến độ)
        Red = 3     // Overdue / Failing (Chậm tiến độ)
    }

    /// <summary>
    /// 6 Trạng thái chuẩn đánh giá tiến độ thực hiện Mục tiêu / Nhiệm vụ theo QĐ 1266
    /// </summary>
    public enum ExecutionStatusEnum
    {
        NotStarted = 1,         // 1. Chưa thực hiện
        InProgressOnTime = 2,   // 2. Đang thực hiện (trong hạn)
        InProgressOverdue = 3,  // 3. Đang thực hiện (quá hạn)
        CompletedOnTime = 4,    // 4. Hoàn thành (đúng hạn)
        CompletedOverdue = 5,   // 5. Hoàn thành (quá hạn)
        ExpiringSoon = 6        // 6. Sắp hết hạn (Nhiệm vụ cha <= 30 ngày, Nhiệm vụ con <= 10% tổng thời gian)
    }
}
