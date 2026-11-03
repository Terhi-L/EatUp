import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    VitePWA({
      strategies: 'injectManifest',
      srcDir: 'src',
      filename: 'sw.ts',
      registerType: 'autoUpdate',
      injectRegister: false,
      includeAssets: ['favicon.svg', 'apple-touch-icon.png'],
      manifest: {
        name: 'EatUp',
        short_name: 'EatUp',
        description: 'Use up your food before it goes off.',
        start_url: '/',
        display: 'standalone',
        background_color: '#f7f4ff',
        theme_color: '#f7f4ff',
        icons: [
          { src: 'icon-192.png', sizes: '192x192', type: 'image/png' },
          { src: 'icon-512.png', sizes: '512x512', type: 'image/png' },
          { src: 'icon-512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
        ],
      },
      injectManifest: { globPatterns: ['**/*.{js,css,html,png,svg}'] },
      // Service worker (and so Web Push) also runs under `npm run dev`.
      devOptions: { enabled: true, type: 'module', navigateFallback: 'index.html' },
    }),
  ],
  server: {
    port: 5173,
    strictPort: true,
    // Lets the phone-testing tunnel in GETTING_STARTED.md through Vite's host check.
    allowedHosts: ['.trycloudflare.com'],
    proxy: { '/api': 'http://localhost:5080' },
  },
})
