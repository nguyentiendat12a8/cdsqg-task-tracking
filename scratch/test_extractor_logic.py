import psycopg2
import json
import re

conn = psycopg2.connect(host='localhost', port=5433, dbname='cdsqg_db', user='postgres', password='postgres')
cur = conn.cursor()
cur.execute('SELECT "PagesJson" FROM "AiSourceDocuments" WHERE "Id" = %s', ('fea09322-a6fd-4611-89ef-abb083221f87',))
pages = json.loads(cur.fetchone()[0])
conn.close()

def normalize(text):
    return re.sub(r'\s+', ' ', text).strip()

quote = "Viet Nam 71 0.7709 0.8260 0.8143 0.6725"
norm_quote = normalize(quote)

found_page = None
for p in pages:
    if norm_quote in normalize(p['Text']):
        found_page = p['Number']
        break

print(f"Matched page: {found_page}")
assert found_page == 137
print("Success!")
