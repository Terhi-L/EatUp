import { Link } from 'react-router'
import type { Suggestion } from '../../api'
import { strings } from '../../strings'
import { ClockIcon } from '../../icons'

/** Nudges toward cooking the top suggestion before its ingredients go off. */
export function TonightCard({ suggestion, className = '' }: { suggestion: Suggestion; className?: string }) {
  return (
    <section aria-labelledby="tonight-heading" className={`card flex-col gap-1.5 bg-cupboard p-4 lg:rotate-[0.6deg] ${className}`}>
      <h2 id="tonight-heading" className="font-display text-[26px] leading-none font-normal">{strings.inventory.tonight}</h2>
      <div className="flex items-start justify-between gap-3">
        <h3 className="text-lg leading-tight font-bold">{suggestion.title}</h3>
        <span className="flex flex-none items-center gap-1 text-sm text-muted">
          <ClockIcon size={16} /> {strings.cook.minutes(suggestion.minutes)}
        </span>
      </div>
      <p className="text-[13px]">
        <span className="font-bold">{strings.cook.uses}: </span>
        {suggestion.usesItems.map((i) => i.name).join(', ')}
      </p>
      <Link to="/cook" className="mt-1 self-start rounded border-2 border-ink bg-white px-3 py-2.5 text-sm font-bold">
        {strings.inventory.seeRecipe}
      </Link>
    </section>
  )
}
