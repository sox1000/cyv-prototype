# CYV Redesign — Template Architecture & Build Plan

Based on the developer's page list (CYV-Page_List.xlsx, 26 page types) and the agreed approach: shared design system, Razor-ready templates, no backend changes, server-rendered schema throughout, performance-first.

---

## 1. Page inventory → template architecture

The 26 pages resolve to **13 templates + 1 shared layout + a component library**. Most "pages" are the same template fed different data.

### Layout (all pages)
Header/nav, footer, breadcrumbs partial, head partial (title/meta/canonical/OG templating), schema partials (Organization sitewide; BreadcrumbList per page).

### Templates

| # | Template | Covers pages | Notes |
|---|---|---|---|
| T1 | **Listing / results** | Venue Search (3), Room Hire (18) | The workhorse — city, county, city×occasion, and room-hire variants are one template with different query scopes and heading/meta templating. Includes filter panel, venue-card grid, local links, FAQ, editorial slot. Already designed (Southampton page). |
| T2 | **Venue detail** | Venue Detail (8) | Commercially the most important template. Gallery, rooms list, capacities, enquiry CTA, map, `EventVenue`/`LocalBusiness` schema. |
| T3 | **Venue × occasion/style** | Packages pages (9) | Venue detail variant with occasion-angled content — this is where the per-occasion AI copy lives, and the template that matters most for the four-site strategy. |
| T4 | **Room detail** | Room Detail (19) | Child of venue detail: single room, capacities by layout, hire rates. |
| T5 | **Taxonomy index** | Venue Styles (4), Event Types (6), Destinations (30) | One card-grid template, three data sources. |
| T6 | **Taxonomy detail / hub** | Venue Style (5), Event Type (7), Destinations-County (29) | Intro copy + featured venues + links into T1 listings. The `destinations/bristol` vs `hire/uk/...` overlap needs a canonical decision (see §4). |
| T7 | **Home** | Home (1) | Hero search, occasion/style entry points, featured venues, trust signals. |
| T8 | **Blog index** | Blog (11) | Card list + categories + pagination. |
| T9 | **Blog article** | Blog Detail (12) | `Article` schema, author/date, related venues cross-links (internal-linking win). |
| T10 | **Offers index + detail** | Offers (13), Offer Detail (14) | Detail page carries `Offer` schema with validity dates; expired offers need a lifecycle rule (410/redirect), same principle as events. |
| T11 | **Static/content** | About (20), Contact (21), Advertising (15), Virtual Tours (10), Ask the Expert (26) | One flexible content template with section blocks. Contact gets `ContactPage` schema. |
| T12 | **Utility** | Sitemap (22), Sitemap A–Z (25), Register (16), Enquiry/Shortlist (17), Thank You (28) | Minimal templates. Enquiry flow + Register + Thank You should be `noindex`. Thank You page is the conversion-tracking hook. |
| T13 | **Venue Directory** | Venue Directory (2) | Likely a search-entry/browse page — confirm whether it differs meaningfully from T5/T1 entry points; may merge. |

### Shared component library (used across templates)
Venue card · room card · filter panel (fieldset/legend markup per the fixes doc) · breadcrumbs · pagination · FAQ accordion · enquiry CTA block · local-links chip row · gallery · capacity/facility tag set · offer card · blog card · search bar · map embed (lazy, on interaction).

---

## 2. Performance approach (responding to the developer's image guidance)

Agreed on the principle — with one refinement: **on a venue directory, venue photography is the product**. Buyers shortlist with their eyes. So the strategy is not "few images" but "zero image cost until needed":

- **No decorative images anywhere.** All visual interest from CSS (the lime/olive system, gradients, borders) and inline SVG icons — zero requests.
- **Listing pages (T1):** one image per venue card maximum, lazy-loaded (`loading="lazy"`), explicit `width`/`height` to prevent layout shift. Cards for venues without photos get a CSS placeholder, not a stock image.
- **Venue detail (T2/T3):** hero image loads eagerly (it's the LCP element — mark it `fetchpriority="high"`); the rest of the gallery lazy-loads on scroll/interaction.
- **Format & sizing:** serve WebP/AVIF with responsive `srcset` so mobiles never download desktop-sized files. Cloudflare (already in place) can do the resizing/format negotiation at the edge — no .NET image pipeline work needed.
- **Beyond images:** self-host the two fonts with `font-display: swap` (or drop to system fonts on utility pages); one CSS file, minified; no JS frameworks on content pages — the templates I produce are HTML/CSS with only small vanilla-JS islands (filters, gallery, accordion); everything server-rendered (no client-side rendering for content, which also keeps the schema strategy intact).
- **Budget:** aim listing + venue pages at green Core Web Vitals on mobile — LCP < 2.5s, CLS < 0.1, INP < 200ms. Measure via PageSpeed Insights per template as each is delivered.

---

## 3. Build order

Sequenced by traffic/commercial impact and dependency:

1. **Design system first** — tokens file (colours/type/spacing) + shared layout + head/schema partials + core components. Everything else consumes this.
2. **T1 Listing** — highest page count, already 90% designed via the Southampton work; generalise to county/city/occasion/room-hire variants.
3. **T2 Venue detail + T4 Room detail** — the conversion pages.
4. **T3 Venue × occasion** — unlocks the per-occasion content strategy.
5. **T7 Home + T6 taxonomy hubs** — the front door and internal-linking spine.
6. **T5, T8, T9, T10** — index pages, blog, offers.
7. **T11–T13** — static and utility pages.

Each delivery = template HTML/CSS (Razor-convertible), the schema partial(s) it needs, and notes on data fields consumed — so the developer can integrate template-by-template with no big-bang release.

---

## 4. Questions / flags back to the developer

1. **`/hire/...` vs `/room-hire/...`** — two parallel URL trees for overlapping intent (row 3 vs row 18). Are both meant to be indexed? If they can serve near-identical results, we need a canonical rule or differentiated content between them.
2. **`/destinations/{place}` vs `/hire/uk/{county}/...`** — same question: which URL canonically owns "venues in Bristol"? Recommend destinations = editorial hub linking into hire = results, with distinct content on each.
3. **Venue URL split** — `/venues/{slug}` (detail) vs `/venue/{slug}/{occasion}` (packages): confirm both patterns are intended long-term (singular vs plural path), as the occasion pages are central to the multi-site plan.
4. **Row 28 (Thank You)** has no URL — need it for the enquiry-flow template and conversion tracking.
5. **For each template as we reach it:** a sample `.cshtml` view + view model (starting with T1) so deliverables land as Razor rather than HTML needing translation.

---

## 5. Immediate next step

Fetch and section-map the multi-URL examples the developer provided (Berkshire county listing, Birmingham funeral-wake, Birmingham summer-party; the three venue-detail examples; the two venue×occasion examples) to catalogue every conditional section per template — then deliver item 1 (design system) and item 2 (generalised T1) as the first drop.
