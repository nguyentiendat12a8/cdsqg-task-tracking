import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT "GoalCode", "ExpectedUnit", length("KeywordsVi"), length("KeywordsEn"), length("ExtractionPrompt") FROM "AiGoalKeywords" ORDER BY "GoalCode";')
rows = cur.fetchall()
print(f"Total rows in AiGoalKeywords: {len(rows)}")
for r in rows:
    print(f"{r[0]}: Unit={r[1]}, ViLen={r[2]}, EnLen={r[3]}, PromptLen={r[4]}")
conn.close()
