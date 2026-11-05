import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, type Allergen, type DietType, type Language, type Profile } from '../../api'
import { language, languageNames, languages, localeOf, setLanguage, strings } from '../../strings'
import { NotificationsCard } from '../push/NotificationsCard'

const diets: DietType[] = ['Vegan', 'Vegetarian', 'Pescatarian', 'Flexitarian', 'Omnivore']
const allergens = Object.keys(strings.allergen) as Allergen[]
const languageSaveKey = ['save-language']

export function ProfilePage() {
  const queryClient = useQueryClient()
  const profile = useQuery({ queryKey: ['profile'], queryFn: api.profile })
  // Each edit sends only the fields it changes, and shows them right away; the server state is refetched after.
  const save = useMutation({
    mutationFn: api.updateProfile,
    onMutate: async (changes) => {
      await queryClient.cancelQueries({ queryKey: ['profile'] })
      queryClient.setQueryData<Profile>(['profile'], (old) => old && { ...old, ...changes })
    },
    // Diet, allergies and language all change what the server sends back (suggestions, names), so refetch everything.
    onSettled: () => queryClient.invalidateQueries(),
  })
  // The UI switches at once; when the last pending save settles, it takes the server's language, so a failed save
  // can't leave them apart. Set on the mutation, not per call, so it still runs after leaving this page.
  const saveLanguage = useMutation({
    mutationKey: languageSaveKey,
    mutationFn: (next: Language) => api.updateProfile({ language: next }),
    onSettled: async () => {
      if (queryClient.isMutating({ mutationKey: languageSaveKey }) > 1) return
      const saved = await queryClient.fetchQuery({ queryKey: ['profile'], queryFn: api.profile, staleTime: 0 }).catch(() => null)
      if (saved?.language) setLanguage(saved.language)
      await queryClient.invalidateQueries()
    },
  })

  if (profile.isPending) return null
  if (profile.isError) return <p className="text-danger">{profile.error.message}</p>

  const p = profile.data
  const update = (changes: Partial<Profile>) => save.mutate(changes)

  const pickLanguage = (next: Language) => {
    setLanguage(next)
    saveLanguage.mutate(next)
  }

  return (
    <div className="space-y-6 lg:max-w-2xl">
      <h1 className="page-title">{strings.profile.title}</h1>

      <section className="space-y-3">
        <h2 className="font-display text-[28px] leading-none font-normal">{strings.profile.notifications}</h2>
        <NotificationsCard />
        <div className="flex items-center gap-3">
          <label className="font-semibold" htmlFor="time">{strings.profile.notificationTime}</label>
          <input id="time" type="time" className="input w-32" value={p.notificationTime.slice(0, 5)}
            onChange={(e) => e.target.value && update({ notificationTime: `${e.target.value}:00` })} />
        </div>
        <p className="text-xs text-muted">{strings.profile.timeZone(p.timeZone)}</p>
      </section>

      <section>
        <h2 className="mb-2 font-display text-[28px] leading-none font-normal">{strings.profile.diet}</h2>
        <div className="space-y-2">
          {diets.map((d) => (
            <label key={d} className={`flex cursor-pointer items-start gap-3 rounded border-2 border-ink p-3 ${p.diet === d ? 'bg-cupboard shadow-hard-sm' : 'bg-white'}`}>
              <input type="radio" name="diet" className="mt-1 size-4 accent-ink" checked={p.diet === d} onChange={() => update({ diet: d })} />
              <span>
                <span className="block font-bold">{strings.diet[d].label}</span>
                <span className="block text-sm text-muted">{strings.diet[d].hint}</span>
              </span>
            </label>
          ))}
        </div>
      </section>

      <section>
        <h2 className="font-display text-[28px] leading-none font-normal">{strings.profile.allergies}</h2>
        <p className="mb-2 mt-1 text-sm text-muted">{strings.profile.allergiesHint}</p>
        <div className="flex flex-wrap gap-2">
          {allergens.map((a) => {
            const on = p.allergens.includes(a)
            return (
              <button key={a} aria-pressed={on} className={`chip ${on ? 'chip-on' : 'chip-off'}`}
                onClick={() => update({ allergens: on ? p.allergens.filter((x) => x !== a) : [...p.allergens, a] })}>
                {strings.allergen[a]}
              </button>
            )
          })}
        </div>
      </section>

      <LanguagePicker onPick={(l) => pickLanguage(l)} />

      {save.isError && <p className="text-sm text-danger">{save.error.message}</p>}
      {saveLanguage.isError && <p className="text-sm text-danger">{saveLanguage.error.message}</p>}

      {import.meta.env.DEV && <DevTools />}
    </div>
  )
}

function LanguagePicker({ onPick }: { onPick: (l: Language) => void }) {
  return (
    <section>
      <h2 className="mb-2 font-display text-[28px] leading-none font-normal">{strings.profile.language}</h2>
      <div className="flex flex-wrap gap-2">
        {languages.map((l) => (
          <button key={l} lang={localeOf(l)} aria-pressed={l === language} className={`chip ${l === language ? 'chip-on' : 'chip-off'}`}
            onClick={() => onPick(l)}>
            {languageNames[l]}
          </button>
        ))}
      </div>
    </section>
  )
}

function DevTools() {
  const queryClient = useQueryClient()
  const [message, setMessage] = useState<string | null>(null)
  const run = (action: () => Promise<string>) =>
    action().then(setMessage, (e: Error) => setMessage(e.message)).finally(() => queryClient.invalidateQueries())

  return (
    <section className="space-y-2 rounded border-2 border-dashed border-ink/50 bg-white p-4">
      <h2 className="font-bold">{strings.dev.title}</h2>
      <div className="flex flex-wrap gap-2">
        <button className="btn-secondary" onClick={() => run(async () => strings.dev.sent((await api.dev.testPush()).sent))}>
          {strings.dev.testPush}
        </button>
        <button className="btn-secondary" onClick={() => run(async () => {
          const r = await api.dev.runDailyCheck()
          return r.title ? `${r.title}: ${r.body} (${strings.dev.sent(r.sent)})` : strings.dev.nothingUrgent
        })}>
          {strings.dev.runDailyCheck}
        </button>
        <button className="btn-secondary" onClick={() => run(async () => { await api.dev.resetDemoData(); return strings.dev.resetDone })}>
          {strings.dev.reset}
        </button>
      </div>
      {message && <p className="text-sm">{message}</p>}
    </section>
  )
}
