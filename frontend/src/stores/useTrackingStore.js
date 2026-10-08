import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axiosLibrary from 'axios';
import { getApiUrl } from '../config/api';
import { getAuthHeaders, logout } from '../services/auth';

const API_BASE_URL = getApiUrl('/api');
const axios = axiosLibrary.create();
axios.interceptors.request.use(config => {
  const url = new URL(config.url, window.location.href);
  if (url.origin === new URL(API_BASE_URL).origin && url.pathname.startsWith('/api/')) {
    Object.assign(config.headers, getAuthHeaders());
  }
  return config;
});
axios.interceptors.response.use(response => response, error => {
  if (error.response?.status === 401) logout();
  else if (error.response?.status === 403) window.dispatchEvent(new CustomEvent('api-feedback', {detail:'Bạn không có quyền thực hiện thao tác này. Vui lòng liên hệ quản trị viên nếu cần được cấp quyền.'}));
  else if (!error.response || error.response.status >= 500) window.dispatchEvent(new CustomEvent('api-feedback', {detail:'Không thể kết nối hoặc xử lý yêu cầu. Vui lòng thử lại; chưa thể xác nhận thao tác thành công.'}));
  else if (error.response.status === 429) window.dispatchEvent(new CustomEvent('api-feedback', {detail:'Bạn thao tác quá nhiều lần. Vui lòng chờ rồi thử lại.'}));
  return Promise.reject(error);
});

export const useTrackingStore = defineStore('tracking', () => {
  // State
  const selectedDocumentId = ref(null);
  const selectedYear = ref(2026);
  const isLoading = ref(false);
  const errorMessage = ref(null);

  // Planning Grid State (PURGED OF FAKE MOCK DATA)
  const planningGrid = ref({
    documentId: '',
    documentNumber: '',
    documentName: '',
    startYear: 2026,
    endYear: 2030,
    dynamicYears: [2026, 2027, 2028, 2029, 2030],
    items: []
  });

  // Dashboard Metrics State
  const dashboardMetrics = ref({
    totalGoals: 0,
    totalTasks: 0,
    completionPercentage: 0,
    remainingPercentage: 100,
    trafficLights: {
      greenCount: 0,
      yellowCount: 0,
      redCount: 0
    },
    staleTasks: [],
    agencyPerformance: [],
    qualitativeDistribution: {
      NotStarted: 0,
      Drafting: 0,
      Reviewing: 0,
      Completed: 0
    }
  });

  // Computed
  const redAlertTasksCount = computed(() => dashboardMetrics.value.trafficLights.redCount);
  const staleTasksCount = computed(() => dashboardMetrics.value.staleTasks.length);

  // Actions
  async function fetchPlanningGrid(documentId) {
    if (!documentId) return;
    isLoading.value = true;
    errorMessage.value = null;
    try {
      const response = await axios.get(`${API_BASE_URL}/planning/documents/${documentId}/grid`);
      if (response.data) {
        planningGrid.value = response.data;
      }
    } catch (err) {
      console.warn('Backend connection failed when fetching planning grid:', err);
    } finally {
      isLoading.value = false;
    }
  }

  async function updateCustomBaseline(taskId, milestones) {
    isLoading.value = true;
    try {
      const response = await axios.post(`${API_BASE_URL}/planning/tasks/${taskId}/custom-baseline`, { milestones });
      return response.data;
    } catch (err) {
      throw new Error(err.response?.data?.error || 'Lỗi cập nhật Custom Baseline.');
    } finally {
      isLoading.value = false;
    }
  }

  async function submitProgressUpdate(taskId, formData) {
    isLoading.value = true;
    try {
      const response = await axios.post(`${API_BASE_URL}/execution/tasks/${taskId}/progress`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });
      return response.data;
    } catch (err) {
      throw new Error(err.response?.data?.error || 'Lỗi khi gửi báo cáo tiến độ.');
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchDashboardMetrics(documentId) {
    isLoading.value = true;
    try {
      const params = { year: selectedYear.value };
      if (documentId) params.documentId = documentId;

      const response = await axios.get(`${API_BASE_URL}/dashboard/metrics`, { params });
      if (response.data) {
        dashboardMetrics.value.totalGoals = response.data.totalGoals;
        dashboardMetrics.value.totalTasks = response.data.totalTasks;
        dashboardMetrics.value.completionPercentage = response.data.overallQuantitativeCompletionPct;
        dashboardMetrics.value.remainingPercentage = response.data.overallRemainingPct;
        dashboardMetrics.value.trafficLights.greenCount = response.data.trafficLights.greenCount;
        dashboardMetrics.value.trafficLights.yellowCount = response.data.trafficLights.yellowCount;
        dashboardMetrics.value.trafficLights.redCount = response.data.trafficLights.redCount;
        dashboardMetrics.value.staleTasks = response.data.staleTasks || [];
        dashboardMetrics.value.agencyPerformance = response.data.agencyPerformance || [];
        dashboardMetrics.value.qualitativeDistribution = response.data.qualitativeDistribution || { NotStarted: 0, Drafting: 0, Reviewing: 0, Completed: 0 };
      }
    } catch (err) {
      console.warn('Failed fetching dashboard metrics from API:', err);
    } finally {
      isLoading.value = false;
    }
  }

  async function importLlmBootstrapPayload(payload) {
    isLoading.value = true;
    try {
      const response = await axios.post(`${API_BASE_URL}/import/llm-bootstrap`, payload);
      return response.data;
    } catch (err) {
      throw new Error(err.response?.data?.error || 'Lỗi khi đồng bộ dữ liệu LLM JSON vào PostgreSQL.');
    } finally {
      isLoading.value = false;
    }
  }

  return {
    selectedDocumentId,
    selectedYear,
    isLoading,
    errorMessage,
    planningGrid,
    dashboardMetrics,
    redAlertTasksCount,
    staleTasksCount,
    fetchPlanningGrid,
    updateCustomBaseline,
    submitProgressUpdate,
    fetchDashboardMetrics,
    importLlmBootstrapPayload
  };
});
