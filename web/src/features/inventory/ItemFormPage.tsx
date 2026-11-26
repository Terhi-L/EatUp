import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  api, ApiError, type ConsumeOutcome, type ExpiryKind, type Ingredient, type Item, type ItemInput, type Product, type StorageLocation,
} from '../../api'
import { storedUnit, strings, unitLabel } from '../../strings'
import { addDays, localIsoDate } from '../../lib/dates'
import { locationAfterOpening } from '../../lib/storage'
import { findTypedIngredient } from '../../lib/ingredients'
import { formExpiryKind, sameName, type KindPick } from '../../lib/expiryKind'
import { CheckIcon, TrashIcon, locationIcon, locationTone } from '../../icons'
import { ScanBarcodeButton } from './ScanBarcodeButton'

const locations: StorageLocation[] = ['Fridge', 'Freezer', 'Cupboard', 'Counter']
const expiryKinds: ExpiryKind[] = ['UseBy', 'BestBefore']

export function ItemFormPage() {
  const { id } = useParams()
  const itemId = id ? Number(id) : null
  const item = useQuery({ queryKey: ['items', itemId], queryFn: () => api.item(itemId!), enabled: itemId !== null })

  if (itemId !== null && item.isPending) return null
  if (item.isError) return <p className="text-danger">{item.error.message}</p>
  return <ItemForm key={itemId ?? 'new'} existing={item.data} />
}

function ItemForm({ existing }: { existing?: Item }) {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [name, setName] = useState(existing?.name ?? '')
  const [quantity, setQuantity] = useState(existing?.quantity ?? 1)
  const [unit, setUnit] = useState(unitLabel(existing?.unit ?? 'pcs'))
  const [location, setLocation] = useState<StorageLocation>(existing?.location ?? 'Fridge')
  const [expiryDate, setExpiryDate] = useState(existing?.expiryDate ?? '')
  const [expiryTouched, setExpiryTouched] = useState(existing !== undefined)
  const [kindPick, setKindPick] = useState<KindPick | null>(null)
  const [opened, setOpened] = useState(existing?.openedDate != null)
  const [openedDate, setOpenedDate] = useState(existing?.openedDate ?? localIsoDate())
  const [movedOnOpening, setMovedOnOpening] = useState<StorageLocation | null>(null)
  // An existing item's location was already chosen, so it isn't moved just for being opened.
  const [locationPicked, setLocationPicked] = useState(existing !== undefined)
  const [usedAmount, setUsedAmount] = useState(1)
  const [scanned, setScanned] = useState(false)
  const nameMissing = scanned && !name.trim()
  const expiryMissing = scanned && !expiryDate

  const ingredients = useQuery({
    queryKey: ['ingredients', name.trim().toLowerCase()],
    queryFn: () => api.ingredients(name.trim()),
    enabled: name.trim().length > 0,
    placeholderData: keepPreviousData,
  })
  const match: Ingredient | undefined = findTypedIngredient(ingredients.data, name)
  const expiryKind = formExpiryKind(name, kindPick, existing, match)

  // Picking a known ingredient suggests an expiry date from its typical shelf life.
  useEffect(() => {
    if (!match) return
    const days = match.shelfLifeDays[location]
    if (!expiryTouched && days !== undefined) setExpiryDate(addDays(localIsoDate(), days))
  }, [match, location, expiryTouched])

  // An effect, not the click handler, so it still applies when the ingredient arrives after "opened" is ticked.
  useEffect(() => {
    if (!opened || locationPicked) return
    const target = locationAfterOpening(match, location)
    if (!target) return
    setLocation(target)
    setMovedOnOpening(target)
  }, [opened, locationPicked, match, location])

  const chooseLocation = (l: StorageLocation) => {
    setLocation(l)
    setLocationPicked(true)
    setMovedOnOpening(null)
  }

  const toggleOpened = (checked: boolean) => {
    setOpened(checked)
    setLocationPicked(false)
  }

  // A barcode never carries the expiry date, so it's cleared for the shelf-life suggestion or the user.
  const applyScannedProduct = (product: Product) => {
    setName(product.name ?? '')
    if (product.quantity) {
      setQuantity(product.quantity)
      setUnit(unitLabel(product.unit ?? 'pcs'))
    }
    setExpiryDate('')
    setExpiryTouched(false)
    setKindPick(null)
    setScanned(true)
  }

  const done = () => {
    queryClient.invalidateQueries()
    navigate('/')
  }
  // The 409 text says the list is up to date. Other failures keep the cache: a refetch that also
  // fails would swap the form for an error and lose unsaved edits.
  const refreshAfterConflict = (error: Error) => {
    if (error instanceof ApiError && error.status === 409) queryClient.invalidateQueries()
  }

  const save = useMutation({
    mutationFn: (input: ItemInput) => (existing ? api.updateItem(existing.id, input) : api.createItem(input)),
    onSuccess: done,
    onError: refreshAfterConflict,
  })
  const remove = useMutation({ mutationFn: () => api.deleteItem(existing!.id), onSuccess: done, onError: refreshAfterConflict })
  const consume = useMutation({
    mutationFn: ({ outcome, amount }: { outcome: ConsumeOutcome; amount?: number }) => api.consume(existing!.id, outcome, amount),
    onSuccess: done,
    onError: refreshAfterConflict,
  })

  const submit = (e: FormEvent) => {
    e.preventDefault()
    save.mutate({
      name,
      // Unknown names fall back to the backend's own matching.
      ingredientKey: match?.key ?? (existing && sameName(name, existing.name) ? existing.ingredientKey : null),
      quantity,
      unit: storedUnit(unit),
      location,
      expiryDate: expiryDate || null,
      expiryKind: expiryKind.sent,
      openedDate: opened ? openedDate : null,
    })
  }

  const error = save.error ?? remove.error ?? consume.error

  return (
    <div className="space-y-6 lg:max-w-2xl">
      <h1 className="page-title">{existing ? strings.form.editTitle : strings.form.addTitle}</h1>

      {!existing && <ScanBarcodeButton onFound={applyScannedProduct} />}

      <form onSubmit={submit} className="space-y-4">
        <div>
          <label className="label" htmlFor="name">{strings.form.name}</label>
          <input
            id="name"
            className={`input ${nameMissing ? 'border-danger' : ''}`}
            list="ingredient-options"
            placeholder={strings.form.namePlaceholder}
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            autoFocus={!existing}
            autoComplete="off"
          />
          <datalist id="ingredient-options">
            {ingredients.data?.map((i) => <option key={i.key} value={i.name} />)}
          </datalist>
          {nameMissing && <p className="mt-1 text-xs text-danger">{strings.scan.fillName}</p>}
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="label" htmlFor="quantity">{strings.form.quantity}</label>
            <input id="quantity" className="input" type="number" min="0" step="any" inputMode="decimal"
              value={quantity} onChange={(e) => setQuantity(Number(e.target.value))} />
          </div>
          <div>
            <label className="label" htmlFor="unit">{strings.form.unit}</label>
            <input id="unit" className="input" value={unit} onChange={(e) => setUnit(e.target.value)} />
          </div>
        </div>

        <fieldset>
          <legend className="label">{strings.form.location}</legend>
          <div className="grid grid-cols-2 gap-2">
            {locations.map((l) => {
              const Icon = locationIcon[l]
              return (
                <button type="button" key={l} aria-pressed={location === l}
                  className={`chip h-11 justify-center ${location === l ? 'chip-on' : locationTone[l]}`} onClick={() => chooseLocation(l)}>
                  <Icon size={16} />
                  {strings.location[l]}
                </button>
              )
            })}
          </div>
        </fieldset>

        <div>
          <label className="label" htmlFor="expiry">{strings.form.expiry}</label>
          <input id="expiry" className={`input ${expiryMissing ? 'border-danger' : ''}`} type="date" value={expiryDate}
            onChange={(e) => { setExpiryDate(e.target.value); setExpiryTouched(true) }} />
          {match && !expiryTouched && <p className="mt-1 text-xs text-muted">{strings.form.expiryHint}</p>}
          {expiryMissing && <p className="mt-1 text-xs text-danger">{strings.scan.fillExpiry}</p>}
        </div>

        <ExpiryKindField kind={expiryKind.shown} onPick={(kind) => setKindPick({ kind, name: name.trim() })} />

        <div className="space-y-2">
          <label className="flex min-h-11 items-center gap-2">
            <input type="checkbox" className="size-5 accent-ink" checked={opened} onChange={(e) => toggleOpened(e.target.checked)} />
            {strings.form.opened}
          </label>
          {opened && movedOnOpening && <p className="text-xs text-muted">{strings.form.movedOnOpening(strings.location[movedOnOpening])}</p>}
          {opened && (
            <div>
              <label className="label" htmlFor="opened">{strings.form.openedOn}</label>
              <input id="opened" className="input" type="date" value={openedDate} max={localIsoDate()}
                onChange={(e) => setOpenedDate(e.target.value)} />
            </div>
          )}
        </div>

        {error && <p className="text-sm text-danger">{error.message}</p>}

        <button type="submit" className="btn-primary w-full" disabled={save.isPending}>
          {save.isPending ? strings.form.saving : strings.form.save}
        </button>
      </form>

      {existing && (
        <div className="card space-y-4 p-4">
          <div>
            <label className="label" htmlFor="used">{strings.form.useSome}</label>
            <div className="flex gap-2">
              <input id="used" className="input w-24" type="number" min="0" step="any" inputMode="decimal"
                value={usedAmount} onChange={(e) => setUsedAmount(Number(e.target.value))} />
              <span className="self-center text-muted">{unitLabel(existing.unit)}</span>
              <button className="btn-secondary ml-auto"
                disabled={consume.isPending || !Number.isFinite(usedAmount) || usedAmount <= 0}
                onClick={() => consume.mutate({ outcome: 'Used', amount: usedAmount })}>
                {strings.form.markUsed}
              </button>
            </div>
          </div>
          <div className="flex gap-2">
            <button className="btn-secondary flex-1" disabled={consume.isPending} onClick={() => consume.mutate({ outcome: 'Used' })}>
              <CheckIcon /> {strings.form.markUsed}
            </button>
            <button className="btn-secondary flex-1" disabled={consume.isPending} onClick={() => consume.mutate({ outcome: 'Discarded' })}>
              <TrashIcon /> {strings.form.markTossed}
            </button>
          </div>
          <button className="btn-danger w-full" disabled={remove.isPending} onClick={() => remove.mutate()}>
            {strings.form.delete}
          </button>
        </div>
      )}
    </div>
  )
}

type ExpiryKindFieldProps = { kind: ExpiryKind; onPick: (kind: ExpiryKind) => void }

function ExpiryKindField({ kind, onPick }: ExpiryKindFieldProps) {
  return (
    <fieldset>
      <legend className="label">{strings.form.kindLegend}</legend>
      <div className="grid grid-cols-2 gap-2">
        {expiryKinds.map((k) => (
          <button type="button" key={k} aria-pressed={kind === k}
            className={`chip min-h-11 justify-center py-1 text-center whitespace-normal ${kind === k ? 'chip-on' : 'chip-off'}`}
            onClick={() => onPick(k)}>
            {strings.form.kind[k]}
          </button>
        ))}
      </div>
      <p className="mt-1 text-xs text-muted">{strings.form.kindHint[kind]}</p>
    </fieldset>
  )
}
