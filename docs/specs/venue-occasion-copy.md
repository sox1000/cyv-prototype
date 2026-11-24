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
- **Hide automatically** the day after the last date (or Valid until).
  No manual clean-up.
- **The page never goes empty.** When the season's packages have expired,
  the Christmas page stays live and indexed (it keeps its rankings) and
  shows: "{Venue}'s {next year} Christmas packages are coming soon. Enquire
  now to be first to hear." with the enquiry form.
- **Year in the title only while packages are live**, taken from the dates:
  "Christmas Party at Stirrups Hotel 2026 | Party Nights from £65pp".
  Without live packages, the title drops the year and the price.
- **Dates show as chips**, with past dates greyed out as the season runs,
  and "Sold out" if the venue marks one.
- **Schema:** each package is an Offer with `validThrough` set to the last
  date; shared party nights can also carry Event markup with the dates.
- **Results cards and filters** (Christmas: shared or private party nights,
  dates available) read the same fields, so expired nights never show.
- **Venue reminders:** in August, email venues with expired Christmas
  packages: "Add your {year} Christmas packages", with last year's copied
  in as a draft to edit. A second nudge in September for those who haven't.

The same fields work for any seasonal package (New Year's Eve, Mother's
Day, summer parties) and match how venue offers already expire
(`/venue-offers`, ValidThrough).
