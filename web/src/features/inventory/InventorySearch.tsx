import { useState, type ReactNode } from 'react'
import { strings } from '../../strings'
import { CloseIcon, SearchIcon } from '../../icons'

type SearchProps = { query: string; onQueryChange: (query: string) => void }
type SearchFieldProps = SearchProps & { onClose?: () => void; autoFocus?: boolean; className?: string }

/** The inventory search box. Its button and Escape clear the text, or close the search when onClose is given. */
export function SearchField({ query, onQueryChange, onClose, autoFocus = false, className = '' }: SearchFieldProps) {
  const dismiss = onClose ?? (() => onQueryChange(''))
  const showButton = onClose !== undefined || query !== ''

  return (
    <div className={`relative flex items-center ${className}`}>
      <SearchIcon size={18} className="pointer-events-none absolute left-3 text-muted" />
      <input
        type="search"
        className="input py-2 pr-11 pl-10 [&::-webkit-search-cancel-button]:appearance-none"
        aria-label={strings.inventory.search}
        placeholder={strings.inventory.searchPlaceholder}
        enterKeyHint="search"
        autoFocus={autoFocus}
        value={query}
        onChange={(e) => onQueryChange(e.target.value)}
        onKeyDown={(e) => e.key === 'Escape' && dismiss()}
      />
      {showButton && (
        <button
          type="button"
          className="absolute right-1 flex size-9 items-center justify-center rounded"
          aria-label={onClose ? strings.inventory.closeSearch : strings.inventory.clearSearch}
          onClick={dismiss}
        >
          <CloseIcon size={18} />
        </button>
      )}
    </div>
  )
}

/** The phone header on the inventory: the brand plus a search button that opens the search box in its place. */
export function PhoneHeaderSearch({ query, onQueryChange, children }: SearchProps & { children: ReactNode }) {
  const [open, setOpen] = useState(query !== '')
  const close = () => {
    onQueryChange('')
    setOpen(false)
  }

  if (open) return <SearchField query={query} onQueryChange={onQueryChange} onClose={close} autoFocus />

  return (
    <div className="flex items-center justify-between">
      {children}
      <button type="button" className="icon-btn" aria-label={strings.inventory.search} onClick={() => setOpen(true)}>
        <SearchIcon />
      </button>
    </div>
  )
}
