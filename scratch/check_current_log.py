import psycopg2
import sys

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('''
    SELECT "Id", "GoalTaskId", "PeriodYear", "QuantitativeValue", "SummaryNotes", "AttachmentFileUrls", "ApprovedAt"
    FROM "ProgressLogs"
    WHERE "SummaryNotes" LIKE '%Admin kiểm chứng%'
    ORDER BY "Id" DESC LIMIT 5;
''')
rows = cur.fetchall()
print(f"Found {len(rows)} logs")
for r in rows:
    print("ID:", r[0])
    print("GoalTaskId:", r[1])
    print("Year:", r[2], "Value:", r[3])
    print("Files:", r[5])
    print("ApprovedAt:", r[6])
    sys.stdout.buffer.write(f"SummaryNotes: {r[4]}\n".encode('utf-8'))
conn.close()
