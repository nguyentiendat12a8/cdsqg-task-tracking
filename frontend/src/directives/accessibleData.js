export const accessibleData = {
  mounted(el) {
    const enhance = () => {
      for (const table of el.querySelectorAll('table')) {
        if (!table.hasAttribute('aria-label') && !table.querySelector('caption')) table.setAttribute('aria-label', 'Bảng dữ liệu nghiệp vụ');
        for (const th of table.querySelectorAll('thead th')) if (!th.hasAttribute('scope')) th.setAttribute('scope','col');
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
    enhance();
    const observer = new MutationObserver(enhance);
    observer.observe(el,{childList:true,subtree:true});
    el.__dataCleanup = () => observer.disconnect();
  },
  unmounted(el) {el.__dataCleanup?.();}
};
