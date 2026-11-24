# Filter field mapping: CMS fields to listing filters

Companion to docs/occasion-config-filter-registry-spec.md. That spec says
each occasion has its own list of filters. This one says which **existing
CMS field** feeds each filter, so filters can launch from today's data.

Source: CMS venue edit screens (Venue Caters For, Venue Facilities, Venue
Types, Rates & capacities, Bedroom Facilities), seen 24 Nov 2026.

## Rules

1. A filter group renders only when at least 60% of venues on that page
   have data for it. Otherwise it is hidden, so families never get thin or
   misleading results.
2. Occasion figures come first. Guests and price use the venue's figures
   for **this** occasion (Venue Caters For › {occasion} › Max Capacity /
   Packages from per person). Fall back to the largest room capacity only
   when the occasion figure is empty. Never fall back for price.
3. Options inside a group show only when they would return at least one
   venue.
4. Merged fields (see "Tidy-up") are read with OR until Ajay merges them.

## Mapping

| Filter | CMS field(s) | Query |
|---|---|---|
| Guests / Delegates | Venue Caters For › {occasion} › Max Capacity; fallback: largest room capacity (theatre for corporate) | range |
| Price per head | Venue Caters For › {occasion} › Packages from (per person) | range |
| Day delegate rate | Rates & capacities › Day delegate rates from | range |
| 24hr delegate rate | Rates & capacities › 24hr delegate rates from | range (residential conferences only) |
| Room hire from | Rates & capacities › Room hire from | range |
| Bedrooms on site | Total Bedrooms > 0 OR Venue Facilities › Accommodation | term |
| Breakout rooms | Total Event Spaces ≥ 2 | range |
| Venue type | Venue Types | terms |
| Birthday milestone | Venue Caters For › Birthday Party dropdown | terms |
| Children's facilities | Venue Facilities › Children's Facilities | term |
| Disabled access | Venue Facilities › Disabled Access | term |
| Parking | Parking OR Carpark | term |
| Garden or outdoor space | Outdoor Space OR Onsite gardens | term |
| Food on site | Kitchen and food onsite OR Restaurant | term |
| Bar | Bar | term |
| Entertainment | Entertainment | term |
| Outdoor activities | Outdoor Activities | term |
| AV equipment | AV Equipment Hire | term |
| ECOsmart | ECOsmart Venues | term |
| Near church / crematorium / station / airport | Computed: venue postcode vs POI table (no CMS field) | geo_distance |
| Search radius | Computed from page location | geo_distance |

## Filter sets per occasion (OccasionConfig.filters, in order)

| Occasion group | Filters |
|---|---|
| Christening, baby shower, naming day, gender reveal | Near your church (christening only), Guests, Price per head, For families (children's facilities, garden or outdoor space, food on site, parking, disabled access), Venue type, ECOsmart |
| Birthday, anniversary, engagement, 18th/21st | Search radius, Birthday milestone (birthday only), Guests, Price per head, Facilities (bar, entertainment, food on site, parking, disabled access), Venue type, ECOsmart |
| Kids' party | Search radius, Guests, Price per head, Facilities (children's facilities, outdoor activities, food on site, parking), Venue type |
| Funeral wake | Near crematorium, Guests, Price per head, Facilities (food on site, bar, parking, disabled access), Venue type, ECOsmart |
| Christmas party, dinner dance, gala dinner, awards | Search radius, Guests, Price per head, Facilities (entertainment, bar, bedrooms on site, parking, disabled access), Venue type, ECOsmart |
| Summer party, corporate fun day, activity day | Search radius, Guests, Price per head, Facilities (garden or outdoor space, outdoor activities, bar, parking), Venue type, ECOsmart |
| Conference, exhibition, corporate event | Search radius, Near station or airport, Delegates, Day delegate rate, Facilities (bedrooms on site, breakout rooms, AV equipment, parking, disabled access), Venue type, ECOsmart |

Prototype exemplars: `/hire/uk/hampshire/southampton/christening-party`
and `/hire/uk/hampshire/southampton/conference`.

## Venue cards use the same occasion figures

Card tags on an occasion page read the occasion fields too:
`{Occasion} up to {Max Capacity} guests` · `Packages from £{price}pp` ·
the first matching facility from that occasion's set. On conference pages:
`{n} theatre` · `{Total Event Spaces} rooms` · `DDR from £{rate}pp`. The
card description uses the venue's text for that occasion (Venue Caters For ›
{occasion} description) when present, otherwise the main description.

## Not in the CMS yet (filters stay hidden until added)

New yes/no ticks on Venue Facilities:
- Late licence (after midnight)
- Dance floor
- Private-hire room (exclusive use of a room)
- Daytime hire available
- Short-notice bookings (within 7 days), mainly for wakes
- Christmas: shared party nights / private parties (two ticks under Christmas Party)

The POI tables (churches, stations and airports) are needed for the "near"
filters. Crematoriums can be copied from WakeVenues (318 records).

## Tidy-up for Ajay (CMS)

- Merge duplicates: Gym (listed twice), Parking + Carpark, Outdoor Space +
  Onsite gardens, Meeting Room + Meetings Facilities, Events + Events Manager.
- Hide retired options: CovidSafe Events, Covid Compliant Venues, Fax,
  iPod Docking.
- Bedroom Facilities are not used as filters. Total Bedrooms is enough.
- Currency: rates carry a currency (e.g. € (Euro)). Filters and cards show
  the venue's own currency; price filter bands are per country.
