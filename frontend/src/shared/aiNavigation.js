export const AI_NAVIGATION = [
 {route:'ai-data',key:'uploads',label:'Nạp file trực tiếp'},
 {route:'ai-alerts',key:'internet',label:'Dữ liệu từ internet'},
];
export const AI_MENU=[...AI_NAVIGATION,{route:'ai-settings',label:'Cấu hình AI'}];
export const aiMenuRoute=route=>route==='ai-review'?'ai-alerts':route;
export const isAiRoute=route=>AI_MENU.some(item=>item.route===aiMenuRoute(route));
export const isAiDataRoute=route=>AI_NAVIGATION.some(item=>item.route===aiMenuRoute(route));
export const aiSectionFor=route=>AI_NAVIGATION.find(item=>item.route===aiMenuRoute(route))||AI_NAVIGATION[0];
