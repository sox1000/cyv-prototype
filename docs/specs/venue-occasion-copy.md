# Venue × occasion copy (T3): integration spec

Route: `/venue/{venue}/{eventType}`
Prototype: `venue/vue-cinema-northampton/birthday-party`

## How it works today

Each event type has one generic paragraph, used for every venue unless the
venue writes its own (Venue Caters For › {occasion} › description). These
pages rank: Vue's birthday and kids' party pages sit around positions 8 to 13
and are CYV's best cinema pages.

The weakness: the generic text is the same on hundreds of pages, and some of
it isn't true for every venue. The live Vue Northampton birthday page promises
"customisable decorations, the best live entertainment, and delicious catering
options", which a cinema may not offer. The page also shows no price, no
package and no rooms.

## The new order of copy (first one that exists wins)

1. **The venue's own text** for this occasion (Venue Caters For description).
2. **A template for this occasion and venue style**, filled with the venue's
   data. For example, "birthday party" × "Cinema & Private Screening Rooms":
   > Celebrate a birthday on the big screen at {Venue} in {Town}. {Venue} has
   > {ScreenCount} screens, from {SmallestScreen} seats for a private party
   > to {LargestScreen} for bigger groups{, with party packages from
   > £{PackageFrom} per guest}.
   One template per occasion per style group (cinema, hotel, pub, country
   house, sports club, attraction, hall). About 10 style groups × the
   occasions that matter is a few hundred short templates, written once.
3. **The current generic occasion text**, as a last resort, with any claim a
   venue might not meet removed (entertainment, decorations, catering) unless
   the matching facility is ticked.

Rules for templates:
- Only state what the data supports. A sentence about food appears only when
  "Kitchen and food onsite" or "Restaurant" is ticked; parking only when
  "Parking" is ticked; and so on.
- Every template uses at least three venue-specific facts (town, room or
  screen sizes, price, facilities), so no two pages read the same.
- No milestone lists ("30th, 40th, 50th...") unless the venue ticked those
  birthday milestones.

## Everything else on the page comes from data

- **Packages:** Venue Caters For › {occasion} › Packages from (per person),
  plus any package records. Price in the title and the first FAQ when known.
- **Rooms or screens:** rooms whose event types include this occasion,
  smallest suitable first.
- **FAQs:** the same field-based approach as room pages
  (docs/specs/room-page-faqs.md): price, what is included, capacity, private
  room, location.

## Venue prompts

In the venue dashboard, show "Your {occasion} page is using our standard
text. Add your own description and package price to stand out." Pages with
the venue's own text and a price convert better and give the page its best
chance of ranking. An AI draft from the venue's data can be offered for the
venue to edit and approve (as WakeVenues does); nothing is published without
the venue's approval.

## Indexing

Unchanged from the page policy engine: pages with occasion content (own
text, template with data, or a package) are indexed with a self-canonical.
Any page that already earns clicks stays indexed whatever its copy.

## Seasonal packages (Christmas party nights and similar)

Christmas packages are entered as free text today (dates, price and an
itinerary in one description), so nothing knows when they are out of date
and they have to be removed by hand each January.

Give packages structured dates instead:

| Field | Example |
|---|---|
| Package name | Festive Party Nights |
| Type | Shared party night / Private party |
| Price from (per person, inc VAT) | 65 |
| Dates | 4, 5, 11, 12 Dec 2026 (date picker, one or more) |
| Minimum guests (private) | 100 |
| Valid until | worked out: the last date, or set by hand |
| Description / itinerary | free text, as now |

What the site does with them:
- **After the last date, the package becomes a guide, not an offer.** It
  stays on the page, so search engines keep the content and families still
  get a sense of what the venue does and costs, but it is clearly labelled:
  > **2026 package, shown as a guide.** 2027 dates and prices are to be
  > confirmed. Enquire to hear first.
  - The name, description, itinerary and inclusions stay.
  - The dates are removed (they are in the past).
  - The price reads "From £65pp in 2026", never as a current price.
  - It moves below any live packages and loses its highlight styling.
- **New packages replace it.** As soon as the venue adds next year's
  package, last year's guide is hidden.
- **Stale guides drop off.** If no new package arrives, a guide shows for
  one more season at most (until the next season's end date, e.g. 31 Dec
  2027 for a 2026 package), then hides, so the page never shows prices more
  than a year old. The page itself stays live and indexed, with the
  "packages coming soon" note and the enquiry form.
- **Year and price in the title only while packages are live**, taken from
  the dates: "Christmas Party at Stirrups Hotel 2026 | Party Nights from
  £65pp". With only a guide, the title drops both: "Christmas Party at
  Stirrups Hotel | Party Nights & Private Parties".
- **Schema only for live packages.** A live package is an Offer with
  `validThrough` set to its last date (shared party nights can also carry
  Event markup). A guide has no Offer or Event markup, so search results
  never show an expired price or date.
- **Dates show as chips**, with past dates greyed out as the season runs,
  and "Sold out" if the venue marks one.
- **Results cards and filters** use live packages only: "Dates available"
  and "Party nights from £x" never count a guide. A venue with only a guide
  still appears in Christmas results, without a price.
- **Venue reminders:** in August, email venues whose packages are now
  guides: "Add your {year} Christmas packages", with last year's copied in
  as a draft to edit. A second nudge in September for those who haven't.

The same fields work for any seasonal package (New Year's Eve, Mother's
Day, summer parties) and match how venue offers already expire
(`/venue-offers`, ValidThrough).
