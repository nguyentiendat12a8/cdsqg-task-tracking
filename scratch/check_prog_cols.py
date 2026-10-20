import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT column_name FROM information_schema.columns WHERE table_name = \'ProgressLogs\';')
print([r[0] for r in cur.fetchall()])
conn.close()
