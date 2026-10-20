import psycopg2
import os

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT "Id", "Title", "StorageKey", "MetricKeysJson" FROM "AiInboxItems" WHERE "Id" = %s;', ('5a60f7bd-aa6c-4e26-a2b7-b20171ce2f54',))
row = cur.fetchone()
print("Inbox Item:", row)
conn.close()
