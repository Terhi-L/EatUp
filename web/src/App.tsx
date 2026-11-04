import { useEffect, type ReactNode } from 'react'
import { NavLink, Route, Routes } from 'react-router'
import { useQueryClient, type QueryClient } from '@tanstack/react-query'
import { api, type Profile } from './api'
import { language, setLanguage, strings, useLanguage } from './strings'
import { browserTimeZone } from './lib/dates'
import { profileChanges } from './lib/profileSync'
import { syncPush } from './features/push/push'
import { InventoryPage } from './features/inventory/InventoryPage'
import { ItemFormPage } from './features/inventory/ItemFormPage'
import { CookNowPage } from './features/suggestions/CookNowPage'
import { ProfilePage } from './features/profile/ProfilePage'
import { BasketIcon, Logo, PotIcon, UserIcon } from './icons'

const tabs = () => [
  { to: '/', icon: <BasketIcon size={22} />, label: strings.nav.food },
  { to: '/cook', icon: <PotIcon size={22} />, label: strings.nav.cook },
  { to: '/profile', icon: <UserIcon size={22} />, label: strings.nav.profile },
]

export function App() {
  const queryClient = useQueryClient()
  useLanguage() // re-renders the whole app in the new language when it changes

  useEffect(() => {
    syncProfile(queryClient).catch(() => {})
    syncPush().catch(() => {})
  }, [queryClient])

  return (
    <div className="min-h-dvh lg:flex">
      {/* Desktop: sidebar */}
      <aside className="sticky top-0 hidden h-dvh w-64 flex-none flex-col gap-7 border-r-2 border-ink bg-white px-6 py-7 lg:flex">
        <Brand large />
        <nav aria-label="Main" className="flex flex-col gap-2">
          {tabs().map((t) => <SideLink key={t.to} {...t} />)}
        </nav>
      </aside>

      <div className="mx-auto flex w-full max-w-lg min-w-0 flex-1 flex-col lg:max-w-6xl">
        <header className="px-4 pt-[calc(env(safe-area-inset-top)+18px)] pb-1.5 lg:hidden">
          <Brand />
        </header>

        <main className="flex-1 px-4 pt-2 pb-28 lg:px-10 lg:pt-8 lg:pb-12">
          <Routes>
            <Route path="/" element={<InventoryPage />} />
            <Route path="/items/new" element={<ItemFormPage />} />
            <Route path="/items/:id" element={<ItemFormPage />} />
            <Route path="/cook" element={<CookNowPage />} />
            <Route path="/profile" element={<ProfilePage />} />
          </Routes>
        </main>
      </div>

      {/* Phone: bottom tab bar */}
      <nav aria-label="Main" className="fixed inset-x-0 bottom-0 z-10 border-t-2 border-ink bg-white pb-[env(safe-area-inset-bottom)] lg:hidden">
        <div className="mx-auto grid h-[84px] max-w-lg grid-cols-3 items-center px-2">
          {tabs().map((t) => <BottomTab key={t.to} {...t} />)}
        </div>
      </nav>
    </div>
  )
}

// Reminders need the browser's time zone; the profile's language applies to every device.
async function syncProfile(queryClient: QueryClient) {
  const profile = await api.profile()
  if (profile.language) setLanguage(profile.language)

  const changes = profileChanges(profile, browserTimeZone(), language)
  if (Object.keys(changes).length === 0) return
  queryClient.setQueryData<Profile>(['profile'], await api.updateProfile(changes))
  // Suggestions and ingredient names fetched before the first language save came back in English.
  if (changes.language) await queryClient.invalidateQueries({ predicate: (q) => q.queryKey[0] !== 'profile' })
}

function Brand({ large = false }: { large?: boolean }) {
  return (
    <div className="flex items-center gap-2">
      <Logo width={large ? 50 : 44} />
      <span className={`font-display leading-none ${large ? 'text-[42px]' : 'text-4xl'}`}>{strings.appName}</span>
    </div>
  )
}

type TabProps = { to: string; icon: ReactNode; label: string }

function BottomTab({ to, icon, label }: TabProps) {
  return (
    <NavLink to={to} end={to === '/'} className="flex flex-col items-center gap-0.5 text-xs font-semibold aria-[current=page]:font-bold">
      {({ isActive }) => (
        <>
          <span className={`flex rounded border-2 px-3.5 py-1 ${isActive ? 'border-ink bg-accent' : 'border-transparent'}`}>{icon}</span>
          {label}
        </>
      )}
    </NavLink>
  )
}

function SideLink({ to, icon, label }: TabProps) {
  return (
    <NavLink
      to={to}
      end={to === '/'}
      className={({ isActive }) =>
        `flex h-12 items-center gap-2.5 rounded border-2 px-3.5 text-base ${isActive ? 'border-ink bg-accent font-bold shadow-hard-sm' : 'border-transparent font-semibold'}`
      }
    >
      {icon}
      {label}
    </NavLink>
  )
}
