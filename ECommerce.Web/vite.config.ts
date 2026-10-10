import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The API runs on the "http" launch profile; proxying /api avoids needing CORS on the API
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5065',
    },
  },
})
