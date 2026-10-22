import { ref, computed } from 'vue';

export const SUPPORTED_DOCUMENTS = [
  {
    id: '12660000-0000-0000-0000-000000001266',
    code: '1266',
    documentNumber: '1266/QĐ-TTg',
    name: 'Quyết định số 1266/QĐ-TTg',
    title: 'Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045',
    shortTitle: 'Chiến lược',
    tag: 'Chiến lược',
    signer: 'Thủ tướng Chính phủ',
    issueDate: '14/07/2026',
    startYear: 2026,
    endYear: 2030,
    timeHorizon: '2026 – 2030 (định hướng 2045)',
    goalsCount: 27,
    tasksCount: 82,
    badgeColor: 'blue',
    description: 'Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045 ban hành theo Quyết định số 1266/QĐ-TTg.'
  },
  {
    id: '26290000-0000-0000-0000-000000002629',
    code: '2629',
    documentNumber: '2629/QĐ-TTg',
    name: 'Quyết định số 2629/QĐ-TTg',
    title: 'Chương trình phát triển Chính phủ số',
    shortTitle: 'Chương trình',
    tag: 'Chương trình',
    signer: 'Thủ tướng Chính phủ',
    issueDate: '01/12/2025',
    startYear: 2025,
    endYear: 2030,
    timeHorizon: '2025 – 2030 (GĐ 2025–2027 & 2028–2030)',
    goalsCount: 22,
    tasksCount: 75,
    badgeColor: 'emerald',
    description: 'Chương trình phát triển Chính phủ số thay thế Quyết định số 942/QĐ-TTg ngày 15/06/2021.'
  }
];

const STORAGE_KEY = 'cdsqg_active_doc_code';

function getInitialDocumentCode() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved && SUPPORTED_DOCUMENTS.some(d => d.code === saved || d.id === saved)) {
      return saved;
    }
  } catch {}
  return '1266';
}

const activeDocCode = ref(getInitialDocumentCode());

export const activeDocument = computed(() => {
  return SUPPORTED_DOCUMENTS.find(d => d.code === activeDocCode.value || d.id === activeDocCode.value) || SUPPORTED_DOCUMENTS[0];
});

export const activeDocumentId = computed(() => activeDocument.value.id);
export const activeDocumentCode = computed(() => activeDocument.value.code);

export function switchDocument(codeOrId) {
  const found = SUPPORTED_DOCUMENTS.find(d => d.code === codeOrId || d.id === codeOrId);
  if (found) {
    activeDocCode.value = found.code;
    try {
      localStorage.setItem(STORAGE_KEY, found.code);
    } catch {}
  }
}
