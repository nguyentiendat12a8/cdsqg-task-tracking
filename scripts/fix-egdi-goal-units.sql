-- PostgreSQL: hai muc tieu EGDI dung don vi So luong (SL).
BEGIN;
DO $$
BEGIN
 IF (SELECT count(*) FROM "Units" WHERE "Code"='SL')<>1 THEN
  RAISE EXCEPTION 'Can dung mot don vi SL trong danh muc Units';
 END IF;
 UPDATE "GoalTaskItems"
 SET "UnitId"=(SELECT "Id" FROM "Units" WHERE "Code"='SL')
 WHERE "ItemType"='Goal' AND "Code" IN ('MT-12','MT-25');
END $$;
SELECT g."Code",u."Code" AS "UnitCode",u."Name" AS "UnitName"
FROM "GoalTaskItems" g JOIN "Units" u ON u."Id"=g."UnitId"
WHERE g."ItemType"='Goal' AND g."Code" IN ('MT-12','MT-25');
COMMIT;
