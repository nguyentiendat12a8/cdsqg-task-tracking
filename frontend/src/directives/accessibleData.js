export const accessibleData = {
  mounted(el) {
    const enhance = () => {
      for (const table of el.querySelectorAll('table')) {
        if (!table.hasAttribute('aria-label') && !table.querySelector('caption')) table.setAttribute('aria-label', 'Bảng dữ liệu nghiệp vụ');
        for (const th of table.querySelectorAll('thead th')) {
          if (!th.hasAttribute('scope')) th.setAttribute('scope', 'col');
          // Existing sortable headers retain their click workflow and table semantics.
          if (th.classList.contains('cursor-pointer') && !th.querySelector('button, a, input') && !th.hasAttribute('tabindex')) {
            th.tabIndex = 0;
            th.dataset.keyboardSort = 'true';
          }
        }
        let wrapper = table.parentElement;
        while (wrapper && wrapper !== el) {
          if (getComputedStyle(wrapper).overflowX === 'auto') {
            wrapper.tabIndex = 0;
            wrapper.setAttribute('role','region');
            wrapper.setAttribute('aria-label','Bảng dữ liệu, cuộn ngang bằng phím mũi tên');
            break;
          }
          wrapper = wrapper.parentElement;
        }
      }
    };
    const onKeydown = event => {
      const header = event.target.closest?.('th[data-keyboard-sort="true"]');
      if (!header || !el.contains(header) || !['Enter', ' '].includes(event.key)) return;
      event.preventDefault();
      header.click();
    };
    el.addEventListener('keydown', onKeydown);
    enhance();
    const observer = new MutationObserver(enhance);
    observer.observe(el,{childList:true,subtree:true});
    el.__dataCleanup = () => { observer.disconnect(); el.removeEventListener('keydown', onKeydown); };
  },
  unmounted(el) {el.__dataCleanup?.();}
};
