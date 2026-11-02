import { api } from '../../api'

export type PushState = 'unsupported' | 'needs-install' | 'denied' | 'enabled' | 'disabled'

const isIos = () =>
  /iPad|iPhone|iPod/.test(navigator.userAgent) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1)

const isStandalone = () =>
  window.matchMedia('(display-mode: standalone)').matches || (navigator as { standalone?: boolean }).standalone === true

const isSupported = () => 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window

export async function getPushState(): Promise<PushState> {
  // iOS only exposes Web Push to apps added to the home screen.
  if (isIos() && !isStandalone()) return 'needs-install'
  if (!isSupported()) return 'unsupported'
  if (Notification.permission === 'denied') return 'denied'
  const registration = await navigator.serviceWorker.ready
  const subscription = await registration.pushManager.getSubscription()
  return subscription && Notification.permission === 'granted' ? 'enabled' : 'disabled'
}

/** Must be called from a user gesture (button tap), or browsers will block the permission prompt. */
export async function enablePush(): Promise<PushState> {
  const permission = await Notification.requestPermission()
  if (permission !== 'granted') return permission === 'denied' ? 'denied' : 'disabled'

  const registration = await navigator.serviceWorker.ready
  // Drop any old subscription: it may be tied to a different VAPID key (e.g. after deleting backend data).
  await (await registration.pushManager.getSubscription())?.unsubscribe()
  const subscription = await registration.pushManager.subscribe({
    userVisibleOnly: true,
    applicationServerKey: urlBase64ToUint8Array(await api.vapidPublicKey()),
  })
  await api.subscribe(subscription.toJSON())
  return 'enabled'
}

export async function disablePush(): Promise<PushState> {
  const registration = await navigator.serviceWorker.ready
  const subscription = await registration.pushManager.getSubscription()
  if (subscription) {
    await api.unsubscribe(subscription.endpoint)
    await subscription.unsubscribe()
  }
  return 'disabled'
}

/** Re-send the current subscription on app start; browsers can rotate it, and the backend may have been reset. */
export async function syncPush(): Promise<void> {
  if (!isSupported() || Notification.permission !== 'granted') return
  const registration = await navigator.serviceWorker.ready
  const subscription = await registration.pushManager.getSubscription()
  if (subscription) await api.subscribe(subscription.toJSON())
}

export function urlBase64ToUint8Array(base64: string): Uint8Array<ArrayBuffer> {
  const padded = (base64 + '='.repeat((4 - (base64.length % 4)) % 4)).replace(/-/g, '+').replace(/_/g, '/')
  const raw = atob(padded)
  const bytes = new Uint8Array(new ArrayBuffer(raw.length))
  for (let i = 0; i < raw.length; i++) bytes[i] = raw.charCodeAt(i)
  return bytes
}
