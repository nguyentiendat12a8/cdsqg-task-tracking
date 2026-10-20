import sys
import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT "Code", "Title" FROM "GoalTaskItems" WHERE "Id" = %s;', ('6b635a7b-7eb6-4085-a5f8-de2a7d3f7626',))
row = cur.fetchone()
if row:
    sys.stdout.buffer.write(f"Goal: {row[0]} - {row[1]}\n".encode('utf-8'))
else:
    print("Not found")
conn.close()
