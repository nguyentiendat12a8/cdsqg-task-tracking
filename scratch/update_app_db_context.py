with open('backend/Infrastructure/Data/AppDbContext.cs', 'r', encoding='utf-8') as f:
    content = f.read()

target = '            modelBuilder.Entity<AiGoalKeyword>().Property(x => x.ExpectedUnit).HasMaxLength(50);'
seed_code = '''            modelBuilder.Entity<AiGoalKeyword>().Property(x => x.ExpectedUnit).HasMaxLength(50);
            modelBuilder.Entity<AiGoalKeyword>().HasData(
                new AiGoalKeyword
                {
                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000012"),
                    GoalCode = "MT-12",
                    GoalTitle = "Việt Nam thuộc nhóm 50 nước dẫn đầu trên thế giới về chính phủ điện tử/chính phủ số (theo báo cáo đánh giá của Liên hợp quốc)",
                    KeywordsVi = "chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí",
                    KeywordsEn = "e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam",
                    ExpectedUnit = "rank",
                    ExtractionPrompt = "Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank).",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new AiGoalKeyword
                {
                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000025"),
                    GoalCode = "MT-25",
                    GoalTitle = "Việt Nam thuộc nhóm 30 nước dẫn đầu thế giới về chính phủ điện tử/chính phủ số (theo báo cáo đánh giá của Liên hợp quốc)",
                    KeywordsVi = "chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí",
                    KeywordsEn = "e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam",
                    ExpectedUnit = "rank",
                    ExtractionPrompt = "Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank).",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new AiGoalKeyword
                {
                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000013"),
                    GoalCode = "MT-13",
                    GoalTitle = "Quy mô kinh tế số đạt tối thiểu 30% GDP",
                    KeywordsVi = "kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, giá trị gia tăng",
                    KeywordsEn = "digital economy, gdp, share of gdp, gross domestic product, digital economy share, vietnam",
                    ExpectedUnit = "%",
                    ExtractionPrompt = "Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%).",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new AiGoalKeyword
                {
                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000016"),
                    GoalCode = "MT-16",
                    GoalTitle = "Tỷ trọng doanh thu thương mại điện tử so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng đạt từ 15 - 17%",
                    KeywordsVi = "thương mại điện tử, tmđt, bán lẻ, doanh thu dịch vụ tiêu dùng, sách trắng tmđt, tỷ trọng b2c",
                    KeywordsEn = "e-commerce, ecommerce, retail, consumer services, b2c, online retail share, vietnam",
                    ExpectedUnit = "%",
                    ExtractionPrompt = "Xác định tỷ trọng doanh thu thương mại điện tử (B2C) so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng của cả nước (%).",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new AiGoalKeyword
                {
                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000018"),
                    GoalCode = "MT-18",
                    GoalTitle = "Giá trị thanh toán không dùng tiền mặt gấp 30 lần GDP",
                    KeywordsVi = "thanh toán không dùng tiền mặt, thanh toán điện tử, ngân hàng nhà nước, gdp, lần gdp",
                    KeywordsEn = "cashless payment, non-cash payment, digital payment, times gdp, ratio to gdp, vietnam",
                    ExpectedUnit = "lần GDP",
                    ExtractionPrompt = "Xác định tỷ lệ giá trị thanh toán không dùng tiền mặt so với GDP (số lần GDP).",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new AiGoalKeyword
                {
                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000020"),
                    GoalCode = "MT-20",
                    GoalTitle = "Hạ tầng mạng băng rộng cáp quang phủ 100% hộ gia đình",
                    KeywordsVi = "cáp quang, băng rộng cố định, ftth, hộ gia đình, thuê bao internet",
                    KeywordsEn = "fiber optic, ftth, fixed broadband, households, broadband penetration, vietnam",
                    ExpectedUnit = "%",
                    ExtractionPrompt = "Xác định tỷ lệ hộ gia đình có kết nối mạng băng rộng cáp quang (FTTH) (%).",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new AiGoalKeyword
                {
                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000021"),
                    GoalCode = "MT-21",
                    GoalTitle = "Tỷ lệ dân số từ 15 tuổi trở lên có tài khoản giao dịch tại ngân hàng hoặc tổ chức được phép khác đạt tối thiểu 95%",
                    KeywordsVi = "tài khoản giao dịch, ngân hàng, thanh toán, 15 tuổi, tổ chức tín dụng",
                    KeywordsEn = "bank account, transaction account, adults, banked population, financial inclusion, vietnam",
                    ExpectedUnit = "%",
                    ExtractionPrompt = "Xác định tỷ lệ dân số từ 15 tuổi trở lên có tài khoản thanh toán / giao dịch tại ngân hàng (%).",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new AiGoalKeyword
                {
                    Id = Guid.Parse("b1000000-0000-0000-0000-000000000022"),
                    GoalCode = "MT-22",
                    GoalTitle = "Tỷ lệ dân số từ 18 tuổi trở lên có chữ ký số hoặc chữ ký điện tử cá nhân đạt trên 70%",
                    KeywordsVi = "chữ ký số, chữ ký điện tử cá nhân, 18 tuổi, chứng thư số, ký số",
                    KeywordsEn = "digital signature, electronic signature, personal digital signature, e-signature, vietnam",
                    ExpectedUnit = "%",
                    ExtractionPrompt = "Xác định tỷ lệ dân số từ 18 tuổi trở lên có chữ ký số hoặc chữ ký điện tử cá nhân (%).",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );'''

if target in content:
    content = content.replace(target, seed_code, 1)
    with open('backend/Infrastructure/Data/AppDbContext.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(content)
    print("Successfully updated AppDbContext.cs")
else:
    print("Target not found")
