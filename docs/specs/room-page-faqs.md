# Room page FAQs (T4): integration spec

Route: `/venues/{venue}/room-hire/{room}/{id}`
Prototype: `venues/hilton-bournemouth/room-hire/phoxtrott-ballroom/27076/index.html`

Room pages earn about 1,100 clicks a quarter at an average position of 9.7
(Search Console, Aug to Nov 2026). They stay on CYV, and the FAQ block is the
main way to give each one more unique, useful text without anyone writing it.

## Rules

1. Every answer is built from a field the room or venue record already holds.
   No hand-written or AI text.
2. A question renders only when its field has data. A room with no price
   shows no price question.
3. The FAQPage JSON-LD in `<head>` is built from the same list, so the
   schema text matches the visible text word for word.
4. No question that would read the same on every room (how to enquire,
   booking fees, opening hours). Those make rooms look like duplicates.
5. Show at most 6 questions, in the order below.
6. Numbers are formatted the same way as elsewhere on the page (no
   decimals on capacities, `£600`, `341 square metres`).

## Templates

| # | Question | Answer template | Field(s) | Shown when |
|---|---|---|---|---|
| 1 | How many guests can the {Room} hold? | The {Room} holds up to {MaxCapacity} guests {largestLayout}-style. For a seated banquet it takes {Banquet}, and for a standing reception {Reception}. | MaxCapacity, meeting-type capacities | MaxCapacity > 0. Drop each extra sentence when that layout is empty. |
| 2 | What layouts can the {Room} be set up in? | {Layout} for {n}, {Layout} for {n} ... and {Layout} for {n}. | Meeting-type capacities, largest first | 2 or more layouts with a capacity |
| 3 | How much does it cost to hire the {Room}? | Room hire starts from £{PriceFrom}. The final price depends on your date, timings and catering, so ask {Venue} for an exact quote. There is no booking fee. | PriceFrom | PriceFrom > 0 |
| 4 | How big is the {Room}? | The room measures {RoomSize} square metres (about {RoomSize × 10.764, rounded to 10} sq ft). | RoomSize | RoomSize > 0 |
| 5 | What events is the {Room} suitable for? | {Venue} offers the {Room} for {event list, lower case, joined with commas and "and"}. | Room event types | 1 or more event types |
| 6 | What other rooms are there at {Venue}? | {Venue} has {OtherRoomCount} other rooms for hire, including {Name} (up to {n} guests), {Name} (up to {n}) and {Name} (up to {n}). | Other rooms at the venue, largest first, top 3 named | The venue has 1 or more other rooms. Use "1 other room" and drop "including" when there is only one. |

Notes:
- `{Room}` takes "the" in questions ("the Floyd Suite"). If the room name
  already starts with "The", don't add another.
- Question 1 uses the largest layout's name as written in the capacities
  list ("theatre-style", "standing").
- Question 5 reuses the same event list as the "Events this room suits"
  chips, so the two can never disagree.
- If question 2 would only repeat question 1 (one layout), skip question 2.

## Worked example: Stirrups Hotel, Floyd Suite (27048)

Live fields: one image, description, meeting-type capacities, price from,
room size, max capacity, event types, other rooms, enquiry form. That is
enough for all six questions, with no new data needed.

## Optional fields (render only if CYV adds them later)

The prototype also shows a layout-tagged gallery, a priced hire-option table
and facility rows (private bar, drinks reception area). The live data has
one image and a single "price from", so these sections stay hidden until the
fields exist. If facility fields are added, a seventh question can follow:
"Does the {Room} have its own bar?", shown only when that field is true.
