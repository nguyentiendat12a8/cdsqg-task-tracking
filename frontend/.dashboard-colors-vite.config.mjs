import base from './vite.config.js';
export default {...base,cacheDir:'node_modules/.vite-dashboard-colors',server:{...base.server,host:'127.0.0.1',port:5206,watch:null,hmr:false}};
