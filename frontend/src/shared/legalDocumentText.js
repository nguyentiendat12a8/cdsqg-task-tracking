import { formatDate } from './formatters';

const clean = value => String(value ?? '').replace(/\s+/g, ' ').trim();

// Preserve the entered abstract rather than inventing the document's purpose.
export function formatLegalDocumentReference(document) {
  const type = clean(document.documentType);
  const code = clean(document.code).replace(/^số\s*:?\s*/i, '');
  const title = clean(document.title);
  const abstract = /^Luật\s/i.test(title) ? `ban hành ${title}` : title;
  return [type, code && `số ${code}`, document.issuedDate && `ngày ${formatDate(document.issuedDate)}`,
    clean(document.issuingAgencyName) && `của ${clean(document.issuingAgencyName)}`, abstract]
    .filter(Boolean).join(' ');
}
