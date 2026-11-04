import type { Strings } from './en'

const locale = 'fi-FI'

// Finnish. Drafted from the English text; have a native speaker review changes.
export const fi: Strings = {
  locale,
  appName: 'EatUp',
  nav: { food: 'Ruokani', cook: 'Kokkaa nyt', profile: 'Profiili' },

  inventory: {
    title: 'Ruokani',
    heading: 'Mitä keittiöstä löytyy?',
    today: (d: Date) => d.toLocaleDateString(locale, { weekday: 'long', day: 'numeric', month: 'long' }),
    add: 'Lisää ruokaa',
    empty: 'Täällä ei ole vielä mitään. Lisää ostamasi ruoat, niin EatUp muistuttaa sinua ennen kuin ne pilaantuvat.',
    all: 'Kaikki',
    markUsed: (name: string) => `Merkitse syödyksi: ${name}`,
    markTossed: (name: string) => `Heitä pois: ${name}`,
    opened: 'avattu',
    urgentBanner: (n: number) => (n === 1 ? '1 tuote pitäisi syödä pian' : `${n} tuotetta pitäisi syödä pian`),
    urgentNames: (names: string[]) =>
      names.length <= 3 ? names.join(', ') : `${names.slice(0, 3).join(', ')} ja ${names.length - 3} muuta`,
    seeIdeas: 'Ideoita →',
  },

  urgency: {
    Expired: 'Vanhentunut',
    Today: 'Syö tänään',
    Soon: '2 päivän sisällä',
    Later: 'Myöhemmin',
  },

  location: {
    Fridge: 'Jääkaappi',
    Freezer: 'Pakastin',
    Cupboard: 'Kaappi',
    Counter: 'Työtaso',
  },

  form: {
    addTitle: 'Lisää ruokaa',
    editTitle: 'Muokkaa ruokaa',
    name: 'Mikä se on?',
    namePlaceholder: 'esim. Kananmunat, Avokado, Maito',
    quantity: 'Määrä',
    unit: 'Yksikkö',
    location: 'Missä sitä säilytetään?',
    expiry: 'Parasta ennen / viimeinen käyttöpäivä',
    expiryHint: 'Ehdotettu tavallisen säilyvyyden mukaan. Tarkista pakkaus!',
    opened: 'Se on avattu',
    openedOn: 'Avattu',
    movedOnOpening: (place: string) => `Siirretty: ${place}. Säilytä siellä avaamisen jälkeen.`,
    save: 'Tallenna',
    saving: 'Tallennetaan…',
    delete: 'Poista',
    useSome: 'Käytitkö osan?',
    amountUsed: 'Määrä',
    markUsed: 'Merkitse käytetyksi',
    markTossed: 'Heitetty pois',
    units: { pcs: 'kpl', can: 'purkkia' },
  },

  scan: {
    button: 'Skannaa viivakoodi',
    reading: 'Luetaan viivakoodia…',
    found: (product: string | null) =>
      product ? `Löytyi: ${product}. Tarkista tiedot ja lisää päiväys pakkauksesta.` : 'Löytyi. Tarkista tiedot alta.',
    noBarcode: 'Viivakoodia ei näkynyt. Yritä uudelleen lähempää niin, että viivakoodi on suorassa ja tarkka.',
    unknownProduct: 'Tätä tuotetta ei vielä ole Open Food Factsissa. Täytä tiedot alle.',
    failed: 'Tuotetta ei voitu hakea. Täytä tiedot itse.',
    fillName: 'Kirjoita, mikä se on.',
    fillExpiry: 'Lisää päiväys pakkauksesta.',
  },

  cook: {
    title: 'Kokkaa nyt',
    intro: 'Reseptejä, joilla käytät seuraavan 2 päivän aikana vanhenevat ruoat.',
    empty: 'Mikään ei ole vanhenemassa. Hyvää työtä!',
    uses: 'Käyttää',
    alsoNeed: 'Tarvitset myös',
    minutes: (m: number) => `${m} min`,
    meatFree: 'Lihaton',
    steps: 'Valmistus',
  },

  profile: {
    title: 'Profiili',
    diet: 'Miten syöt?',
    allergies: 'Allergiat ja intoleranssit',
    allergiesHint: 'Näitä sisältäviä reseptejä ei koskaan ehdoteta.',
    notifications: 'Päivittäinen muistutus',
    notificationTime: 'Muistuta minua klo',
    timeZone: (tz: string) => `Aikavyöhyke: ${tz}`,
    saved: 'Tallennettu',
    language: 'Kieli',
  },

  diet: {
    Omnivore: { label: 'Sekasyöjä', hint: 'Syön kaikkea (kasvisideat tulevat silti ensin)' },
    Flexitarian: { label: 'Fleksaaja', hint: 'Enimmäkseen kasviksia, lihaa tai kalaa silloin tällöin' },
    Pescatarian: { label: 'Pescovegetaristi', hint: 'Kasvissyöjä, joka syö kalaa' },
    Vegetarian: { label: 'Kasvissyöjä', hint: 'Ei lihaa eikä kalaa' },
    Vegan: { label: 'Vegaani', hint: 'Ei eläinperäisiä tuotteita' },
  },

  allergen: {
    Gluten: 'Gluteeni', Crustaceans: 'Äyriäiset', Eggs: 'Kananmuna', Fish: 'Kala', Peanuts: 'Maapähkinä',
    Soy: 'Soija', Milk: 'Maito', TreeNuts: 'Pähkinät', Celery: 'Selleri', Mustard: 'Sinappi',
    Sesame: 'Seesami', Sulphites: 'Sulfiitit', Lupin: 'Lupiini', Molluscs: 'Nilviäiset',
  },

  push: {
    pitch: 'Saat ilmoituksen ennen kuin ruoka pilaantuu',
    pitchDetail: 'Yksi ilmoitus päivässä, vain kun jotain pitää käyttää.',
    enable: 'Ota ilmoitukset käyttöön',
    enabled: 'Ilmoitukset ovat käytössä tällä laitteella.',
    disable: 'Poista käytöstä',
    denied: 'Ilmoitukset on estetty. Salli ne tälle sivustolle selaimen asetuksista.',
    unsupported: 'Tämä selain ei tue ilmoituksia.',
    iosInstall: 'Jotta saat ilmoituksia iPhonella, lisää EatUp ensin Koti-valikkoon: napauta Jaa ja sitten ”Lisää Koti-valikkoon”. Avaa EatUp sen jälkeen sieltä.',
  },

  dev: {
    title: 'Kehittäjätyökalut',
    testPush: 'Lähetä testi-ilmoitus',
    runDailyCheck: 'Suorita päivittäinen tarkistus nyt',
    reset: 'Palauta esimerkkidata',
    sent: (n: number) => `Lähetetty ${n} laitteelle.`,
    nothingUrgent: 'Ei mitään kiireellistä, joten ilmoitusta ei lähetetty.',
    resetDone: 'Esimerkkidata palautettu.',
  },

  errors: {
    offline: 'EatUp-palvelimeen ei saada yhteyttä. Onko `npm run dev` käynnissä?',
    requestFailed: (status: number) => `Pyyntö epäonnistui (${status})`,
    byStatus: {
      403: 'Pyyntö estettiin. Lataa sivu uudelleen ja yritä uudestaan.',
      404: 'Ei löytynyt. Se on ehkä poistettu.',
      502: 'Tuotetietokantaan ei saatu yhteyttä. Täytä tiedot itse.',
    },
  },

  apiErrors: {
    name: 'Nimi on pakollinen.',
    expiryDate: 'Päiväys on pakollinen.',
    amount: 'Määrän on oltava suurempi kuin nolla.',
    barcode: 'Viivakoodissa on oltava 8–14 numeroa.',
  },

  daysLeft: (days: number) => {
    if (days < -1) return `Vanheni ${-days} päivää sitten`
    if (days === -1) return 'Vanheni eilen'
    if (days === 0) return 'Tänään'
    if (days === 1) return 'Huomenna'
    return `${days} päivän päästä`
  },
}
