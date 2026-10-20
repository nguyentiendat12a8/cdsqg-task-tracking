import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('''
    UPDATE "AiInboxItems" 
    SET "Status" = 'ReadQueued', "LeaseUntil" = NULL, "Error" = ''
    WHERE "Id" = '5a60f7bd-aa6c-4e26-a2b7-b20171ce2f54';
''')
conn.commit()
print("Updated rows:", cur.rowcount)
conn.close()
