import psycopg2
import json

conn = psycopg2.connect(host='localhost', port=5433, dbname='cdsqg_db', user='postgres', password='postgres')
cur = conn.cursor()
cur.execute('SELECT "PagesJson" FROM "AiSourceDocuments" WHERE "Id" = %s', ('fea09322-a6fd-4611-89ef-abb083221f87',))
pages = json.loads(cur.fetchone()[0])
conn.close()

p204 = [p for p in pages if p['Number'] == 204][0]['Text']
idx = p204.find('Viet Nam')
print("Slice around Viet Nam on p204:")
print(repr(p204[idx-20:idx+60]))
