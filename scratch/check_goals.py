import sys
import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT "Code", "Title" FROM "GoalTaskItems" WHERE "ItemType" = \'Goal\' ORDER BY "Code" LIMIT 40;')
for row in cur.fetchall():
    sys.stdout.buffer.write(f"{row[0]}: {row[1]}\n".encode('utf-8'))
conn.close()
