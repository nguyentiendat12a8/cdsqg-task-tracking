import { ref } from 'vue';
import { availableReportingYears, currentReportingYear } from '../config/reporting';

export const reportYear = ref(currentReportingYear());
export const reportYearOptions = availableReportingYears().map(year => ({ value: year, label: `Năm ${year}` }));
