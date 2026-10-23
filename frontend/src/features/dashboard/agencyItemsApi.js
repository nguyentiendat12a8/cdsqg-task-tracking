import {fetchWithAuth} from '../../services/auth';
import {getApiUrl} from '../../config/api';
export async function fetchDashboardAgencyItems(query,{signal}={}) {
  const response=await fetchWithAuth(getApiUrl('/api/dashboard/agency-items?'+query),{signal});
  if(!response.ok)throw new Error('Không tải được công việc của đơn vị.');
  const data=await response.json();
  if(Array.isArray(data))return data;
  if(data && Array.isArray(data.items))return data;
  throw new Error('Danh sách công việc không hợp lệ.');
}
