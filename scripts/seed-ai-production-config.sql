-- ==============================================================================
-- Script cấu hình AI cho môi trường Production (PostgreSQL)
-- Ngày tạo: 2026-10-21
-- ==============================================================================

BEGIN;

-- 1. Cấu hình nhà cung cấp AI (Google Gemini / OpenAI Compatible endpoint)
-- Lưu ý: Cột "ProtectedApiKey" lưu khóa API đã được mã hóa bởi ASP.NET Core Data Protection.
-- Để giải mã được chuỗi này, server Production cần có thư mục khóa Data Protection (App_Data/ai-keys).
-- Nếu server Production dùng bộ khóa riêng, Quản trị viên có thể vào màn hình "Cấu hình AI" 
-- trên giao diện Web để nhập lại API key (hệ thống sẽ tự mã hóa tương thích với server Production).
INSERT INTO "AiProviderConfigurations" (
    "Id",
    "Enabled",
    "BaseUrl",
    "Model",
    "ProtectedApiKey",
    "UpdatedAt",
    "UpdatedBy"
)
VALUES (
    1,
    TRUE,
    'https://generativelanguage.googleapis.com/v1beta/openai',
    'models/gemini-3.5-flash',
    'CfDJ8D_vHy1rIdNLqFws8OC36Tgk64K9rsAadbY-oXQlGpRAKhBz78HDexkF8Ns6SilwQ0I8sKolHtQdOWDf0BsNLWYn776fotR2VOdoImFULwdx0vl77CCA21VdY-RgdjWB6Jz_4JLXsHvTGV50u7uMt-hSBxJ8Wv4K9otSigph6cfseZb2H0urftLINnxdG-DD3Q',
    CURRENT_TIMESTAMP,
    'Bộ KH & CN (Admin)'
)
ON CONFLICT ("Id") DO UPDATE SET
    "Enabled" = EXCLUDED."Enabled",
    "BaseUrl" = EXCLUDED."BaseUrl",
    "Model" = EXCLUDED."Model",
    "ProtectedApiKey" = CASE 
        WHEN EXCLUDED."ProtectedApiKey" IS NOT NULL AND EXCLUDED."ProtectedApiKey" <> '' 
        THEN EXCLUDED."ProtectedApiKey" 
        ELSE "AiProviderConfigurations"."ProtectedApiKey" 
    END,
    "UpdatedAt" = CURRENT_TIMESTAMP,
    "UpdatedBy" = EXCLUDED."UpdatedBy";

-- Đảm bảo sequence (nếu có) được đồng bộ
SELECT setval(pg_get_serial_sequence('"AiProviderConfigurations"', 'Id'), coalesce(max("Id"), 1)) FROM "AiProviderConfigurations";

-- 2. Đảm bảo trạng thái hàng đợi xử lý ngầm (AiInboxQueueState) sẵn sàng
INSERT INTO "AiInboxQueueState" ("Id", "Owner", "LeaseUntil", "NextReadAt")
VALUES (1, NULL, NULL, CURRENT_TIMESTAMP)
ON CONFLICT ("Id") DO UPDATE SET
    "Owner" = NULL,
    "LeaseUntil" = NULL,
    "NextReadAt" = LEAST("AiInboxQueueState"."NextReadAt", CURRENT_TIMESTAMP);

-- 3. Cấu hình 3 nguồn cấp dữ liệu chuẩn (Google Alerts RSS) theo 3 nhóm chỉ số ưu tiên
INSERT INTO "AiAlertFeeds" (
    "Id",
    "Name",
    "FeedUrl",
    "Enabled",
    "ScanMinute",
    "NextScanAt",
    "CreatedAt",
    "UpdatedBy"
)
VALUES
(
    'c4a989de-8c33-4cd0-9139-688eac54b524',
    'Thứ hạng EGDI toàn cầu của Việt Nam',
    'https://www.google.com/alerts/feeds/17039773334331169358/2758392895643864911',
    TRUE,
    480, -- 08:00 sáng
    CURRENT_DATE + TIME '01:00:00', -- 08:00 VN = 01:00 UTC
    CURRENT_TIMESTAMP,
    'Bộ KH & CN (Admin)'
),
(
    '6799b9b0-d1b4-4a89-81b4-d8718dd7cc6b',
    'Tỷ trọng doanh thu thương mại điện tử',
    'https://www.google.com/alerts/feeds/17039773334331169358/6061840220872706819',
    TRUE,
    480, -- 08:00 sáng
    CURRENT_DATE + TIME '01:00:00',
    CURRENT_TIMESTAMP,
    'Bộ KH & CN (Admin)'
),
(
    '1fdab812-a0b6-498d-a342-2bcafb952f9c',
    'Tỷ trọng kinh tế số trong GDP',
    'https://www.google.com/alerts/feeds/17039773334331169358/10962589691048183334',
    TRUE,
    480, -- 08:00 sáng
    CURRENT_DATE + TIME '01:00:00',
    CURRENT_TIMESTAMP,
    'Bộ KH & CN (Admin)'
)
ON CONFLICT ("FeedUrl") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Enabled" = EXCLUDED."Enabled",
    "ScanMinute" = EXCLUDED."ScanMinute",
    "UpdatedBy" = EXCLUDED."UpdatedBy",
    "DeletedAt" = NULL;

COMMIT;
