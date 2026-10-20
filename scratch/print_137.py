import psycopg2
import json

conn = psycopg2.connect(host='localhost', port=5433, dbname='cdsqg_db', user='postgres', password='postgres')
cur = conn.cursor()
cur.execute('SELECT "PagesJson" FROM "AiSourceDocuments" WHERE "Id" = %s', ('fea09322-a6fd-4611-89ef-abb083221f87',))
pages = json.loads(cur.fetchone()[0])
conn.close()

p137 = [p for p in pages if p['Number'] == 137][0]['Text']
print(p137[1000:])
