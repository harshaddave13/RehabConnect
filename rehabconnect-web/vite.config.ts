
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/fhir': {
        target: 'https://localhost:7102',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
