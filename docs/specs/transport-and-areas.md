# Transport and areas: integration spec (v1.5, 26 Nov 2026)

Two additions, both computed from data we already hold or can load once:

1. **Transport.** Nearest railway station, metro stop and airport for every
   venue, shown on venue pages and listing cards, used by the "Near" filter
   and in computed FAQs. Venues enter nothing.
2. **Areas.** Better pages for the parts of large cities (Shoreditch,
   Canary Wharf, the Northern Quarter). The live site already has these as
   `{city}` rows under city subdivisions (`/hire/uk/east-london/shoreditch/conference`,
   22 venues today); they lack breadcrumbs to the city, links to
   neighbouring areas and transport. No new route.

Prototype pages:

| Page | What it shows |
|---|---|
| `/venues/hilton-bournemouth` | "Getting there" block (T2), nearest station in the glance card |
| `/hire/uk/hampshire/southampton/conference` | Grouped "Near" filter, card transport line, station FAQ (T1, town) |
| `/hire/uk/east-london/shoreditch/conference` | Area variant of T1: metro-aware filter and cards, "Getting to" band, nearby areas, other areas of London |
| `/venue-directory#london` | "Popular area searches in London" row |

---

## 1. Data

### 1.1 POI table (load once, refresh quarterly)

Source: **NaPTAN** (National Public Transport Access Nodes, Department for
Transport, Open Government Licence). One download covers Great Britain.
Credit line wherever a distance appears:
"Station and airport locations: NaPTAN, Open Government Licence."

```sql
CREATE TABLE TransportPoi (
  Id          INT IDENTITY PRIMARY KEY,
  Kind        VARCHAR(10)  NOT NULL,  -- 'rail' | 'metro' | 'airport'
  Name        NVARCHAR(120) NOT NULL, -- 'Old Street', 'Heathrow Airport'
  Slug        VARCHAR(120) NOT NULL,  -- 'old-street', 'heathrow-airport'
  Lat         DECIMAL(9,6) NOT NULL,
  Lng         DECIMAL(9,6) NOT NULL,
  Lines       NVARCHAR(200) NULL,     -- 'Northern line (Bank branch)'; metro and major rail only
  Network     VARCHAR(30)  NULL,      -- 'tfl', 'metrolink', 'tyne-wear', 'glasgow-subway', 'west-midlands-metro', 'nottingham-tram', 'sheffield-supertram', 'edinburgh-trams'
  IsMajor     BIT NOT NULL DEFAULT 0, -- top ~60 stations by footfall (ORR estimates) + all airports
  NaptanCode  VARCHAR(20)  NULL
);
CREATE INDEX IX_TransportPoi_Geo ON TransportPoi (Lat, Lng);
```

From NaPTAN `StopType`:
- `RLY` → `rail` (about 2,600 stations; collapse multiple entrances to one row per station)
- `MET`/`PLT` → `metro` (Underground, Overground already appears as rail, DLR, Elizabeth line core, trams, Metrolink, Glasgow Subway)
- `AIR` → `airport`; keep only the ~25 UK airports with scheduled passenger flights

`Lines` for TfL comes from the TfL Unified API (open data, `/Line/Mode/tube,dlr,overground,elizabeth-line/StopPoints`). Elsewhere, lines are optional; leave null.

### 1.2 Airport links (curated, about 10 rows)

```sql
CREATE TABLE AirportLink (
  AirportPoiId INT NOT NULL,      -- Heathrow
  StationPoiId INT NOT NULL,      -- Liverpool Street
  Service      NVARCHAR(60) NOT NULL, -- 'Elizabeth line', 'Stansted Express', 'Gatwick Express'
  Minutes      INT NOT NULL       -- typical, rounded to 5
);
```

Used only in "Getting to {Area}" and the airport FAQ: "From Heathrow, the
Elizabeth line runs direct to Liverpool Street in about 40 minutes."

### 1.3 Venue cache (computed nightly, and on venue save)

```sql
CREATE TABLE VenueTransport (
  VenueId      INT NOT NULL,
  Rank         TINYINT NOT NULL,   -- 1 = nearest
  PoiId        INT NOT NULL,
  Miles        DECIMAL(5,2) NOT NULL,
  PRIMARY KEY (VenueId, PoiId)
);
```

Store per venue: the 3 nearest rail or metro stops within 5 miles, and the 2
nearest airports within 40 miles. Straight-line (haversine) from the venue's
lat/lng; a bounding box (±0.15° lat, ±0.25° lng for stations) before the
trig keeps it cheap. Index the same values in Elasticsearch as
`nearest_rail_miles`, `nearest_airport_miles` and a `transport_pois` nested
field (poi slug + miles) for the "Near" filter.

**Known limit:** straight-line can cross a river (a hotel at Ocean Village is
0.8 miles from Woolston, but Woolston is across the Itchen). Mitigation:
on venue pages show the nearest **major** station when a minor one is
less than 0.5 miles closer. The prototype uses Southampton Central for this
reason.

---

## 2. Display rules

| Distance | Rail / metro wording | Airport wording |
|---|---|---|
| ≤ 0.75 mi | "{n} minutes' walk" | n/a |
| ≤ 1 mi | "{miles} miles, about {n} minutes on foot" | n/a |
| 1 to 5 mi | "{miles} miles, a short taxi ride" | n/a |
| > 5 mi | hidden on cards; venue page says "{miles} miles; a taxi or lift needed" | n/a |
| ≤ 30 mi | n/a | "{miles} miles, about {n} minutes by car" |
| > 30 mi | n/a | hidden |

- Walk minutes = miles × 1.2 (route factor) ÷ 3 mph × 60, rounded.
- Drive minutes = miles × 1.4 ÷ 30 mph × 60, rounded to 5. Always "about".
- Metro cities (any `Network` stop within 1 mile): show the two nearest
  stops, metro or rail; elsewhere show the nearest rail station only.
- **Airports appear only where `OccasionConfig.showAirport = true`**
  (conference, exhibition, product launch, awards, team building, and the
  venue page itself when the venue lists any corporate occasion). Social
  occasions (christening, birthday, wake) never show airports.

### 2.1 Where each piece appears

| Surface | Content | Conditional |
|---|---|---|
| T2 venue page, "Getting there" | Rail, metro (lines), airport, car (parking from Facilities, optional "Directions" field) | Each row only when its data exists |
| T2 glance card | "Nearest station: {miles} mi" linking to #getting-there | Rail ≤ 5 mi |
| T1 card | `.venue-transport` line, max two items | As above |
| T1 "Near" filter | Optgroups "Stations" (max 5: within page radius, ≥1 venue within 1 mile, busiest first) and "Airports" (corporate only) | Choosing one re-sorts by distance from it |
| T1 "Within a short walk of a station" | Checkbox: nearest rail/metro ≤ 0.75 mi | Shown when ≥ 2 venues qualify |
| T1 FAQ | "Which {occasion} venues in {place} are near a station?" (+ airport clause on corporate) | ≥ 1 venue within 1 mile |
| T1 area "Getting to {Area}" band | Up to 4 stops within 0.75 mi of the area centroid with lines; airport row from AirportLink | Area variant only |

### 2.2 Schema

No new types. Station FAQs go into the page's existing `FAQPage`. Do not
add `publicAccess` or transit properties to `EventVenue`; Google ignores
them.

---

## 3. Areas

### 3.1 What an area is

A `{city}` row whose `{county}` is a **city subdivision**: `central-london`,
`east-london`, `north-london`, `south-london`, `west-london` today. Add a
`CityGroup` column to the county table (`London` for those five; later
`Manchester` for `greater-manchester` areas, `Birmingham`, `Glasgow`,
`Edinburgh`, `Leeds`, `Bristol` once their districts exist as rows).

Before building, list the live `{city}` rows under each subdivision with
their venue counts; merge obvious duplicates (`old-street` vs
`shoreditch` if both exist with overlapping venues: keep the one with more
search impressions, 301 the other).

### 3.2 Area page (T1 area variant)

Same template, same policy engine threshold as a town (band 2 never counts).
Differences from a town page:

1. **Breadcrumb:** Home › Event Types › {Occasion} › {CityGroup} › {Subdivision} › {Area}.
   `{CityGroup}` links to `/venue-directory#{group-slug}`.
2. **Title:** `{Count} {Occasion} Venues in {Area}, {CityGroup} {Year} | ChooseYourVenue`.
3. **Intro:** first sentence from the "Getting to" data (stations and lines
   within a 10-minute walk); second from the venue-type mix.
4. **Default sort:** "Nearest a station" in metro cities.
5. **Nearby areas:** other areas whose centroid is within 1.5 miles,
   **across subdivisions** (Shoreditch → Clerkenwell is Central London), each
   with ≥ 1 venue for this occasion, nearest first, max 6, with counts.
6. **Other areas of {CityGroup}:** the busiest areas for this occasion, max 8,
   plus "All of {CityGroup} →".

Area centroid = mean lat/lng of the area's venues (recomputed nightly),
unless a gazetteer point exists.

### 3.3 Directory hub

`/venue-directory#london` gains "Popular area searches in London": area ×
occasion pairs ranked by Search Console impressions (or enquiries where GSC
is unavailable), max 8, only pages that pass the index threshold.

---

## 4. Station and airport landing pages (phase 2)

Corporate search demand exists for "conference venues near Heathrow" and
"meeting venues near King's Cross". Proposed, not built:

- URL: `/hire/near/{poi-slug}/{occasion}` (new route; no live equivalent,
  so nothing to preserve). Example: `/hire/near/heathrow-airport/conference`.
- Only for `IsMajor` POIs and corporate occasions.
- Lists venues within 1 mile (stations) or 5 miles (airports), nearest first.
- Indexed when ≥ 5 venues qualify; otherwise noindex and out of the sitemap.
- Linked from the "Near" filter option ("See all conference venues near
  Heathrow →") and from the "Getting there" block on venue pages.

WakeVenues runs the social-occasion version at `/venues/near-station/{slug}`
(index rule: ≥ 3 venues within 3 miles and ≥ 1 within a short walk).

---

## 5. Build order

1. Load `TransportPoi` and `AirportLink`; nightly `VenueTransport` job; ES fields.
2. T2 "Getting there" + glance row.
3. T1 card line, "Near" optgroups, short-walk checkbox, station FAQ.
4. `CityGroup` column, area variant of T1, directory row.
5. Phase 2 landing pages, if Search Console shows demand.
