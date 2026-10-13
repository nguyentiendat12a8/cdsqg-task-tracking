import json,math
from pathlib import Path
src=json.loads(Path('scratch/map-source/provinces34.geojson').read_text(encoding='utf-8-sig'))
assert len(src['features'])==34
# Source feature Ma=31 has a copied Lang Son label; its southern geometry is Dong Thap.
for f in src['features']:
 if str(f['properties']['Ma'])=='31': f['properties']['TinhThanh']='Đồng Tháp'
 if f['properties']['TinhThanh']=='TP. Hồ Chí Minh': f['properties']['TinhThanh']='Hồ Chí Minh'
assert len({f['properties']['TinhThanh'] for f in src['features']})==34
rings=[]
for f in src['features']:
 g=f['geometry']; polys=g['coordinates'] if g['type']=='MultiPolygon' else [g['coordinates']]
 for poly in polys:rings.extend(poly)
points=[p for r in rings for p in r]
lo=min(p[0] for p in points);hi=max(p[0] for p in points);bottom=min(p[1] for p in points);top=max(p[1] for p in points)
scale=680/(top-bottom);cos=math.cos(math.radians(17));width=(hi-lo)*cos*scale+32
# Douglas-Peucker preserves closed ring endpoints and enough detail for dashboard use.
def simplify(pts,tol=0.004):
 if len(pts)<=2:return pts
 a,b=pts[0],pts[-1];dx=b[0]-a[0];dy=b[1]-a[1];den=dx*dx+dy*dy
 def dist(p):
  t=max(0,min(1,((p[0]-a[0])*dx+(p[1]-a[1])*dy)/den)) if den else 0
  return (p[0]-a[0]-t*dx)**2+(p[1]-a[1]-t*dy)**2
 i=max(range(1,len(pts)-1),key=lambda i:dist(pts[i]))
 if dist(pts[i])<=tol*tol:return [a,b]
 return simplify(pts[:i+1],tol)[:-1]+simplify(pts[i:],tol)
features=[]
for f in src['features']:
 g=f['geometry'];polys=g['coordinates'] if g['type']=='MultiPolygon' else [g['coordinates']];paths=[]
 for poly in polys:
  for ring in poly:
   r=simplify(ring)
   if len(r)<4:r=ring
   paths.append('M'+'L'.join(f'{16+(p[0]-lo)*cos*scale:.1f},{16+(top-p[1])*scale:.1f}' for p in r)+'Z')
 features.append({'code':f['properties']['Ma'],'name':f['properties']['TinhThanh'],'path':''.join(paths)})
Path('frontend/src/features/dashboard/map/provinces.json').write_text(json.dumps({'viewBox':f'0 0 {width:.1f} 712','provinces':features},ensure_ascii=False,separators=(',',':')),encoding='utf-8')
print('34 province paths; bounds',lo,hi,bottom,top,'bytes',Path('frontend/src/features/dashboard/map/provinces.json').stat().st_size)
