import { useEffect, useState } from 'react'
import { strings } from '../../strings'
import { BellIcon, CheckIcon } from '../../icons'
import { disablePush, enablePush, getPushState, type PushState } from './push'

export function NotificationsCard() {
  const [state, setState] = useState<PushState | null>(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getPushState().then(setState, () => setState('unsupported'))
  }, [])

  const run = async (action: () => Promise<PushState>) => {
    setBusy(true)
    setError(null)
    try {
      setState(await action())
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    } finally {
      setBusy(false)
    }
  }

  if (state === null) return null

  return (
    <div className="card p-4">
      {state === 'needs-install' && <p className="text-sm">{strings.push.iosInstall}</p>}
      {state === 'unsupported' && <p className="text-sm">{strings.push.unsupported}</p>}
      {state === 'denied' && <p className="text-sm">{strings.push.denied}</p>}
      {state === 'disabled' && (
        <>
          <p className="flex items-center gap-2 font-bold"><BellIcon /> {strings.push.pitch}</p>
          <p className="mt-1 text-sm text-muted">{strings.push.pitchDetail}</p>
          <button className="btn-primary mt-3 w-full" disabled={busy} onClick={() => run(enablePush)}>
            {strings.push.enable}
          </button>
        </>
      )}
      {state === 'enabled' && (
        <div className="flex items-center justify-between gap-3">
          <p className="flex items-center gap-2 text-sm"><CheckIcon size={18} /> {strings.push.enabled}</p>
          <button className="btn-secondary" disabled={busy} onClick={() => run(disablePush)}>
            {strings.push.disable}
          </button>
        </div>
      )}
      {error && <p className="mt-2 text-sm text-danger">{error}</p>}
    </div>
  )
}
