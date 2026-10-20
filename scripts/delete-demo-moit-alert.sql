-- Script xóa nguồn RSS demo (Bộ Công Thương / site:moit.gov.vn) và toàn bộ bài viết, lịch sử thu thập liên quan

BEGIN;

-- 1. Xóa liên kết mục tiêu (nếu có đề xuất số liệu đã được bóc tách)
DELETE FROM "AiGoalDataLinks"
WHERE "SuggestionId" IN (
    SELECT s."Id" FROM "AiDataSuggestions" s
    JOIN "AiSourceDocuments" d ON s."DocumentId" = d."Id"
    JOIN "AiInboxItems" i ON d."InboxItemId" = i."Id"
    WHERE i."AlertFeedId" IN (
        SELECT "Id" FROM "AiAlertFeeds" WHERE "FeedUrl" LIKE '%10847235485381683809%' OR "Name" ILIKE '%Bộ Công Thương%'
    )
);

-- 2. Xóa các đề xuất số liệu bóc tách từ bài viết của nguồn này (nếu có)
DELETE FROM "AiDataSuggestions"
WHERE "DocumentId" IN (
    SELECT d."Id" FROM "AiSourceDocuments" d
    JOIN "AiInboxItems" i ON d."InboxItemId" = i."Id"
    WHERE i."AlertFeedId" IN (
        SELECT "Id" FROM "AiAlertFeeds" WHERE "FeedUrl" LIKE '%10847235485381683809%' OR "Name" ILIKE '%Bộ Công Thương%'
    )
);

-- 3. Xóa nội dung bài viết / tài liệu đã tải về (nếu có)
DELETE FROM "AiSourceDocuments"
WHERE "InboxItemId" IN (
    SELECT "Id" FROM "AiInboxItems"
    WHERE "AlertFeedId" IN (
        SELECT "Id" FROM "AiAlertFeeds" WHERE "FeedUrl" LIKE '%10847235485381683809%' OR "Name" ILIKE '%Bộ Công Thương%'
    )
);

-- 4. Xóa toàn bộ các bài viết / tiêu đề đã thu thập trong Inbox của nguồn này
DELETE FROM "AiInboxItems"
WHERE "AlertFeedId" IN (
    SELECT "Id" FROM "AiAlertFeeds" WHERE "FeedUrl" LIKE '%10847235485381683809%' OR "Name" ILIKE '%Bộ Công Thương%'
);

-- 5. Xóa lịch sử các lượt quét của nguồn này
DELETE FROM "AiAlertScans"
WHERE "FeedId" IN (
    SELECT "Id" FROM "AiAlertFeeds" WHERE "FeedUrl" LIKE '%10847235485381683809%' OR "Name" ILIKE '%Bộ Công Thương%'
);

-- 6. Xóa chính nguồn RSS demo
DELETE FROM "AiAlertFeeds"
WHERE "FeedUrl" LIKE '%10847235485381683809%' OR "Name" ILIKE '%Bộ Công Thương%';

COMMIT;
