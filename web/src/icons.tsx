import type { ReactNode, SVGProps } from 'react'
import type { StorageLocation, Urgency } from './api'

type IconProps = Omit<SVGProps<SVGSVGElement>, 'children'> & { size?: number }

function Svg({ size = 20, strokeWidth = 2.2, children, ...rest }: IconProps & { children: ReactNode }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={strokeWidth}
      strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" {...rest}>
      {children}
    </svg>
  )
}

export const FridgeIcon = (p: IconProps) => <Svg {...p}><rect x="5" y="2" width="14" height="20" rx="2" /><path d="M5 9h14M8 5v2M8 12v3" /></Svg>
export const FreezerIcon = (p: IconProps) => <Svg {...p}><path d="M12 2v20M3.3 7l17.4 10M3.3 17L20.7 7" /></Svg>
export const PantryIcon = (p: IconProps) => <Svg {...p}><path d="M7 3h10M8 3v3L6 9v11a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V9l-2-3V3M6 13h12" /></Svg>
export const CounterIcon = (p: IconProps) => <Svg {...p}><path d="M3 11h18a9 9 0 0 1-18 0zM9 8c0-2 2-3 2-5M14 8c0-2 2-3 2-5" /></Svg>
export const BasketIcon = (p: IconProps) => <Svg {...p}><path d="M3 9h18l-2 11H5zM8 9l3-6M16 9l-3-6" /></Svg>
export const PotIcon = (p: IconProps) => <Svg {...p}><path d="M4 10h16v7a3 3 0 0 1-3 3H7a3 3 0 0 1-3-3zM2 10h2M20 10h2M9 6c0-1 1-1 1-2M14 6c0-1 1-1 1-2" /></Svg>
export const UserIcon = (p: IconProps) => <Svg {...p}><circle cx="12" cy="8" r="4" /><path d="M4 21c1-4 4-6 8-6s7 2 8 6" /></Svg>
export const CheckIcon = (p: IconProps) => <Svg strokeWidth={2.6} {...p}><path d="M5 12.5l4.5 4.5L19 7" /></Svg>
export const TrashIcon = (p: IconProps) => <Svg {...p}><path d="M4 7h16M9 7V4h6v3M6 7l1 13h10l1-13" /></Svg>
export const PlusIcon = (p: IconProps) => <Svg strokeWidth={3} {...p}><path d="M12 5v14M5 12h14" /></Svg>
export const AlarmIcon = (p: IconProps) => <Svg strokeWidth={2} {...p}><circle cx="12" cy="13" r="7" /><path d="M12 9v4l2.5 2M4 5l3-2.5M20 5l-3-2.5" /></Svg>
export const ClockIcon = (p: IconProps) => <Svg {...p}><circle cx="12" cy="12" r="8" /><path d="M12 8v4l3 2" /></Svg>
export const BellIcon = (p: IconProps) => <Svg {...p}><path d="M6 16V11a6 6 0 0 1 12 0v5l2 2H4zM10 21h4" /></Svg>
export const LeafIcon = (p: IconProps) => <Svg {...p}><path d="M5 19C5 10 11 5 20 4c0 9-5 15-14 15zM5 19l8-8" /></Svg>

/** The pea-pod logo, drawn in the design's own colours. */
export function Logo({ width = 44 }: { width?: number }) {
  return (
    <svg width={width} height={(width * 36) / 44} viewBox="0 0 44 36" aria-hidden="true">
      <path d="M4 28C9 12 27 5 37 9C32 25 15 33 4 28z" fill="#9BDE7E" stroke="#2B2140" strokeWidth="2.2" strokeLinejoin="round" />
      <circle cx="14" cy="22" r="4" fill="#4FB548" stroke="#2B2140" strokeWidth="1.8" />
      <circle cx="21.5" cy="18" r="4" fill="#4FB548" stroke="#2B2140" strokeWidth="1.8" />
      <circle cx="29" cy="14" r="4" fill="#4FB548" stroke="#2B2140" strokeWidth="1.8" />
      <path d="M37 9l4-5" stroke="#2B2140" strokeWidth="2.2" strokeLinecap="round" />
    </svg>
  )
}

export const locationIcon: Record<StorageLocation, (p: IconProps) => ReactNode> = {
  Fridge: FridgeIcon,
  Freezer: FreezerIcon,
  Pantry: PantryIcon,
  Counter: CounterIcon,
}

export const locationTone: Record<StorageLocation, string> = {
  Fridge: 'bg-fridge',
  Freezer: 'bg-freezer',
  Pantry: 'bg-pantry',
  Counter: 'bg-counter',
}

export const urgencyTone: Record<Urgency, string> = {
  Expired: 'bg-gone hatch',
  Today: 'bg-today',
  Soon: 'bg-soon',
  Later: 'bg-later',
}
