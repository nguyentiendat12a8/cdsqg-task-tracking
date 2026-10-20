import sys
import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('''
    SELECT g."Code", g."Title", u."Name", u."Code", g."EvaluationType", g."IsGeneralTask"
    FROM "GoalTaskItems" g
    LEFT JOIN "Units" u ON g."UnitId" = u."Id"
    WHERE g."ItemType" = 'Goal'
    ORDER BY g."Code";
''')
rows = cur.fetchall()
print(f"Total goals: {len(rows)}")
for r in rows:
    sys.stdout.buffer.write(f"{r[0]} | {r[2]} ({r[3]}) | Eval: {r[4]} | General: {r[5]} | Title: {r[1]}\n".encode('utf-8'))
conn.close()
