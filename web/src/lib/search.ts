/** Whether an item name contains the typed text, ignoring case. A blank search matches every name. */
export function nameMatches(name: string, query: string): boolean {
  return comparable(name).includes(comparable(query))
}

// NFC so "å" typed as a + ring (some keyboards, pasted text) equals the single "å" character.
function comparable(text: string): string {
  return text.normalize('NFC').toLowerCase().trim()
}
