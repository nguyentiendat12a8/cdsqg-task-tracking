import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT "GoalCode", "ExpectedUnit", "KeywordsVi", "KeywordsEn" FROM "AiGoalKeywords";')
rows = cur.fetchall()
print(f"Total rows: {len(rows)}")
for r in rows:
    print(r[0], r[1])
conn.close()
