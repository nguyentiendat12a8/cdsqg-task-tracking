import psycopg2
import sys

sys.stdout.reconfigure(encoding='utf-8')

try:
    conn = psycopg2.connect("host=localhost port=5433 dbname=cdsqg_db user=postgres password=postgres")
    cur = conn.cursor()
    
    cur.execute('SELECT "Id", "Code", "Name", "Type", "ParentId" FROM "Agencies"')
    agencies = cur.fetchall()
    ag_map = {a[0]: a for a in agencies}
    print("=== Agencies List ===")
    for a in agencies:
        print(f"Agency [{a[1]}] {a[2]} | Type: {a[3]} | ParentId: {a[4]}")
        
    print("\n=== Tasks Led by Non-Ministry or Enterprise ===")
    cur.execute('SELECT "Id", "Code", "Title", "IsGeneralTask", "LeadAgencyId", "AssignedAgencyId", "CoordinatingAgencyIds" FROM "GoalTaskItems"')
    items = cur.fetchall()
    for item in items:
        itemId, code, title, isGeneral, leadId, assignedId, coordIds = item
        lead = ag_map.get(leadId)
        leadCode = lead[1] if lead else ""
        leadName = lead[2] if lead else "Unknown"
        leadType = lead[3] if lead else ""
        leadParent = lead[4] if lead else None
        
        parentName = ag_map[leadParent][2] if (leadParent and leadParent in ag_map) else "None"
        
        print(f"\nTask [{code}]: {title[:60]}...")
        print(f"  -> IsGeneralTask: {isGeneral}")
        print(f"  -> LeadAgency: [{leadCode}] {leadName} (Type: {leadType}, Parent: {parentName})")
        print(f"  -> AssignedAgencyId: {assignedId}")
        print(f"  -> CoordinatingAgencyIds: {coordIds}")
            
except Exception as e:
    print("Error:", e)
