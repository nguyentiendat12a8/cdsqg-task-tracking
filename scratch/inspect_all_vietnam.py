import psycopg2
import json

conn = psycopg2.connect(host='localhost', port=5433, dbname='cdsqg_db', user='postgres', password='postgres')
cur = conn.cursor()
cur.execute('SELECT "PagesJson" FROM "AiSourceDocuments" WHERE "Id" = %s', ('fea09322-a6fd-4611-89ef-abb083221f87',))
pages = json.loads(cur.fetchone()[0])
conn.close()

for p in pages:
    txt = p['Text']
    if 'Viet Nam' in txt or 'viet nam' in txt.lower():
        idx = txt.lower().find('viet nam')
        print(f"Page {p['Number']}: {repr(txt[max(0, idx-10):idx+60])}")
