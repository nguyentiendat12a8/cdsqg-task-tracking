import psycopg2
import json
import re

conn = psycopg2.connect(host='localhost', port=5433, dbname='cdsqg_db', user='postgres', password='postgres')
cur = conn.cursor()
cur.execute('SELECT "PagesJson" FROM "AiSourceDocuments" WHERE "Id" = %s', ('fea09322-a6fd-4611-89ef-abb083221f87',))
pages = json.loads(cur.fetchone()[0])
print(f'Total pages in document: {len(pages)}')

entity_keywords = ['viet nam', 'vietnam', 'việt nam']
topic_keywords_vi = ['chính phủ điện tử', 'chính phủ số', 'thứ hạng', 'xếp hạng']
topic_keywords_en = ['e-government', 'egdi', 'ranking', 'rank', 'annex', 'table']

all_keywords = entity_keywords + topic_keywords_vi + topic_keywords_en

def normalize(text):
    return re.sub(r'\s+', ' ', text).lower()

# Score each page
scored_pages = []
for p in pages:
    norm = normalize(p.get('Text', ''))
    has_entity = any(k in norm for k in entity_keywords)
    topic_matches = sum(1 for k in (topic_keywords_vi + topic_keywords_en) if k in norm)
    table_bonus = 10 if ('egdi rank' in norm or 'egdi ranking' in norm or 'annex' in norm) else 0
    score = (20 if has_entity else 0) + (topic_matches * 5) + table_bonus
    if has_entity or topic_matches >= 2:
        scored_pages.append((p.get('Number'), score, has_entity))

scored_pages.sort(key=lambda x: -x[1])
print(f'Hit pages: {scored_pages[:10]}')

# Take top hit pages with entity
top_hits = [sp[0] for sp in scored_pages if sp[2]][:5]
print(f'Top entity hits: {top_hits}')

# Expand each hit by ±3 pages
ranges = []
for h in top_hits:
    start = max(1, h - 3)
    end = min(len(pages), h + 3)
    ranges.append((start, end))

ranges.sort()
print(f'Raw ±3 ranges: {ranges}')

# Merge overlapping ranges
merged = []
for r in ranges:
    if not merged:
        merged.append(r)
    else:
        prev_start, prev_end = merged[-1]
        if r[0] <= prev_end + 1:
            merged[-1] = (prev_start, max(prev_end, r[1]))
        else:
            merged.append(r)

print(f'Merged clusters (±3 pages): {merged}')
for start, end in merged:
    cluster_pages = [p for p in pages if start <= p.get('Number') <= end]
    char_len = sum(len(p.get('Text', '')) for p in cluster_pages)
    print(f'Cluster {start}..{end}: {len(cluster_pages)} pages, {char_len} chars')
    # Check if target rank 71 is in this cluster
    combined = ' '.join(p.get('Text', '') for p in cluster_pages)
    if '71' in combined and ('viet nam' in combined.lower() or 'vietnam' in combined.lower()):
        print(f'  >>> FOUND VIET NAM WITH 71 in cluster {start}..{end}!')
