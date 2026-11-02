import { useQuery } from '@tanstack/react-query'
import { api, type Suggestion } from '../../api'
import { strings } from '../../strings'
import { ClockIcon, LeafIcon } from '../../icons'

export function CookNowPage() {
  const suggestions = useQuery({ queryKey: ['suggestions'], queryFn: api.suggestions })

  return (
    <div className="space-y-5">
      <div>
        <h1 className="page-title">{strings.cook.title}</h1>
        <p className="mt-1 text-muted">{strings.cook.intro}</p>
      </div>
      {suggestions.isError && <p className="text-danger">{suggestions.error.message}</p>}
      {suggestions.data?.length === 0 && <p className="py-8 text-center text-muted">{strings.cook.empty}</p>}
      <div className="grid gap-5 lg:grid-cols-2 lg:items-start lg:gap-7">
        {suggestions.data?.map((s) => <SuggestionCard key={s.key} suggestion={s} />)}
      </div>
    </div>
  )
}

function SuggestionCard({ suggestion: s }: { suggestion: Suggestion }) {
  return (
    <article className="card p-4">
      <div className="flex items-start justify-between gap-3">
        <h2 className="font-display text-3xl leading-[1.05] font-normal">{s.title}</h2>
        <span className="flex items-center gap-1 text-sm whitespace-nowrap text-muted">
          <ClockIcon size={16} /> {strings.cook.minutes(s.minutes)}
        </span>
      </div>
      <p className="mt-1 text-muted">{s.description}</p>
      {s.isPlantBased && (
        <p className="mt-2 inline-flex items-center gap-1 rounded-[3px] border-[1.5px] border-ink bg-later px-2 py-0.5 text-xs font-bold">
          <LeafIcon size={14} /> {strings.cook.meatFree}
        </p>
      )}

      <p className="mt-3 text-sm">
        <span className="font-bold">{strings.cook.uses}: </span>
        {s.usesItems.map((i) => `${i.name} (${strings.daysLeft(i.daysLeft).toLowerCase()})`).join(', ')}
      </p>
      {s.missingIngredients.length > 0 && (
        <p className="mt-1 text-sm text-muted">
          <span className="font-bold">{strings.cook.alsoNeed}: </span>
          {s.missingIngredients.join(', ')}
        </p>
      )}

      <details className="mt-3">
        <summary className="cursor-pointer py-2 text-sm font-bold underline decoration-accent decoration-4 underline-offset-4">
          {strings.cook.steps}
        </summary>
        <ol className="mt-2 list-decimal space-y-1 pl-5 text-sm">
          {s.steps.map((step) => <li key={step}>{step}</li>)}
        </ol>
      </details>
    </article>
  )
}
