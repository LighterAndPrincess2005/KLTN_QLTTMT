import { defineConfig } from 'vite';

export default defineConfig({
  server: {
    watch: {
      // .NET builds replace/lock executables; they are not web source files.
      ignored: ['**/backend/**', '**/LhlArt/**', '**/bin/**', '**/obj/**'],
    },
    proxy: {
      '/api': { target: 'http://127.0.0.1:5080', changeOrigin: true },
    },
  },
});
