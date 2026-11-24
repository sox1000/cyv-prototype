# Page rules: ChooseYourVenue and Meeting Venues

Agreed with Ben, 24 Nov 2026. This sets which page type owns which kind of
search, so CYV pages don't compete with each other or with Meeting Venues.

## The split

**ChooseYourVenue (CYV)** is the established, trusted domain, so it ranks
**venues** for every occasion: social, family and corporate (conferences,
Christmas parties, summer parties, team building, awards, gala dinners).

**Meeting Venues (MV)** ranks **individual rooms** for meetings and
training: meeting rooms, boardrooms, training rooms, day delegate rate
searches.

CYV keeps its room pages, which get traffic, but as part of each venue. It
does not run room results pages.

### What the CYV Search Console data shows (Aug to Nov 2026)

| Page type | Pages in top 1,000 | Clicks | Avg position | Decision |
|---|---|---|---|---|
| Venue results `/hire/...` | 655 | 7,410 | 35.6 | Main ranking target. Most of the gains are here. |
| Venue page `/venues/{venue}` | 146 | 1,321 | 10.1 | Keep |
| Room page `/venues/{venue}/room-hire/{room}/{id}` | 118 | 1,134 | 9.7 | Keep, and add FAQs (docs/specs/room-page-faqs.md) |
| Venue × occasion `/venue/{venue}/{eventType}` | 37 | 339 | – | Keep where it has its own content (policy engine) |
| Offers `/venue-offers/...` | 16 | 188 | – | Keep |
| Room results `/room-hire/...` | 0 | 0 | – | Retire: 301 to `/hire` |

Searches for meetings and conferences bring CYV almost nothing: 4 clicks
from 1,250 impressions ("corporate venue hire" at position 40). Christmas
searches do work. "Function room hire near me" and "pubs with function
rooms near me" (about 290 clicks) are party searches, and they stay with CYV.

## Who owns which search

| Search intent | Owner | Page type |
|---|---|---|
| {occasion} venues in {place}, including conference, Christmas party, summer party | CYV | `/hire/{country}/{county}/{city}/{category}` |
| function rooms / party rooms / rooms to hire in {place} | CYV | `/hire/...` for the closest occasion (birthday party or private party) |
| {venue name}, {venue} events | CYV | `/venues/{venue}` |
| {room name} {venue} | CYV | room page |
| {venue} Christmas party, {venue} wedding | CYV | `/venue/{venue}/{eventType}` |
| {venue} Christmas menu / package / deal | CYV | `/venue-offers/details/{id}/{seoUrl}` |
| meeting rooms / boardroom / training room hire in {place} | MV | MV room results |
| day delegate rate {place} | MV | MV room results |
| conference venues in {place} | CYV | `/hire/.../conference` (MV can list the same venues but doesn't target this phrase) |

## Rules by page type (CYV)

### Venue results `/hire/...`
- Venue cards only, never room cards. Corporate occasions use the same
  template as social ones, written for that audience (occasion config).
- Card facts for corporate occasions: largest theatre capacity, number of
  rooms, lowest day delegate rate.
- One contextual link to Meeting Venues on meeting-type occasions
  (conference, meeting, training) for people who only need one room. No
  site-wide links to MV, and no MV links on social occasions.
- Indexing follows the page policy engine (docs/page-policy-engine-spec.md).

### Venue page `/venues/{venue}`
- The main page for the venue name. Lists every room, each linking to its
  room page.
- Canonical to itself.

### Room page `/venues/{venue}/room-hire/{room}/{id}`
- Stays indexed. Canonical to itself. It must link up to the venue page and
  across to the venue's other rooms.
- Auto-generated FAQs from room data (docs/specs/room-page-faqs.md) keep each
  page unique.
- Title leads with the room and venue name, not the city plus "room hire",
  so it doesn't compete with results pages.

### Venue × occasion `/venue/{venue}/{eventType}`
- Indexed only when the venue has content specific to that occasion
  (package, menu, offer, or a description of that event). Otherwise
  noindex, canonical to `/venues/{venue}` (current policy engine states).

### Offers `/venue-offers/details/{id}/{seoUrl}`
- Targets the specific offer ("{venue} Christmas party nights 2026"), not
  the general occasion search.
- Must link to the matching venue × occasion page when one exists, so the
  two don't compete for "{venue} Christmas party".
- Expired offers: 301 to the venue × occasion page, or to the venue page if
  there isn't one.

### Retiring `/room-hire` results
- Every `/room-hire/{country}/{county}/{city}/{category}` 301s to
  `/hire/{country}/{county}/{city}/{category}`.
- If that /hire page doesn't exist, it 301s to the nearest one that does,
  in this order: the county page for that category, then the city page for
  that category, then `/event-types/{category}`.
- Remove `/room-hire` from the sitemap (DynamicSitemap.cs) and from all
  internal links.
- Before switching on: in Search Console, filter Pages by URLs containing
  "/room-hire/" and not "/venues/", for the last 12 months. Any page with
  clicks gets a hand-picked redirect target instead of the pattern.
- Room pages (`/venues/{venue}/room-hire/...`) are not affected.

## International

CYV is a global directory, but almost every paying venue is in the UK, and
the plan is to grow the network abroad.

- UK pages come first in navigation, the home page and the sitemap order.
- The international tree stays (`/hire/{country}/...`, for example
  `/hire/malta/christmas-party`, which already gets clicks). The same policy
  engine applies: an overseas page is indexed only when it reaches the
  venue threshold. Thin country and city pages stay noindex until they have
  enough venues, so they don't drag down the domain.
- Overseas pages use the same templates and URL shape as the UK, so adding
  a country is data, not new code.
- Meeting Venues follows the same approach, on its own domain.

## Checks before launch
- No `/room-hire/` results URL returns 200.
- No internal link points to a `/room-hire/` results URL.
- Every room page links to its venue page.
- Sitemap holds venue results, venue pages, room pages, indexed venue ×
  occasion pages and live offers only.
