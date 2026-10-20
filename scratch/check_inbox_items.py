import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT "Id", "Title", "Status", "MetricKeysJson", "Error" FROM "AiInboxItems" ORDER BY "ReceivedAt" DESC LIMIT 5;')
for row in cur.fetchall():
    print("ID:", row[0])
    print("Title:", row[1])
    print("Status:", row[2])
    print("MetricKeys:", row[3])
    print("Error:", row[4])
    print("-" * 40)
conn.close()
