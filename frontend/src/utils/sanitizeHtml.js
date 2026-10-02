// Rebuild rich text with a small allowlist; never copy executable attributes,
// styles, embedded media, SVG, forms, or custom elements from stored content.
const allowedTags = new Set(['P', 'BR', 'DIV', 'SPAN', 'B', 'STRONG', 'I', 'EM', 'U', 'S',
  'UL', 'OL', 'LI', 'BLOCKQUOTE', 'H1', 'H2', 'H3', 'H4', 'TABLE', 'THEAD', 'TBODY',
  'TR', 'TH', 'TD', 'A', 'PRE', 'CODE', 'HR']);
const blockedTags = new Set(['SCRIPT', 'STYLE', 'IFRAME', 'OBJECT', 'EMBED', 'SVG', 'MATH',
  'TEMPLATE', 'FORM', 'INPUT', 'BUTTON', 'META', 'LINK', 'BASE']);

export function sanitizeHtml(value) {
  const source = new DOMParser().parseFromString(String(value ?? ''), 'text/html');
  const output = document.createElement('div');
  function copy(node, parent) {
    if (node.nodeType === 3) { parent.appendChild(document.createTextNode(node.textContent)); return; }
    if (node.nodeType !== 1 || blockedTags.has(node.tagName)) return;
    let target = parent;
    if (allowedTags.has(node.tagName)) {
      target = document.createElement(node.tagName.toLowerCase());
      if (node.tagName === 'A') {
        try {
          const href = node.getAttribute('href');
          const url = new URL(href || '', window.location.href);
          if (href && ['http:', 'https:', 'mailto:', 'tel:'].includes(url.protocol)) {
            target.setAttribute('href', url.href);
            target.setAttribute('rel', 'noopener noreferrer');
          }
        } catch { /* Invalid links remain plain text. */ }
      }
      parent.appendChild(target);
    }
    for (const child of node.childNodes) copy(child, target);
  }
  for (const node of source.body.childNodes) copy(node, output);
  return output.innerHTML;
}
