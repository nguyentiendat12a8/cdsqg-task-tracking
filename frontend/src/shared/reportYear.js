import { ref } from 'vue';
import { availableViewingYears, currentReportingYear } from '../config/reporting';

export const reportYear = ref(currentReportingYear());
export const reportYearOptions = availableViewingYears().map(year => ({ value: year, label: `Năm ${year}` }));
