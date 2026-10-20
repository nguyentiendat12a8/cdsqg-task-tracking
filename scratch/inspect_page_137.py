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

p137 = [p for p in pages if p['Number'] == 137][0]['Text']
p204 = [p for p in pages if p['Number'] == 204][0]['Text']

q1 = "Viet Nam* V1 71 South-eastern Asia 0.7081 0.7267 0.8780 0.7709 0.6787"
print("q1 in p137:", normalize(q1) in normalize(p137))

q2 = "Viet Nam 71"
print("q2 in p204:", normalize(q2) in normalize(p204))
