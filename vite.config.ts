import { defineConfig } from 'vite';
export default defineConfig({server:{watch:{ignored:['**/backend/**']},proxy:{'/api':{target:'http://127.0.0.1:5080',changeOrigin:true}}}});
