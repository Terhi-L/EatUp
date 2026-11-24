import { useQuery } from '@tanstack/react-query'
import { api, type WasteSummary } from '../../api'
import { strings } from '../../strings'
import { encouragementFor, monthName } from '../../lib/wasteStats'

type MonthCounts = WasteSummary['thisMonth']

export function WasteStatsSection() {
  // Hourly, so a page left open past midnight on the 1st moves on to the new month by itself.
  const stats = useQuery({ queryKey: ['waste-stats'], queryFn: api.wasteStats, refetchInterval: 60 * 60_000 })

  if (stats.isPending) return null
  if (stats.isError) return <p className="text-danger">{stats.error.message}</p>

  return (
    <section className="space-y-3">
      <h2 className="font-display text-[28px] leading-none font-normal">{strings.waste.title}</h2>
      {stats.data.hasHistory
        ? <WasteStatsCard summary={stats.data} />
        : <p className="text-muted">{strings.waste.empty}</p>}
    </section>
  )
}

function WasteStatsCard({ summary }: { summary: WasteSummary }) {
  const { thisMonth, lastMonth } = summary
  return (
    <div className="card space-y-2 p-4">
      <p className="font-semibold">{strings.waste.thisMonth(nameOf(thisMonth))}</p>
      <p className="flex flex-wrap gap-x-5 gap-y-1 text-[17px]">
        <span>{strings.waste.used(thisMonth.used)}</span>
        <span>{strings.waste.discarded(thisMonth.discarded)}</span>
      </p>
      <p>{strings.waste.encouragement[encouragementFor(summary)]}</p>
      <p className="text-sm text-muted">{strings.waste.lastMonth(nameOf(lastMonth), lastMonth.used, lastMonth.discarded)}</p>
    </div>
  )
}

function nameOf(month: MonthCounts): string {
  return monthName(month.year, month.month, strings.locale)
}
