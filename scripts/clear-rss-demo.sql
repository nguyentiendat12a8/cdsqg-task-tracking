-- Script xóa sạch dữ liệu bài viết RSS đã quét để phục vụ demo
BEGIN;

-- 1. Xóa liên kết gợi ý AI với chỉ tiêu mục tiêu (nếu có bài từ RSS được bóc tách)
DELETE FROM "AiGoalDataLinks"
WHERE "SuggestionId" IN (
    SELECT s."Id" FROM "AiDataSuggestions" s
    JOIN "AiSourceDocuments" d ON s."DocumentId" = d."Id"
    JOIN "AiInboxItems" i ON d."InboxItemId" = i."Id"
    WHERE i."AlertFeedId" IS NOT NULL
);

-- 2. Xóa các đề xuất số liệu bóc tách từ bài viết RSS
DELETE FROM "AiDataSuggestions"
WHERE "DocumentId" IN (
    SELECT d."Id" FROM "AiSourceDocuments" d
    JOIN "AiInboxItems" i ON d."InboxItemId" = i."Id"
    WHERE i."AlertFeedId" IS NOT NULL
);

-- 3. Xóa nội dung bài viết / tài liệu đã tải từ RSS
DELETE FROM "AiSourceDocuments"
WHERE "InboxItemId" IN (
    SELECT "Id" FROM "AiInboxItems" WHERE "AlertFeedId" IS NOT NULL
);

-- 4. Xóa toàn bộ bài viết / tiêu đề RSS trong Hộp thư AI
DELETE FROM "AiInboxItems"
WHERE "AlertFeedId" IS NOT NULL;

-- 5. Xóa toàn bộ lịch sử các lượt quét RSS
DELETE FROM "AiAlertScans";

-- 6. Nếu muốn xóa luôn cả Nguồn RSS để demo thêm nguồn mới từ đầu, bỏ comment dòng dưới:
-- DELETE FROM "AiAlertFeeds";

COMMIT;
