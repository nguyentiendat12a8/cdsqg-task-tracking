import psycopg2

conn = psycopg2.connect(dbname='cdsqg_db', user='postgres', password='postgres', host='localhost', port=5433)
cur = conn.cursor()
cur.execute('SELECT "Enabled", "BaseUrl", "Model", length("ProtectedApiKey") FROM "AiProviderConfigurations";')
print("AI Config:", cur.fetchall())
conn.close()
