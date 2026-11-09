using System.Collections.Generic;
using Cdsqg.Core.Entities;

namespace Cdsqg.Application.Services
{
    public class DigitalPlanSeedItem
    {
        public string AgencyName { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = new string[0];
        public string? ResponseDocument { get; set; }
        public bool HasReviewedPlan { get; set; }
        public string? DocumentNumber { get; set; }
        public bool IsUpdated1266 { get; set; }
        public string? Notes { get; set; }
    }

    public static class DigitalPlanSeedData
    {
        public static List<DigitalPlanSeedItem> GetSeedItems()
        {
            return new List<DigitalPlanSeedItem>
            {
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Quốc phòng",
                    Keywords = new[] { "Bộ Quốc phòng" },
                    ResponseDocument = "Số 6005/BQP-BTL86 ngày 13/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Kế hoạch số 7076/KH-BQP ngày 22/9/2026 của Bộ Quốc phòng ban hành Kế hoạch Chuyển đổi số giai đoạn 2026-2030",
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Công an",
                    Keywords = new[] { "Bộ Công an" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Ngoại giao",
                    Keywords = new[] { "Bộ Ngoại giao" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Nội vụ",
                    Keywords = new[] { "Bộ Nội vụ" },
                    ResponseDocument = "Số 8775/BNV ngày 12/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Quyết định số 428/QĐ-BNV ngày 14/4/2026 ban hành kế hoạch chuyể đổi số của Bộ nội vụ giai đoạn 2026-2030",
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Tư pháp",
                    Keywords = new[] { "Bộ Tư pháp" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Tài chính",
                    Keywords = new[] { "Bộ Tài chính" },
                    ResponseDocument = "Số 2213/CNTT ngày 12/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Quyết định số 3049/QĐ-BTC ngày 21/10/2026 ban hành Kế hoạch Chuyển đổi số của Bộ Tài chính giai đoạn 2026-2030",
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Công Thương",
                    Keywords = new[] { "Bộ Công Thương" },
                    ResponseDocument = "Số 1870/TMĐT ngày 18/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Quyết định số 405/QĐ-BCT ngày 9/3/2026 ban hành kế hoạch Chuyển đổi số của Bộ Công Thương giai đoạn 2026-2030",
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Nông nghiệp và Môi trường",
                    Keywords = new[] { "Bộ Nông nghiệp và Môi trường" },
                    ResponseDocument = "Số 1675/CĐS ngày 24/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Quyết định số 3254/QĐ-BNNMT ngày 14/8/2026 ban hành Kế hoạch Chuyển đổi số của Bộ Nông nghiệp và Môi trường giai đoạn 2026-2030",
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Xây dựng",
                    Keywords = new[] { "Bộ Xây dựng" },
                    ResponseDocument = "Số 13014/BXD-TTCNTT ngày 18/8/2026",
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = "đang rà soát - dự thảo kế hoạch"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Văn hóa, Thể thao và Du lịch",
                    Keywords = new[] { "Bộ Văn hóa, Thể thao và Du lịch" },
                    ResponseDocument = "Số 1120/CĐSVHTTDL ngày 12/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Quyết định số 4220/QĐ-BVHTTDL ngày 5/11/2025 ban hành Chiến lược Chuyển đổi số của Bộ Van hóa, Thể thao và Du lich giai đoạn 2025-2030",
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Khoa học và Công nghệ",
                    Keywords = new[] { "Bộ Khoa học và Công nghệ" },
                    ResponseDocument = null,
                    HasReviewedPlan = true,
                    DocumentNumber = "Số 2227/QĐ-BKHCN ngày 24/4/2026",
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Giáo dục và Đào tạo",
                    Keywords = new[] { "Bộ Giáo dục và Đào tạo" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Y tế",
                    Keywords = new[] { "Bộ Y tế" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bộ Dân tộc và Tôn giáo",
                    Keywords = new[] { "Bộ Dân tộc và Tôn giáo" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Ngân hàng Nhà nước Việt Nam",
                    Keywords = new[] { "Ngân hàng Nhà nước Việt Nam" },
                    ResponseDocument = "Số 7121/NHNN-CNTT ngày 7/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = "Quyết định số 1344/QĐ-NHNN ngày 23/6/2026 ban hành Kế hoạch chuyển đổi số của Ngân hàng Nhà nước giai đoạn 2026-2030Đang rà soát theo Cv 7121/NHNN"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Thanh tra Chính phủ",
                    Keywords = new[] { "Thanh tra Chính phủ" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Văn phòng Chính phủ",
                    Keywords = new[] { "Văn phòng Chính phủ" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Tuyên Quang",
                    Keywords = new[] { "Tuyên Quang" },
                    ResponseDocument = "Số 2888/SKHCN-CĐS ngày 11/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Quyết định số 2310/QĐ-UBND ngày 19/9/2026 của UBND tỉnh Tuyên Quang điều chỉnh, bổ sung một số nội dung Quyết định số 1539/QĐ-UBND ngày 11/12/2025 của Ủy ban nhân dân tỉnh phê duyệt Chiến lược Chuyển đổi số tỉnh Tuyên Quang đến năm 2030",
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Lào Cai",
                    Keywords = new[] { "Lào Cai" },
                    ResponseDocument = "Số 2650/SKHCN-CĐS ngày 14/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Kế hoạch số 399/KH-UBND ngày 21/8/2026 của UBND tỉnh Lào Cai Chuyển đổi số tỉnh Lào Cai giai đoạn 2026 - 2030",
                    IsUpdated1266 = true,
                    Notes = "Văn bản báo cáo của Lào Cai không bao gồm nội dung rà soát"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Thái Nguyên",
                    Keywords = new[] { "Thái Nguyên" },
                    ResponseDocument = null,
                    HasReviewedPlan = true,
                    DocumentNumber = "Kế hoạch số 380/KH-UBND ngày 26/8/2026 của UBND tỉnh Thái Nguyên thực hiện Quyết định số 1266/QĐ-TTg ngày 14 tháng 7 năm 2026 của Thủ tướng Chính phủ phê duyệt Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045 trên địa bàn tỉnh Thái Nguyên",
                    IsUpdated1266 = true,
                    Notes = "Không có CV gửi đầu mối"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Phú Thọ",
                    Keywords = new[] { "Phú Thọ" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Bắc Ninh",
                    Keywords = new[] { "Bắc Ninh" },
                    ResponseDocument = "Số 3556/SKHCN-CĐS ngày 15/8/2026",
                    HasReviewedPlan = false,
                    DocumentNumber = "Chỉ có dự thảo kế hoạch",
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Hưng Yên",
                    Keywords = new[] { "Hưng Yên" },
                    ResponseDocument = null,
                    HasReviewedPlan = true,
                    DocumentNumber = "Số 278/KH-UBND ngày 13/7/2026",
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Thành phố Hải Phòng",
                    Keywords = new[] { "Thành phố Hải Phòng" },
                    ResponseDocument = "Số 3529/SKHCN-CNTT ngày 10/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "1167/QĐ-UBND ngày 31/3/2026 (chiến lược)",
                    IsUpdated1266 = false,
                    Notes = "Chưa có kế hoạch"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Ninh Bình",
                    Keywords = new[] { "Ninh Bình" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Quảng Trị",
                    Keywords = new[] { "Quảng Trị" },
                    ResponseDocument = "Số 3362/SKHCN-CĐS ngày 10/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "số 1785/KH-UBND ngày 7/4/2026",
                    IsUpdated1266 = true,
                    Notes = "Thực hiện Quyết định số 2629/QĐ-TTg ngày 01/12/2025 của Thủ tướng Chính phủ, UBND tỉnh Quảng trị đã ban hành Kế hoạch số 1785/KH-UBND ngày 07/4/2026 về chuyển đổi số tỉnh Quảng Trị giai đoạn 2026 - 2030 và Kế hoạch 3472/KH-UBND ngày 15/7/2026 về phát triển kinh tế số và xã hội số tỉnh Quảng Trị giai đoạn 2026 - 2030. Nhìn chung, các kế hoạch triển khai đã bám sát quan điểm, mục tiêu, chỉ tiêu, nhiệm vụ, giải pháp của Trung ương, phù hợp với Nghị quyết số 57-NQ/TW, Luật Chuyển đổi số, các chương trình, kế hoạch có liên quan và phù hợp với điều kiện thực tiễn, tình hình phát triển kinh tế, xã hội, nguồn lực của tỉnh Quảng Trị; nội dung của 02 Kế hoạch do UBND tỉnh ban hành đã xác định rõ nhiệm vụ trọng tâm, trọng điểm giai đoạn 2026 -2030, nhiệm vụ ưu tiên hằng năm, sản phẩm đầu ra, tiến độ thực hiện, đơn vị chủ trì, đơn vị phối hợp, nguồn lực thực hiện và các chỉ tiêu theo dõi, đánh giá kết quả cụ thể. Các nhiệm vụ được triển khai đồng bộ, có tính liên thông, phát triển chuyển đổi số toàn diện trên cả 3 trụ cột là chính quyền số, kinh tế số, xã hội số, có tính đột phá, tạo động lực phát triển nhanh, bền vững trên tất cả các lĩnh vực của tỉnh Quảng Trị; đảm bảo phù hợp với các nội dung của Quyết định số 1266/QĐ-TTg ngày 14/7/2026 của Thủ tướng Chính phủ. (Theo CV số 3362 ngày 10/8/2026)"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Thành phố Đà Nẵng",
                    Keywords = new[] { "Thành phố Đà Nẵng" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Quảng Ngãi",
                    Keywords = new[] { "Quảng Ngãi" },
                    ResponseDocument = "Số 4062/SHKCN-BCVT ngày 11/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Kế hoạch số 298/KH-UBND ngày 7/8/2026 Triển khai thực hiện Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045 trên địa bàn tỉnh Quảng Ngãi",
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Gia Lai",
                    Keywords = new[] { "Gia Lai" },
                    ResponseDocument = null,
                    HasReviewedPlan = true,
                    DocumentNumber = "Quyết định số 4155/QĐ-UBND ngày 1/10/2026 của UBND tỉnh Gia Lai ban hành Kế hoạch chuyển đổi số tỉnh Gia Lai  giai đoạn 2026 – 2030",
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Khánh Hoà",
                    Keywords = new[] { "Khánh Hoà" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Lâm Đồng",
                    Keywords = new[] { "Lâm Đồng" },
                    ResponseDocument = "Số 4099/SKHCN-CĐS ngày 20/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Số 6388/KH-UBND ngày 8/5/2026",
                    IsUpdated1266 = false,
                    Notes = "Văn bản báo cáo của Lâm Đồng không bao gồm nội dung rà soát"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Đắk Lắk",
                    Keywords = new[] { "Đắk Lắk" },
                    ResponseDocument = "Số 3960/SKHCN-CĐS ngày 4/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Có danh mục bổ sung nhiệm vụ của kế hoach số 95/KH-UBND ngày 25/3/2026",
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Thành phố Hồ Chí Minh",
                    Keywords = new[] { "Thành phố Hồ Chí Minh" },
                    ResponseDocument = "Số 8071/SKHCN-CĐS ngày 15/8/2026",
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = "TP HCM đang rà soát"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Đồng Nai",
                    Keywords = new[] { "Đồng Nai" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Tây Ninh",
                    Keywords = new[] { "Tây Ninh" },
                    ResponseDocument = "Số 15054/UBND-VHXH ngày 18/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Số 482/KH_UBND ngày 2/2/2026",
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Thành phố Cần Thơ",
                    Keywords = new[] { "Thành phố Cần Thơ" },
                    ResponseDocument = "Số 4349/SKHCN-CĐS ngày 19/8/2026",
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Vĩnh Long",
                    Keywords = new[] { "Vĩnh Long" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Đồng Tháp",
                    Keywords = new[] { "Đồng Tháp" },
                    ResponseDocument = "Số 4304/SKH&CN-CĐS ngày 17/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Số 1109/KH-UBND ngày 15/7/2026",
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Cà Mau",
                    Keywords = new[] { "Cà Mau" },
                    ResponseDocument = "Số 4325/SKHCN-CNSBC ngày 17/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Kế hoạch số 556/KH-UBND ngày 17/9/2026 ngày 17/9/2026 Chuyển đổi số trên địa bàn tỉnh Cà Mau giai đoạn 2026 - 2030, định hướng đến năm 2045",
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "An Giang",
                    Keywords = new[] { "An Giang" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Thành phố Hà Nội",
                    Keywords = new[] { "Thành phố Hà Nội" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Thành phố Huế",
                    Keywords = new[] { "Thành phố Huế" },
                    ResponseDocument = "Số 4105/SKHCN-CĐS ngày 14/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Kế hoạch số 469/KH-UBND ngày 14/8/2026 Triển khai Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045 trên địa bàn thành phố Huế",
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Lai Châu",
                    Keywords = new[] { "Lai Châu" },
                    ResponseDocument = "Số 3466/SKHCN-BCVTCNNTT ngày 13/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Số 576/KH-UBND ngày 23/1/2026",
                    IsUpdated1266 = false,
                    Notes = "Văn bản báo cáo của Lai Châu không bao gồm nội dung rà soát"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Điện Biên",
                    Keywords = new[] { "Điện Biên" },
                    ResponseDocument = "Số 5011/SKHCN-KĐC ngày 9/11/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Số 1898/KH-UBND ngày 17/3/2026",
                    IsUpdated1266 = true,
                    Notes = "Ngày 17/3/2026, UBND tỉnh Điện Biên đã ban hành Kế hoạch số 1898/KH-UBND Chuyển đổi số tỉnh Điện Biên giai đoạn 2026 - 2030. Ngoài ra, tỉnh Điện Biên đã ban hành Nghị quyết số 17-NQ/TU ngày 27/02/2026 của Ban Chấp hành Đảng bộ tỉnh về đột phá phát triển khoa học, công nghệ, đổi mới sáng tạo và chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2035; Kế hoạch số 2092/KH-UBND ngày 23/3/2026 của UBND tỉnh thực hiện Nghị quyết số 17-NQ/TU ngày 27/02/2026 của Ban Chấp hành Đảng bộ tỉnh về đột phá phát triển khoa học, công nghệ, đổi mới sáng tạo và chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2035 (Có Nghị quyết, Kế hoạch kèm theo). Các nội dung tại Nghị quyết, Kế hoạch bảo đảm bám sát quan điểm, mục tiêu, chỉ tiêu, nhiệm vụ, giải pháp của Quyết định số 1266/QĐ-TTg ngày 14/7/2026 của Thủ tướng Chính phủ phê duyệt Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045 và phù hợp với Nghị quyết số 57-NQ/TW, Luật Chuyển đổi số và các chương trình, kế hoạch có liên quan; trong đó, xác định rõ nhiệm vụ trọng tâm giai đoạn 2026 - 2030, nhiệm vụ ưu tiên hằng năm, sản phẩm đầu ra, tiến độ thực hiện, đơn vị chủ trì, đơn vị phối hợp, nguồn lực thực hiện và chỉ tiêu theo dõi, đánh giá kết quả. (theo CV số 5011/SKHCN-KĐC ngày 9/11/2026)"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Sơn La",
                    Keywords = new[] { "Sơn La" },
                    ResponseDocument = "Số 394/BC-SKHCN ngày 14/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Quyết định số 2364/QĐ-UBND ngày 16/9/2026 QĐ cập nhật bổ sung 1 số chỉ tiêu, nhiệm vụ...2026-2030",
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Lạng Sơn",
                    Keywords = new[] { "Lạng Sơn" },
                    ResponseDocument = "Số 4629/BC-SKHCN ngày 14/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Kế hoạch số 338/KH-UBND ngày 1/9/2026  Triển khai thực hiện Chiến lược quốc gia về chuyển đổi số giai đoạn  2026 - 2030, định hướng đến năm 2045 trên địa bàn tỉnh Lạng Sơn",
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Quảng Ninh",
                    Keywords = new[] { "Quảng Ninh" },
                    ResponseDocument = "Số 3987/SKHCN-QL ngày 14/8/2026",
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = "Tỉnh đang tổng hợp"
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Thanh Hoá",
                    Keywords = new[] { "Thanh Hoá" },
                    ResponseDocument = "Số 3730/SKHCN-CNTT ngày 14/8/2026",
                    HasReviewedPlan = true,
                    DocumentNumber = "Kế hoạch số 282/KH-UBND ngày 4/8/2026 của UBND tỉnh Thanh Hóa triển khai thực hiện Chiến lược quốc gia về chuyển đổi số trên địa bàn tỉnh Thanh Hóa giai đoạn 2026 - 2030, định hướng đến năm 2045",
                    IsUpdated1266 = true,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Nghệ An",
                    Keywords = new[] { "Nghệ An" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Hà Tĩnh",
                    Keywords = new[] { "Hà Tĩnh" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
                new DigitalPlanSeedItem
                {
                    AgencyName = "Cao Bằng",
                    Keywords = new[] { "Cao Bằng" },
                    ResponseDocument = null,
                    HasReviewedPlan = false,
                    DocumentNumber = null,
                    IsUpdated1266 = false,
                    Notes = null
                },
            };
        }
    }
}
