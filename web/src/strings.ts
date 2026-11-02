import type { Allergen, DietType, StorageLocation, Urgency } from './api'

// All user-facing copy lives here so translations can be added later.
export const strings = {
  appName: 'EatUp',
  nav: { food: 'My food', cook: 'Cook now', profile: 'Profile' },

  inventory: {
    title: 'My food',
    heading: "What's in the kitchen?",
    today: (d: Date) => d.toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'long' }),
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
    Pantry: 'Pantry',
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
    save: 'Save',
    saving: 'Saving…',
    delete: 'Delete',
    useSome: 'Used some of it?',
    amountUsed: 'Amount',
    markUsed: 'Mark used',
    markTossed: 'Thrown away',
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

  daysLeft: (days: number) => {
    if (days < -1) return `Expired ${-days} days ago`
    if (days === -1) return 'Expired yesterday'
    if (days === 0) return 'Today'
    if (days === 1) return 'Tomorrow'
    return `In ${days} days`
  },
}
