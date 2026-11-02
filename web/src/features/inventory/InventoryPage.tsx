import { useState } from 'react'
import { Link } from 'react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, type ConsumeOutcome, type Item, type StorageLocation, type Urgency } from '../../api'
import { strings } from '../../strings'
import { AlarmIcon, CheckIcon, PlusIcon, TrashIcon, locationIcon, locationTone, urgencyTone } from '../../icons'

const urgencyOrder: Urgency[] = ['Expired', 'Today', 'Soon', 'Later']
const locations: StorageLocation[] = ['Fridge', 'Freezer', 'Pantry', 'Counter']

export function InventoryPage() {
  const [location, setLocation] = useState<StorageLocation | null>(null)
  const items = useQuery({ queryKey: ['items'], queryFn: () => api.items() })

  if (items.isPending) return null
  if (items.isError) return <p className="text-danger">{items.error.message}</p>

  const visible = items.data.filter((i) => location === null || i.location === location)
  const urgent = items.data.filter((i) => i.urgency === 'Today' || i.urgency === 'Soon')

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="text-sm text-muted lg:text-[15px]">{strings.inventory.today(new Date())}</p>
          <h1 className="page-title mt-0.5">{strings.inventory.heading}</h1>
          <svg viewBox="0 0 190 10" aria-hidden="true" className="mt-0.5 block h-2.5 w-[190px] lg:w-[300px]" preserveAspectRatio="none">
            <path d="M2 6 L188 4" stroke="var(--color-accent)" strokeWidth="5" strokeLinecap="round" />
          </svg>
        </div>
        <Link to="/items/new" className="btn-primary hidden h-[50px] text-[17px] lg:inline-flex">
          <PlusIcon /> {strings.inventory.add}
        </Link>
      </div>

      {urgent.length > 0 && (
        <div className="card flex -rotate-1 items-center gap-3 bg-fridge p-3.5 pl-4 lg:max-w-xl lg:-rotate-[0.6deg]">
          <AlarmIcon size={30} className="flex-none" />
          <div className="min-w-0 flex-1">
            <p className="font-bold">{strings.inventory.urgentBanner(urgent.length)}</p>
            <p className="mt-0.5 truncate text-[13px]">{strings.inventory.urgentNames(urgent.map((i) => i.name))}</p>
          </div>
          <Link to="/cook" className="flex-none rounded border-2 border-ink bg-white px-3 py-2.5 text-sm font-bold">
            {strings.inventory.seeIdeas}
          </Link>
        </div>
      )}

      <div className="-mx-4 flex gap-2 overflow-x-auto px-4 pt-2 pb-1.5 [scrollbar-width:none] lg:mx-0 lg:flex-wrap lg:px-0">
        <button className={`chip ${location === null ? 'chip-on' : 'chip-off'}`} onClick={() => setLocation(null)}>
          {strings.inventory.all} · {items.data.length}
        </button>
        {locations.map((l) => {
          const Icon = locationIcon[l]
          return (
            <button key={l} className={`chip ${location === l ? 'chip-on' : locationTone[l]}`} onClick={() => setLocation(l)}>
              <Icon size={16} />
              {strings.location[l]}
            </button>
          )
        })}
      </div>

      {visible.length === 0 && <p className="py-8 text-center text-muted">{strings.inventory.empty}</p>}

      <div className="grid gap-[18px] lg:grid-cols-[repeat(auto-fit,minmax(280px,1fr))] lg:items-start lg:gap-7">
        {urgencyOrder.map((urgency) => {
          const group = visible.filter((i) => i.urgency === urgency)
          if (group.length === 0) return null
          return (
            <section key={urgency} className="flex flex-col gap-2">
              <div className="flex items-center gap-2.5">
                <h2 className="font-display text-[28px] leading-none font-normal lg:text-[32px]">{strings.urgency[urgency]}</h2>
                <span className={`flex h-[26px] min-w-[26px] items-center justify-center rounded-full border-2 border-ink px-1.5 text-[13px] font-bold ${urgencyTone[urgency]}`}>
                  {group.length}
                </span>
                <div className="flex-1 border-t-2 border-ink" />
              </div>
              <ul className="card divide-y-2 divide-dashed divide-ink/25">
                {group.map((item) => <ItemRow key={item.id} item={item} />)}
              </ul>
            </section>
          )
        })}
      </div>

      <Link
        to="/items/new"
        className="btn-primary fixed right-4 bottom-[104px] z-10 h-[54px] text-[17px] sm:right-[calc(50%-15rem)] lg:hidden"
      >
        <PlusIcon /> {strings.inventory.add}
      </Link>
    </div>
  )
}

function ItemRow({ item }: { item: Item }) {
  const queryClient = useQueryClient()
  const consume = useMutation({
    mutationFn: (outcome: ConsumeOutcome) => api.consume(item.id, outcome),
    onSuccess: () => queryClient.invalidateQueries(),
  })
  const Icon = locationIcon[item.location]

  return (
    <li className="flex items-center gap-2 py-3 pr-2.5 pl-3.5">
      <Link to={`/items/${item.id}`} className="flex min-w-0 flex-1 flex-col gap-1">
        <span className="flex flex-wrap items-center gap-2">
          <span className="text-[17px] font-bold">{item.name}</span>
          <span className={`-rotate-2 rounded-[3px] border-[1.5px] border-ink px-2 py-0.5 text-xs font-bold whitespace-nowrap ${urgencyTone[item.urgency]}`}>
            {strings.daysLeft(item.daysLeft)}
          </span>
        </span>
        <span className="flex items-center gap-1.5 text-[13px] text-muted">
          {item.quantity} {item.unit}
          <span aria-hidden="true">·</span>
          <Icon size={14} />
          {strings.location[item.location]}
          {item.openedDate && ` · ${strings.inventory.opened}`}
        </span>
      </Link>
      <button className="icon-btn bg-later" aria-label={strings.inventory.markUsed(item.name)}
        disabled={consume.isPending} onClick={() => consume.mutate('Used')}>
        <CheckIcon size={22} />
      </button>
      <button className="icon-btn" aria-label={strings.inventory.markTossed(item.name)}
        disabled={consume.isPending} onClick={() => consume.mutate('Discarded')}>
        <TrashIcon />
      </button>
    </li>
  )
}
