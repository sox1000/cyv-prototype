# Urgent SEO fixes — city × occasion template (chooseyourvenue.com)

**Scope:** template-level changes only. No visual/design changes required — all fixes are meta tags, JSON-LD blocks, HTML element swaps (keeping existing CSS classes), and one data-query fix. Reference page: `/hire/uk/hampshire/southampton/christening-party` (same template as the Exeter summer-party page).

Ordered by impact. Items 1–3 are blockers; nothing else matters until they're resolved.

---

## 1. 🔴 BLOCKER — robots meta is `noindex,nofollow`

**Current:**
```html
<meta name="robots" content="noindex,nofollow">
```

This removes the page from Google entirely and stops link equity flowing through every link on it (venue profiles, nearby-town links). Every other fix in this document is pointless while this tag is in place.

**Action — one of two, depending on business decision:**

If these pages SHOULD rank (recommended for cities with decent venue inventory):
```html
<meta name="robots" content="index,follow">
```

If these pages are intentionally excluded (crawl-budget policy):
```html
<meta name="robots" content="noindex,follow">
```
Never `noindex,nofollow` — `follow` must be kept so venue pages and sibling city pages still receive internal link equity.

**Also check:** these URL patterns must NOT be in robots.txt `Disallow`. A disallowed URL's noindex tag is never seen by Google, so the two directives cancel each other out.

**Verify after deploy:** Search Console → URL Inspection on 2–3 city pages; then watch Pages/Indexing report over the following weeks.

---

## 2. 🔴 BLOCKER — results scope mismatch (county results on city pages)

The Southampton page renders **"65 Venues for Hire in Hampshire, England"** — a county-wide result set on a city-scoped URL. If every Hampshire city page falls back to the same 65 venues, all those pages are near-duplicates and none will rank well.

**Action:**
- Scope the venue query to the city (or city + radius) so the count and result set match the H1/URL.
- Update the results heading template: `{count} christening party venues in {city}` — not the county.
- If a city has too few venues to fill a page, either (a) show city venues first with a clearly labelled "More venues nearby in {county}" section, or (b) keep that city page `noindex,follow` until inventory exists. Do not silently substitute county results.

---

## 3. 🔴 Canonical tag — missing

Sort orders, radius filters and pagination can all create URL variants. Add a self-referencing canonical to the clean URL:

```html
<link rel="canonical" href="https://www.chooseyourvenue.com/hire/uk/hampshire/southampton/christening-party">
```

Templated: scheme + host + clean path, no query parameters.

---

## 4. 🟠 BreadcrumbList schema — missing

The visible breadcrumb exists; the schema doesn't. Server-render this in the `<head>` (do not inject via JS):

```html
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "BreadcrumbList",
  "itemListElement": [
    { "@type": "ListItem", "position": 1, "name": "Home", "item": "https://www.chooseyourvenue.com/" },
    { "@type": "ListItem", "position": 2, "name": "Event Types", "item": "https://www.chooseyourvenue.com/event-types" },
    { "@type": "ListItem", "position": 3, "name": "{Occasion}", "item": "https://www.chooseyourvenue.com/event-types/{occasion-slug}" },
    { "@type": "ListItem", "position": 4, "name": "{County}", "item": "https://www.chooseyourvenue.com/hire/uk/{county-slug}/{occasion-slug}" },
    { "@type": "ListItem", "position": 5, "name": "{City}" }
  ]
}
</script>
```

All five values are already available to the template (they render the visible breadcrumb).

---

## 5. 🟠 Heading hierarchy — element swaps only, zero visual change

Keep every existing CSS class; change only the tags.

| Current | Change to | Notes |
|---|---|---|
| Filter panel labels as `<h2>` ("Location", "Accessibility", "Search Radius", "By Occasion", "Venue Type", etc.) | `<div class="...">` or `<fieldset>`/`<legend>` | UI controls, not document structure — currently ~10 junk H2s |
| `<h5>` "Filter Venues" (appears above the H2s) | `<div class="...">` | Inverted hierarchy |
| Venue card names as `<h2>` (~24 of them) | `<h3>` | They sit under the results `<h2>` |
| Results count line | `<h2>` (keep) | With the corrected city scope from item 2 |

Result: one `<h1>` (page title), `<h2>`s for genuine sections (results, explore locally, related searches), `<h3>`s for venue names. Style the swapped elements with the same classes — pixel-identical output.

---

## 6. 🟠 ItemList schema for venues — missing

Server-rendered in the head, generated from the same venue query that renders the cards:

```html
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "ItemList",
  "name": "{Occasion} Venues in {City}",
  "numberOfItems": {count},
  "itemListElement": [
    { "@type": "ListItem", "position": 1, "item": {
        "@type": "EventVenue",
        "name": "{venue name}",
        "url": "{absolute venue URL}",
        "address": { "@type": "PostalAddress", "streetAddress": "{street}", "addressLocality": "{city}", "postalCode": "{postcode}", "addressCountry": "GB" },
        "maximumAttendeeCapacity": {capacity}
      } }
  ]
}
</script>
```

Omit `maximumAttendeeCapacity` where the field is empty rather than outputting null/0.

---

## 7. 🟡 Title + meta description refresh

**Current title:** `Christening Party Venues Southampton | Reception & Room Hire`
**Suggested:** `{count} Christening Party Venues in Southampton 2026 | ChooseYourVenue`
(count + year improves CTR on "best/venues" queries; templated so it self-updates)

**Meta description:** currently identical to the on-page intro sentence. Rewrite per template with count + USP + CTA, e.g.:
`Compare {count} christening party venues in Southampton — family-friendly hotels, function rooms & pubs for 20–300+ guests. Free to enquire.`

---

## 8. 🟡 Open Graph + Twitter Card — missing entirely

```html
<meta property="og:type" content="website">
<meta property="og:site_name" content="ChooseYourVenue.com">
<meta property="og:locale" content="en_GB">
<meta property="og:title" content="{page title without brand suffix}">
<meta property="og:description" content="{meta description}">
<meta property="og:url" content="{canonical URL}">
<meta property="og:image" content="{first venue image, big variant}">
<meta name="twitter:card" content="summary_large_image">
<meta name="twitter:title" content="{page title without brand suffix}">
<meta name="twitter:description" content="{meta description}">
<meta name="twitter:image" content="{same image}">
```

---

## 9. 🟡 Schema delivery — server-render everything

Any existing JSON-LD injected via JavaScript (FAQ etc.) should move into the server-rendered HTML. JS-injected schema is unreliably picked up. Same for lazy-loaded FAQ content: the Q&A text must be present in the initial HTML and match the FAQPage schema word-for-word.

---

## 10. 🟢 Quick tidy-ups (one-line each)

- Remove `<meta name="keywords">` (obsolete; mild low-quality signal).
- Remove the Google+ link from the footer social block sitewide (`plus.google.com/...` — service shut down 2019).
- Fix advertise-CTA copy: "Do you have a Christening Party in Southampton,Hampshire you wish to advertise?" → "Do you have a christening party venue in Southampton, Hampshire you'd like to advertise?" (missing space; you advertise a venue, not a party).
- Geo meta: replace bare `content="GB"` region with proper subdivision codes where available; ensure geo tags aren't duplicated (they were on the Exeter page).
- British spellings in any surfaced blog excerpts ("Organizing" → "Organising").

---

## Deploy order

1. Items 1–3 together (robots decision + scope fix + canonical) — this is the release that matters.
2. Items 4–6 (schema + heading swaps) — second release.
3. Items 7–10 — batch into the next routine template release.

## Post-deploy checks

- Validate schema: https://search.google.com/test/rich-results on 2–3 pages.
- Search Console URL Inspection: confirm `index,follow` pages show "Indexing allowed".
- Watch Search Console → Indexing → Pages for the city×occasion pattern over 2–4 weeks.
