import { watch } from 'vue';

function agencyNames(ids, agencies) {
  return ids.map(id => agencies.find(agency => agency.id === id)?.name).filter(Boolean).join('; ');
}

export function initializeAgencyDisplayNames(form, agencies) {
  form.leadAgencyDisplayText ||= agencyNames([form.leadAgencyId], agencies);
  form.coordinatingAgenciesDisplayText ||= agencyNames(form.coordinatingAgencyIds || [], agencies);
}

export function useAgencyDisplayNames(form, agencies) {
  // Loading a record preserves its saved custom names. Only user selection changes replace them.
  watch(() => [form.value, form.value.leadAgencyId], ([current, id], [previous, previousId]) => {
    if (current === previous && id !== previousId) {
      current.leadAgencyDisplayText = agencyNames([id], agencies.value);
    }
  }, { flush: 'sync' });
  watch(() => [form.value, ...(form.value.coordinatingAgencyIds || [])], ([current, ...ids], [previous, ...previousIds]) => {
    if (current === previous && (ids.length !== previousIds.length || ids.some((id, index) => id !== previousIds[index]))) {
      current.coordinatingAgenciesDisplayText = agencyNames(ids, agencies.value);
    }
  }, { flush: 'sync' });
}
