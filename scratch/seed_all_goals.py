# Full list of 26 goals with comprehensive keywords and prompts
goals_data = [
    {
        "code": "MT-01",
        "title": "100% hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa",
        "keywords_vi": "số hóa hồ sơ, số hóa kết quả, thủ tục hành chính, tthc, hồ sơ trực tuyến, dịch vụ công, lưu trữ điện tử, bưu chính công ích",
        "keywords_en": "digitization, digitized administrative records, public administrative services, digitized results, digital archives, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa (%)."
    },
    {
        "code": "MT-02",
        "title": "99% người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến",
        "keywords_vi": "đánh giá hài lòng, mức độ hài lòng, dịch vụ công trực tuyến, người dân doanh nghiệp, cổng dịch vụ công quốc gia, chỉ số hài lòng",
        "keywords_en": "user satisfaction, citizen satisfaction, satisfaction rate, online public services, national public service portal, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến (%)."
    },
    {
        "code": "MT-03",
        "title": "100% bộ, ngành, địa phương sử dụng thống nhất, hiệu quả các nền tảng số dùng chung quốc gia phục vụ chỉ đạo, điều hành và cung cấp dịch vụ công",
        "keywords_vi": "nền tảng số dùng chung, bộ ngành địa phương, chỉ đạo điều hành, dịch vụ công, trục liên thông văn bản, ndxp",
        "keywords_en": "shared digital platforms, ministries, provinces, national digital platforms, public service delivery, data exchange, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ bộ, ngành, địa phương sử dụng nền tảng số dùng chung quốc gia phục vụ chỉ đạo điều hành và dịch vụ công (%)."
    },
    {
        "code": "MT-04",
        "title": "100% hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu",
        "keywords_vi": "kết nối chia sẻ dữ liệu, hệ thống thông tin, nền tảng chia sẻ dữ liệu quốc gia, ndxp, lgsp, tích hợp dữ liệu",
        "keywords_en": "information systems, data sharing, data exchange, ndxp, national data exchange platform, interoperability, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ hệ thống thông tin có kết nối, chia sẻ dữ liệu qua nền tảng tích hợp, chia sẻ dữ liệu (%)."
    },
    {
        "code": "MT-05",
        "title": "100% cơ sở dữ liệu dùng chung quốc gia, ngành, lĩnh vực, cấp tỉnh được chuẩn hóa, kết nối, phân quyền và sẵn sàng khai thác cho AI",
        "keywords_vi": "cơ sở dữ liệu dùng chung, csdl quốc gia, csdl chuyên ngành, chuẩn hóa dữ liệu, sẵn sàng khai thác ai, trí tuệ nhân tạo",
        "keywords_en": "shared databases, national databases, standardized data, ai readiness, artificial intelligence, open data, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ cơ sở dữ liệu dùng chung quốc gia, ngành, địa phương được chuẩn hóa và sẵn sàng kết nối, khai thác AI (%)."
    },
    {
        "code": "MT-06",
        "title": "100% trung tâm điều hành thông minh cấp tỉnh được nâng cấp theo hướng tích hợp AI phục vụ phân tích, dự báo, cảnh báo, giám sát và hỗ trợ ra quyết định",
        "keywords_vi": "trung tâm điều hành thông minh, ioc, cấp tỉnh, tích hợp ai, dự báo cảnh báo, giám sát ra quyết định",
        "keywords_en": "intelligent operations center, ioc, provincial operations center, ai integration, predictive analytics, smart city, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ trung tâm điều hành thông minh (IOC) cấp tỉnh được nâng cấp tích hợp AI (%)."
    },
    {
        "code": "MT-07",
        "title": "100% cán bộ, công chức, viên chức trong cơ quan nhà nước có hồ sơ điện tử theo quy định",
        "keywords_vi": "cán bộ công chức viên chức, hồ sơ điện tử, quản lý cán bộ, cơ sở dữ liệu cán bộ công chức, cbccvc, bộ nội vụ",
        "keywords_en": "civil servants, public employees, electronic personnel records, civil servant database, ministry of home affairs, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ cán bộ, công chức, viên chức trong cơ quan nhà nước có hồ sơ điện tử (%)."
    },
    {
        "code": "MT-08",
        "title": "70% số chỉ tiêu thống kê thuộc các hệ thống chỉ tiêu thống kê được biên soạn từ nguồn dữ liệu hành chính",
        "keywords_vi": "chỉ tiêu thống kê, dữ liệu hành chính, hệ thống chỉ tiêu, tổng cục thống kê, cục thống kê, số liệu hành chính",
        "keywords_en": "statistical indicators, administrative data sources, national statistical system, general statistics office, gso, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ chỉ tiêu thống kê được biên soạn từ nguồn dữ liệu hành chính (%)."
    },
    {
        "code": "MT-09",
        "title": "100% cán bộ lãnh đạo, cán bộ, công chức, viên chức trong cơ quan nhà nước được đào tạo, bồi dưỡng, cập nhật kiến thức, kỹ năng số",
        "keywords_vi": "đào tạo kỹ năng số, bồi dưỡng kiến thức số, cán bộ công chức, chuyển đổi số công vụ, tập huấn kỹ năng số",
        "keywords_en": "digital skills training, digital literacy, civil servants training, public sector digital skills, government workforce, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ cán bộ, công chức, viên chức được đào tạo, bồi dưỡng cập nhật kỹ năng số (%)."
    },
    {
        "code": "MT-10",
        "title": "100% cán bộ chuyên trách về chuyển đổi số, công nghệ thông tin được đào tạo, bồi dưỡng, cập nhật công nghệ số",
        "keywords_vi": "chuyên trách cntt, chuyên trách chuyển đổi số, bồi dưỡng công nghệ số, cán bộ công nghệ thông tin",
        "keywords_en": "it specialists, digital transformation officers, specialized it training, technology capacity, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ cán bộ chuyên trách chuyển đổi số, CNTT được đào tạo, bồi dưỡng nâng cao công nghệ số (%)."
    },
    {
        "code": "MT-11",
        "title": "100% cán bộ, công chức, viên chức có tài khoản và được đào tạo sử dụng nền tảng trợ lý AI công vụ",
        "keywords_vi": "trợ lý ai công vụ, trợ lý ảo công vụ, tài khoản trợ lý ai, đào tạo trợ lý ảo, ai phục vụ cán bộ",
        "keywords_en": "public service ai assistant, virtual assistant for civil servants, ai assistant accounts, public sector ai, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ cán bộ, công chức, viên chức có tài khoản và được tập huấn sử dụng trợ lý AI công vụ (%)."
    },
    {
        "code": "MT-12",
        "title": "Việt Nam thuộc nhóm 50 nước dẫn đầu trên thế giới về chính phủ điện tử/chính phủ số (theo báo cáo đánh giá của Liên hợp quốc)",
        "keywords_vi": "chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí",
        "keywords_en": "e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam",
        "unit": "rank",
        "prompt": "Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank)."
    },
    {
        "code": "MT-13",
        "title": "Quy mô kinh tế số đạt tối thiểu 30% GDP",
        "keywords_vi": "kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, giá trị gia tăng",
        "keywords_en": "digital economy, gdp, share of gdp, gross domestic product, digital economy share, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%)."
    },
    {
        "code": "MT-14",
        "title": "Hỗ trợ tối thiểu 500.000 doanh nghiệp nhỏ và vừa chuyển đổi số",
        "keywords_vi": "doanh nghiệp nhỏ và vừa, sme, chuyển đổi số doanh nghiệp, smedx, hỗ trợ doanh nghiệp",
        "keywords_en": "smes, small and medium enterprises, enterprise digital transformation, smedx, business support, vietnam",
        "unit": "doanh nghiệp",
        "prompt": "Xác định số lượng doanh nghiệp nhỏ và vừa (SME) được hỗ trợ chuyển đổi số (đơn vị: doanh nghiệp)."
    },
    {
        "code": "MT-15",
        "title": "Phát triển, thúc đẩy và đưa vào sử dụng tối thiểu 5 sàn dữ liệu, bảo đảm an toàn, tuân thủ quy định của pháp luật",
        "keywords_vi": "sàn dữ liệu, trung tâm giao dịch dữ liệu, thị trường dữ liệu, data exchange, sàn giao dịch dữ liệu",
        "keywords_en": "data exchange platform, data marketplace, data trading, data exchanges, vietnam",
        "unit": "sàn",
        "prompt": "Xác định số lượng sàn dữ liệu / trung tâm giao dịch dữ liệu được đưa vào vận hành (đơn vị: sàn)."
    },
    {
        "code": "MT-16",
        "title": "Tỷ trọng doanh thu thương mại điện tử so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng đạt từ 15 - 17%",
        "keywords_vi": "thương mại điện tử, tmđt, bán lẻ, doanh thu dịch vụ tiêu dùng, sách trắng tmđt, tỷ trọng b2c",
        "keywords_en": "e-commerce, ecommerce, retail, consumer services, b2c, online retail share, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ trọng doanh thu thương mại điện tử (B2C) so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng của cả nước (%)."
    },
    {
        "code": "MT-17",
        "title": "Phấn đấu đạt 45% số doanh nghiệp nhỏ và vừa hoạt động trong lĩnh vực thương mại tham gia các sàn thương mại điện tử lớn trong và ngoài nước",
        "keywords_vi": "sme thương mại, sàn thương mại điện tử, tham gia sàn tmđt, shopee, lazada, tiki, alibaba, amazon, bán hàng trực tuyến",
        "keywords_en": "smes on ecommerce platforms, ecommerce marketplace participation, online retail smes, e-marketplace, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ doanh nghiệp nhỏ và vừa ngành thương mại tham gia các sàn thương mại điện tử (%)."
    },
    {
        "code": "MT-18",
        "title": "Giá trị thanh toán không dùng tiền mặt gấp 30 lần GDP",
        "keywords_vi": "thanh toán không dùng tiền mặt, thanh toán điện tử, ngân hàng nhà nước, gdp, lần gdp, giao dịch phi tiền mặt",
        "keywords_en": "cashless payment, non-cash payment, digital payment, times gdp, ratio to gdp, cashless transactions, vietnam",
        "unit": "lần GDP",
        "prompt": "Xác định tỷ lệ giá trị thanh toán không dùng tiền mặt so với GDP (số lần GDP)."
    },
    {
        "code": "MT-19",
        "title": "Trên 40% doanh nghiệp nhỏ và vừa tiếp cận, sử dụng nền tảng “mô hình như một dịch vụ”, tác nhân AI hoặc giải pháp AI dùng chung",
        "keywords_vi": "mô hình như một dịch vụ, maas, tác nhân ai, ai agent, giải pháp ai dùng chung, sme ứng dụng ai",
        "keywords_en": "model as a service, maas, ai agents, shared ai solutions, smes adopting ai, ai adoption rate, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ doanh nghiệp nhỏ và vừa tiếp cận hoặc sử dụng giải pháp AI / mô hình như một dịch vụ (%)."
    },
    {
        "code": "MT-20",
        "title": "Hạ tầng mạng băng rộng cáp quang phủ 100% hộ gia đình",
        "keywords_vi": "cáp quang, băng rộng cố định, ftth, hộ gia đình, thuê bao internet, phủ cáp quang",
        "keywords_en": "fiber optic, ftth, fixed broadband, households, broadband penetration, fiber coverage, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ hộ gia đình có kết nối mạng băng rộng cáp quang (FTTH) (%)."
    },
    {
        "code": "MT-21",
        "title": "Tỷ lệ dân số từ 15 tuổi trở lên có tài khoản giao dịch tại ngân hàng hoặc tổ chức được phép khác đạt tối thiểu 95%",
        "keywords_vi": "tài khoản giao dịch, ngân hàng, thanh toán, 15 tuổi, tổ chức tín dụng, tài khoản thanh toán",
        "keywords_en": "bank account, transaction account, adults, banked population, financial inclusion, account ownership, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ dân số từ 15 tuổi trở lên có tài khoản thanh toán / giao dịch tại ngân hàng (%)."
    },
    {
        "code": "MT-22",
        "title": "Tỷ lệ dân số từ 18 tuổi trở lên có chữ ký số hoặc chữ ký điện tử cá nhân đạt trên 70%",
        "keywords_vi": "chữ ký số, chữ ký điện tử cá nhân, 18 tuổi, chứng thư số, ký số cá nhân, vnpt smartca, viettel ca",
        "keywords_en": "digital signature, electronic signature, personal digital signature, e-signature, digital certificate, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ dân số từ 18 tuổi trở lên có chữ ký số hoặc chữ ký điện tử cá nhân (%)."
    },
    {
        "code": "MT-23",
        "title": "Phấn đấu tỷ lệ người dân được lập sổ sức khỏe điện tử đạt 100%",
        "keywords_vi": "sổ sức khỏe điện tử, hồ sơ sức khỏe điện tử, y tế số, vneid sức khỏe, bộ y tế, bảo hiểm y tế",
        "keywords_en": "electronic health record, ehr, personal health record, digital healthcare, ministry of health, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ lệ người dân được lập sổ / hồ sơ sức khỏe điện tử (%)."
    },
    {
        "code": "MT-24",
        "title": "Đào tạo, bồi dưỡng, tập huấn kỹ năng số cơ bản cho tối thiểu 10.000.000 lượt người trong độ tuổi lao động",
        "keywords_vi": "kỹ năng số cơ bản, độ tuổi lao động, tập huấn kỹ năng số, phổ cập kỹ năng số, lượt người",
        "keywords_en": "basic digital skills, working age population, digital skills training, digital literacy training, participants, vietnam",
        "unit": "lượt người",
        "prompt": "Xác định số lượt người trong độ tuổi lao động được đào tạo, bồi dưỡng kỹ năng số cơ bản (đơn vị: lượt người)."
    },
    {
        "code": "MT-25",
        "title": "Việt Nam thuộc nhóm 30 nước dẫn đầu thế giới về chính phủ điện tử/chính phủ số (theo báo cáo đánh giá của Liên hợp quốc)",
        "keywords_vi": "chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí",
        "keywords_en": "e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam",
        "unit": "rank",
        "prompt": "Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank)."
    },
    {
        "code": "MT-26",
        "title": "Quy mô kinh tế số đạt tối thiểu 50% GDP, kinh tế số trở thành một trong những động lực phát triển chủ yếu của nền kinh tế",
        "keywords_vi": "kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, 50% gdp, động lực phát triển",
        "keywords_en": "digital economy, gdp, share of gdp, 50% gdp, gross domestic product, digital economic growth, vietnam",
        "unit": "%",
        "prompt": "Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%)."
    }
]

# Generate C# seed statements
csharp_entries = []
for g in goals_data:
    num = int(g["code"].replace("MT-", ""))
    guid = f"b1000000-0000-0000-0000-{num:012d}"
    # Escape quotes
    title_esc = g["title"].replace('"', '\\"')
    prompt_esc = g["prompt"].replace('"', '\\"')
    entry = f'''                new AiGoalKeyword
                {{
                    Id = Guid.Parse("{guid}"),
                    GoalCode = "{g["code"]}",
                    GoalTitle = "{title_esc}",
                    KeywordsVi = "{g["keywords_vi"]}",
                    KeywordsEn = "{g["keywords_en"]}",
                    ExpectedUnit = "{g["unit"]}",
                    ExtractionPrompt = "{prompt_esc}",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }}'''
    csharp_entries.append(entry)

full_seed_block = "            modelBuilder.Entity<AiGoalKeyword>().HasData(\n" + ",\n".join(csharp_entries) + "\n            );"

# Read AppDbContext.cs
with open('backend/Infrastructure/Data/AppDbContext.cs', 'r', encoding='utf-8') as f:
    content = f.read()

start_marker = "            modelBuilder.Entity<AiGoalKeyword>().HasData("
end_marker = "            // ----------------------------------------------------"

start_idx = content.find(start_marker)
end_idx = content.find(end_marker, start_idx)

if start_idx != -1 and end_idx != -1:
    new_content = content[:start_idx] + full_seed_block + "\n\n\n" + content[end_idx:]
    with open('backend/Infrastructure/Data/AppDbContext.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(new_content)
    print("Updated AppDbContext.cs with 26 goals successfully")
else:
    print(f"Markers not found: start={start_idx}, end={end_idx}")
