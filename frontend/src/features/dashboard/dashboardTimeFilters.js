import { currentReportingYear } from '../../config/reporting';

export function defaultDashboardTimeFilters() {
  const current = currentReportingYear();
  return { reportFromYear: current, reportToYear: current, executionFrom: '', executionTo: '', isOngoingOnly: false };
}
