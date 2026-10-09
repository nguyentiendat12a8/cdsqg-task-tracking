import { REPORTING_YEARS } from '../config/reporting';
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { requestJson } from '../services/apiClient';

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
    dynamicYears: [...REPORTING_YEARS],
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
      const response = await requestJson(`/api/planning/documents/${documentId}/grid`);
      if (response) {
        planningGrid.value = response;
      }
    } catch (err) {
      errorMessage.value = err.message;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateCustomBaseline(taskId, milestones) {
    isLoading.value = true;
    try {
      const response = await requestJson(`/api/planning/tasks/${taskId}/custom-baseline`, { method: 'POST', body: { milestones } });
      return response;
    } catch (err) {
      throw new Error(err.message || 'Lỗi cập nhật Custom Baseline.');
    } finally {
      isLoading.value = false;
    }
  }

  async function submitProgressUpdate(taskId, formData) {
    isLoading.value = true;
    try {
      const response = await requestJson(`/api/execution/tasks/${taskId}/progress`, { method: 'POST', body: formData });
      return response;
    } catch (err) {
      throw new Error(err.message || 'Lỗi khi gửi báo cáo tiến độ.');
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchDashboardMetrics(documentId) {
    isLoading.value = true;
    try {
      const params = { year: selectedYear.value };
      if (documentId) params.documentId = documentId;

      const response = await requestJson('/api/dashboard/metrics', { params });
      if (response) {
        dashboardMetrics.value.totalGoals = response.totalGoals;
        dashboardMetrics.value.totalTasks = response.totalTasks;
        dashboardMetrics.value.completionPercentage = response.overallQuantitativeCompletionPct;
        dashboardMetrics.value.remainingPercentage = response.overallRemainingPct;
        dashboardMetrics.value.trafficLights.greenCount = response.trafficLights.greenCount;
        dashboardMetrics.value.trafficLights.yellowCount = response.trafficLights.yellowCount;
        dashboardMetrics.value.trafficLights.redCount = response.trafficLights.redCount;
        dashboardMetrics.value.staleTasks = response.staleTasks || [];
        dashboardMetrics.value.agencyPerformance = response.agencyPerformance || [];
        dashboardMetrics.value.qualitativeDistribution = response.qualitativeDistribution || { NotStarted: 0, Drafting: 0, Reviewing: 0, Completed: 0 };
      }
    } catch (err) {
      errorMessage.value = err.message;
    } finally {
      isLoading.value = false;
    }
  }

  async function importLlmBootstrapPayload(payload) {
    isLoading.value = true;
    try {
      const response = await requestJson('/api/import/llm-bootstrap', { method: 'POST', body: payload });
      return response;
    } catch (err) {
      throw new Error(err.message || 'Lỗi khi đồng bộ dữ liệu LLM JSON vào PostgreSQL.');
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


