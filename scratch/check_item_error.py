import sys
import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT "Error" FROM "AiInboxItems" WHERE "Id" = %s;', ('5a60f7bd-aa6c-4e26-a2b7-b20171ce2f54',))
row = cur.fetchone()
if row and row[0]:
    sys.stdout.buffer.write(f"Error: {row[0]}\n".encode('utf-8'))
else:
    print("No error")
conn.close()
