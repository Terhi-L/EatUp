import type { Strings } from './en'

const locale = 'nb-NO'

// Norwegian Bokmål. Drafted from the English text; have a native speaker review changes.
export const nb: Strings = {
  locale,
  appName: 'EatUp',
  nav: { food: 'Maten min', cook: 'Lag mat', profile: 'Profil' },

  inventory: {
    title: 'Maten min',
    heading: 'Hva har du på kjøkkenet?',
    today: (d: Date) => d.toLocaleDateString(locale, { weekday: 'long', day: 'numeric', month: 'long' }),
    add: 'Legg til mat',
    empty: 'Ingenting her ennå. Legg inn det du har kjøpt, så minner EatUp deg på det før det blir dårlig.',
    all: 'Alle',
    markUsed: (name: string) => `Merk ${name} som spist`,
    markTossed: (name: string) => `Kast ${name}`,
    opened: 'åpnet',
    urgentBanner: (n: number) => `${n} ${n === 1 ? 'vare' : 'varer'} bør spises snart`,
    urgentNames: (names: string[]) =>
      names.length <= 3 ? names.join(', ') : `${names.slice(0, 3).join(', ')} og ${names.length - 3} til`,
    seeIdeas: 'Ideer →',
  },

  urgency: {
    Expired: 'Gått ut',
    Today: 'Spis i dag',
    Soon: 'Innen 2 dager',
    Later: 'Senere',
  },

  location: {
    Fridge: 'Kjøleskap',
    Freezer: 'Fryser',
    Cupboard: 'Skap',
    Counter: 'Benk',
  },

  form: {
    addTitle: 'Legg til mat',
    editTitle: 'Rediger mat',
    name: 'Hva er det?',
    namePlaceholder: 'f.eks. Egg, Avokado, Melk',
    quantity: 'Mengde',
    unit: 'Enhet',
    location: 'Hvor oppbevares det?',
    expiry: 'Best før / siste forbruksdag',
    expiryHint: 'Foreslått ut fra vanlig holdbarhet. Sjekk pakken!',
    opened: 'Den er åpnet',
    openedOn: 'Åpnet',
    movedOnOpening: (place: string) => `Flyttet til ${place.toLowerCase()}: oppbevares der etter åpning.`,
    save: 'Lagre',
    saving: 'Lagrer…',
    delete: 'Slett',
    useSome: 'Brukt litt av den?',
    amountUsed: 'Mengde',
    markUsed: 'Merk som brukt',
    markTossed: 'Kastet',
    units: { pcs: 'stk', can: 'boks' },
  },

  scan: {
    button: 'Skann strekkode',
    reading: 'Leser strekkoden…',
    found: (product: string | null) =>
      product ? `Fant ${product}. Sjekk detaljene og legg inn datoen fra pakken.` : 'Funnet. Sjekk detaljene nedenfor.',
    noBarcode: 'Fant ingen strekkode. Prøv igjen nærmere, med strekkoden flat og skarp.',
    unknownProduct: 'Denne varen finnes ikke i Open Food Facts ennå. Fyll inn detaljene nedenfor.',
    failed: 'Kunne ikke slå opp varen. Fyll inn detaljene selv.',
    fillName: 'Skriv hva det er.',
    fillExpiry: 'Legg inn datoen fra pakken.',
  },

  cook: {
    title: 'Lag mat',
    intro: 'Oppskrifter som bruker opp det som går ut på dato de neste 2 dagene.',
    empty: 'Ingenting er i ferd med å gå ut. Bra jobba!',
    uses: 'Bruker',
    alsoNeed: 'Du trenger også',
    minutes: (m: number) => `${m} min`,
    meatFree: 'Kjøttfri',
    steps: 'Slik gjør du',
  },

  profile: {
    title: 'Profil',
    diet: 'Hvordan spiser du?',
    allergies: 'Allergier og intoleranser',
    allergiesHint: 'Oppskrifter med disse blir aldri foreslått.',
    notifications: 'Daglig påminnelse',
    notificationTime: 'Minn meg på kl.',
    timeZone: (tz: string) => `Tidssone: ${tz}`,
    saved: 'Lagret',
    language: 'Språk',
  },

  diet: {
    Omnivore: { label: 'Altetende', hint: 'Jeg spiser alt (plantebaserte ideer kommer likevel først)' },
    Flexitarian: { label: 'Fleksitarianer', hint: 'Mest planter, kjøtt eller fisk av og til' },
    Pescatarian: { label: 'Pescetarianer', hint: 'Vegetarianer, men spiser fisk' },
    Vegetarian: { label: 'Vegetarianer', hint: 'Ikke kjøtt eller fisk' },
    Vegan: { label: 'Veganer', hint: 'Ingen animalske produkter' },
  },

  allergen: {
    Gluten: 'Gluten', Crustaceans: 'Skalldyr', Eggs: 'Egg', Fish: 'Fisk', Peanuts: 'Peanøtter',
    Soy: 'Soya', Milk: 'Melk', TreeNuts: 'Nøtter', Celery: 'Selleri', Mustard: 'Sennep',
    Sesame: 'Sesam', Sulphites: 'Sulfitt', Lupin: 'Lupin', Molluscs: 'Bløtdyr',
  },

  push: {
    pitch: 'Få beskjed før maten blir dårlig',
    pitchDetail: 'Ett varsel om dagen, bare når noe bør brukes.',
    enable: 'Slå på varsler',
    enabled: 'Varsler er på for denne enheten.',
    disable: 'Slå av',
    denied: 'Varsler er blokkert. Tillat dem for dette nettstedet i nettleserinnstillingene.',
    unsupported: 'Denne nettleseren støtter ikke varsler.',
    iosInstall: 'For å få varsler på iPhone må du først legge EatUp til på Hjem-skjermen: trykk Del, deretter «Legg til på Hjem-skjerm». Åpne så EatUp derfra.',
  },

  dev: {
    title: 'Utviklerverktøy',
    testPush: 'Send testvarsel',
    runDailyCheck: 'Kjør daglig sjekk nå',
    reset: 'Tilbakestill demodata',
    sent: (n: number) => `Sendt til ${n} enhet(er).`,
    nothingUrgent: 'Ingenting haster, så ikke noe varsel.',
    resetDone: 'Demodata er tilbakestilt.',
  },

  errors: {
    offline: 'Får ikke kontakt med EatUp-serveren. Kjører `npm run dev`?',
    requestFailed: (status: number) => `Forespørselen feilet (${status})`,
    byStatus: {
      403: 'Forespørselen ble blokkert. Last inn siden på nytt og prøv igjen.',
      404: 'Ikke funnet. Den kan ha blitt fjernet.',
      409: 'Dette ble endret et annet sted. Listen er oppdatert nå; prøv igjen om nødvendig.',
      502: 'Fikk ikke kontakt med produktdatabasen. Fyll inn opplysningene selv.',
    },
  },

  apiErrors: {
    name: 'Navn må fylles ut.',
    expiryDate: 'Datoen må fylles ut.',
    amount: 'Mengden må være større enn null.',
    barcode: 'Strekkoden må ha 8 til 14 sifre.',
  },

  daysLeft: (days: number) => {
    if (days < -1) return `Gikk ut for ${-days} dager siden`
    if (days === -1) return 'Gikk ut i går'
    if (days === 0) return 'I dag'
    if (days === 1) return 'I morgen'
    return `Om ${days} dager`
  },
}
