import psycopg2
import json
import re

conn = psycopg2.connect(host='localhost', port=5433, dbname='cdsqg_db', user='postgres', password='postgres')
cur = conn.cursor()
cur.execute('SELECT "PagesJson" FROM "AiSourceDocuments" WHERE "Id" = %s', ('fea09322-a6fd-4611-89ef-abb083221f87',))
pages = json.loads(cur.fetchone()[0])

keywords = ['viet nam', 'vietnam', 'việt nam']
candidates = []
for p in pages:
    txt = p.get('Text', '')
    for offset in range(0, len(txt), 9000):
        chunk = txt[offset:offset+min(12000, len(txt)-offset)]
        clean = re.sub(r'\s+', ' ', chunk).lower()
        if any(k in clean for k in keywords):
            candidates.append((p.get('Number'), chunk))

print(f'Total candidates with Viet Nam: {len(candidates)}')
def score(chunk):
    text = re.sub(r'\s+', ' ', chunk).lower()
    return (10 if re.search(r'egdi\s+(rank|ranking)', text) else 0) + (2 if 'annex' in text else 0)

scored = [(score(c[1]), c[0]) for c in candidates]
scored.sort(key=lambda x: (-x[0], x[1]))
print(f'Top scored chunks for EGDI_RANK: {scored[:5]}')
for s, num in scored[:3]:
    matching = [c[1] for c in candidates if c[0] == num][0]
    print(f'--- Page {num} (Score {s}) snippet ---')
    print(matching[:300].replace('\r', ' ').replace('\n', ' '))
