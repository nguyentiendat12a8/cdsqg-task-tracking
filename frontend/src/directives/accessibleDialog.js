import { accessibleData } from './accessibleData';
const stack = [];
const focusable = 'button:not(:disabled), input:not(:disabled), select:not(:disabled), textarea:not(:disabled), a[href], [tabindex="0"]';
export const accessibleDialog = {
  mounted(el, binding) {
    const previous = document.activeElement;
    const panel = el.firstElementChild || el;
    accessibleData.mounted(panel);
    panel.setAttribute('role', 'dialog');
    panel.setAttribute('aria-modal', 'true');
    panel.setAttribute('tabindex', '-1');
    const heading = panel.querySelector('h1,h2,h3,h4');
    if (heading) panel.setAttribute('aria-label', heading.textContent.trim());
    else panel.setAttribute('aria-label', 'Chi tiết và thao tác');
    const entry = {el, previous};
    stack.push(entry);
    const items = () => [...panel.querySelectorAll(focusable)].filter(node => node.getClientRects().length);
    const key = event => {
      if (stack.at(-1) !== entry) return;
      if (document.querySelector('[role="alertdialog"]')) return;
      if (event.key === 'Escape') { event.stopPropagation(); binding.value?.(); }
      if (event.key === 'Tab') {
        // Dropdowns teleported from the dialog manage their own focus.
        if (!panel.contains(document.activeElement) && document.activeElement.closest('[role="listbox"],.v-popper__popper')) return;
        const list = items(), first = list[0], last = list.at(-1);
        if (!first) {event.preventDefault(); panel.focus();}
        else if (event.shiftKey && (document.activeElement === first || !panel.contains(document.activeElement))) {event.preventDefault(); last.focus();}
        else if (!event.shiftKey && (document.activeElement === last || !panel.contains(document.activeElement))) {event.preventDefault(); first.focus();}
      }
    };
    document.addEventListener('keydown', key);
    requestAnimationFrame(() => {if (el.isConnected) (items()[0] || panel).focus();});
    el.__dialogCleanup = () => {
      document.removeEventListener('keydown', key);
      accessibleData.unmounted(panel);
      stack.splice(stack.indexOf(entry),1);
      if (previous?.isConnected) previous.focus();
    };
  },
  unmounted(el) {el.__dialogCleanup?.();}
};
