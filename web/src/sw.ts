/// <reference lib="webworker" />
import { cleanupOutdatedCaches, createHandlerBoundToURL, precacheAndRoute } from 'workbox-precaching'
import { NavigationRoute, registerRoute } from 'workbox-routing'

declare let self: ServiceWorkerGlobalScope

self.skipWaiting()
self.addEventListener('activate', (event) => event.waitUntil(self.clients.claim()))

// App shell is precached; /api is always fetched from the network (the MVP needs the backend running).
precacheAndRoute(self.__WB_MANIFEST)
cleanupOutdatedCaches()
registerRoute(new NavigationRoute(createHandlerBoundToURL('index.html'), { denylist: [/^\/api\//] }))

interface PushPayload {
  title?: string
  body?: string
  url?: string
}

self.addEventListener('push', (event) => {
  const data: PushPayload = event.data?.json() ?? {}
  event.waitUntil(
    self.registration.showNotification(data.title ?? 'EatUp', {
      body: data.body,
      icon: '/icon-192.png',
      badge: '/icon-192.png',
      data: { url: data.url ?? '/' },
    }),
  )
})

self.addEventListener('notificationclick', (event) => {
  event.notification.close()
  const url: string = event.notification.data?.url ?? '/'
  event.waitUntil(
    (async () => {
      const windows = await self.clients.matchAll({ type: 'window', includeUncontrolled: true })
      // navigate() rejects on windows this worker doesn't control (e.g. after a hard reload), so fall back to a new window.
      const existing = windows[0]
      if (existing) {
        try {
          const focused = await existing.focus() // focus first, while we still have the click's user activation
          await focused.navigate(url)
          return
        } catch {
          // fall through and open a fresh window
        }
      }
      await self.clients.openWindow(url)
    })(),
  )
})
