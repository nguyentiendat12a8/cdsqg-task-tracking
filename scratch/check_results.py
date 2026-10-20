import sys
import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT "Status", "Error" FROM "AiInboxItems" WHERE "Id" = %s;', ('5a60f7bd-aa6c-4e26-a2b7-b20171ce2f54',))
row = cur.fetchone()
print("Item Status:", row[0])
if row[1]:
    sys.stdout.buffer.write(f"Error: {row[1]}\n".encode('utf-8'))

cur.execute('SELECT "MetricKey", "DataYear", "Value", "Unit", "Quote", "Page", "Status" FROM "AiDataSuggestions" ORDER BY "CreatedAt" DESC LIMIT 5;')
suggs = cur.fetchall()
print(f"Total suggestions found: {len(suggs)}")
for s in suggs:
    print("-" * 30)
    print("MetricKey:", s[0])
    print("Year:", s[1], "Value:", s[2], "Unit:", s[3], "Page:", s[5], "Status:", s[6])
    sys.stdout.buffer.write(f"Quote: {s[4]}\n".encode('utf-8'))
conn.close()
