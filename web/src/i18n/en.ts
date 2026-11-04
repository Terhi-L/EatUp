import type { Allergen, DietType, StorageLocation, Urgency } from '../api'

const locale = 'en-GB'

// English is the source language: the other languages must have exactly the same keys (see Strings).
export const en = {
  locale,
  appName: 'EatUp',
  nav: { food: 'My food', cook: 'Cook now', profile: 'Profile' },

  inventory: {
    title: 'My food',
    heading: "What's in the kitchen?",
    today: (d: Date) => d.toLocaleDateString(locale, { weekday: 'long', day: 'numeric', month: 'long' }),
    add: 'Add food',
    empty: 'Nothing here yet. Add what you bought and EatUp will remind you before it goes off.',
    all: 'All',
    markUsed: (name: string) => `Mark ${name} as eaten`,
    markTossed: (name: string) => `Throw out ${name}`,
    opened: 'opened',
    urgentBanner: (n: number) => `${n} ${n === 1 ? 'thing wants' : 'things want'} eating soon`,
    urgentNames: (names: string[]) =>
      names.length <= 3 ? names.join(', ') : `${names.slice(0, 3).join(', ')} and ${names.length - 3} more`,
    seeIdeas: 'Ideas →',
  },

  urgency: {
    Expired: 'Gone off',
    Today: 'Eat today',
    Soon: 'Within 2 days',
    Later: 'Later',
  } satisfies Record<Urgency, string>,

  location: {
    Fridge: 'Fridge',
    Freezer: 'Freezer',
    Cupboard: 'Cupboard',
    Counter: 'Counter',
  } satisfies Record<StorageLocation, string>,

  form: {
    addTitle: 'Add food',
    editTitle: 'Edit food',
    name: 'What is it?',
    namePlaceholder: 'e.g. Eggs, Avocado, Milk',
    quantity: 'Amount',
    unit: 'Unit',
    location: 'Where is it kept?',
    expiry: 'Best before / use by',
    expiryHint: 'Suggested from typical shelf life. Check the package!',
    opened: "It's opened",
    openedOn: 'Opened on',
    movedOnOpening: (place: string) => `Moved to ${place}: keep it there once opened.`,
    save: 'Save',
    saving: 'Saving…',
    delete: 'Delete',
    useSome: 'Used some of it?',
    amountUsed: 'Amount',
    markUsed: 'Mark used',
    markTossed: 'Thrown away',
    units: { pcs: 'pcs', can: 'can' },
  },

  scan: {
    button: 'Scan barcode',
    reading: 'Reading barcode…',
    found: (product: string | null) =>
      product ? `Found ${product}. Check the details and add the date from the package.` : 'Found it. Check the details below.',
    noBarcode: "Couldn't see a barcode. Try again closer, with the barcode flat and sharp.",
    unknownProduct: "This product isn't in Open Food Facts yet. Fill in the details below.",
    failed: "Couldn't look up the product. Fill in the details yourself.",
    fillName: 'Please type what it is.',
    fillExpiry: 'Please add the date from the package.',
  },

  cook: {
    title: 'Cook now',
    intro: 'Recipes that use up what expires in the next 2 days.',
    empty: 'Nothing is about to expire. Nice work!',
    uses: 'Uses your',
    alsoNeed: "You'll also need",
    minutes: (m: number) => `${m} min`,
    meatFree: 'Meat-free',
    steps: 'How to make it',
  },

  profile: {
    title: 'Profile',
    diet: 'How do you eat?',
    allergies: 'Allergies & intolerances',
    allergiesHint: 'Recipes with these are never suggested.',
    notifications: 'Daily reminder',
    notificationTime: 'Remind me at',
    timeZone: (tz: string) => `Time zone: ${tz}`,
    saved: 'Saved',
    language: 'Language',
  },

  diet: {
    Omnivore: { label: 'Omnivore', hint: 'I eat everything (plant-based ideas still come first)' },
    Flexitarian: { label: 'Flexitarian', hint: 'Mostly plants, meat or fish now and then' },
    Pescatarian: { label: 'Pescatarian', hint: 'Vegetarian plus fish' },
    Vegetarian: { label: 'Vegetarian', hint: 'No meat or fish' },
    Vegan: { label: 'Vegan', hint: 'No animal products' },
  } satisfies Record<DietType, { label: string; hint: string }>,

  allergen: {
    Gluten: 'Gluten', Crustaceans: 'Crustaceans', Eggs: 'Eggs', Fish: 'Fish', Peanuts: 'Peanuts',
    Soy: 'Soy', Milk: 'Milk', TreeNuts: 'Tree nuts', Celery: 'Celery', Mustard: 'Mustard',
    Sesame: 'Sesame', Sulphites: 'Sulphites', Lupin: 'Lupin', Molluscs: 'Molluscs',
  } satisfies Record<Allergen, string>,

  push: {
    pitch: 'Get a heads-up before food goes off',
    pitchDetail: 'One notification a day, only when something needs using.',
    enable: 'Turn on notifications',
    enabled: 'Notifications are on for this device.',
    disable: 'Turn off',
    denied: 'Notifications are blocked. Allow them for this site in your browser settings.',
    unsupported: "This browser doesn't support notifications.",
    iosInstall: 'To get notifications on iPhone, add EatUp to your home screen first: tap Share, then "Add to Home Screen". Then open EatUp from there.',
  },

  dev: {
    title: 'Developer tools',
    testPush: 'Send test notification',
    runDailyCheck: 'Run daily check now',
    reset: 'Reset demo data',
    sent: (n: number) => `Sent to ${n} device(s).`,
    nothingUrgent: 'Nothing urgent, so no notification.',
    resetDone: 'Demo data restored.',
  },

  errors: {
    offline: "Can't reach the EatUp backend. Is `npm run dev` running?",
    requestFailed: (status: number) => `Request failed (${status})`,
    /** Errors without a field, by HTTP status. */
    byStatus: {
      403: 'The request was blocked. Reload the page and try again.',
      404: 'Not found. It may have been removed.',
      502: "Couldn't reach the product database. Fill in the details yourself.",
    } as Record<number, string | undefined>,
  },

  /** Validation errors from the backend, by field name. */
  apiErrors: {
    name: 'Name is required.',
    expiryDate: 'Expiry date is required.',
    amount: 'Amount must be more than zero.',
    barcode: 'Barcode must be 8 to 14 digits.',
  } as Record<string, string | undefined>,

  daysLeft: (days: number) => {
    if (days < -1) return `Expired ${-days} days ago`
    if (days === -1) return 'Expired yesterday'
    if (days === 0) return 'Today'
    if (days === 1) return 'Tomorrow'
    return `In ${days} days`
  },
}

export type Strings = typeof en
