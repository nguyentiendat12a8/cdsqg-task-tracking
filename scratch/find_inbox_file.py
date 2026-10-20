import psycopg2
import os

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT "StorageKey", "Title" FROM "AiInboxItems" WHERE "Id" = %s;', ('5a60f7bd-aa6c-4e26-a2b7-b20171ce2f54',))
row = cur.fetchone()
print("StorageKey:", row[0], "Title:", row[1])

# Check where files are stored
uploads_dir = 'backend/uploads'
appdata = os.environ.get('LOCALAPPDATA', '')
print("Checking possible paths...")
for root, dirs, files in os.walk('.'):
    if row[0] in files:
        print("Found file at:", os.path.join(root, row[0]))
conn.close()
