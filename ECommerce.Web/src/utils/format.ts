// Change these two to switch currency display across the whole app
const CURRENCY = 'NGN'
const LOCALE = 'en-NG'

const money = new Intl.NumberFormat(LOCALE, { style: 'currency', currency: CURRENCY })
const dateTime = new Intl.DateTimeFormat(LOCALE, { dateStyle: 'medium', timeStyle: 'short' })

export const formatMoney = (value: number) => money.format(value)

// The API returns UTC without a "Z" suffix, so mark it as UTC before parsing
export function formatDate(value: string) {
  const iso = /Z|[+-]\d\d:\d\d$/.test(value) ? value : `${value}Z`
  return dateTime.format(new Date(iso))
}

export const shortId = (id: string) => id.slice(0, 8).toUpperCase()

// "HomeAndGarden" -> "Home and garden"
export function humanize(value: string) {
  const spaced = value.replace(/([a-z])([A-Z])/g, '$1 $2').toLowerCase()
  return spaced.charAt(0).toUpperCase() + spaced.slice(1)
}
