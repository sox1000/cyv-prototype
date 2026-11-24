> **Superseded 24 Nov 2026** by docs/page-rules-cyv-mv.md: /room-hire results are retired and 301 to /hire. Kept for background.

# Room-Tree Eligibility — All 43 Occasions
## The `RoomTreeEligible` flag, classified for sign-off

**The test:** does someone book a *room* or a *venue* for this occasion? ROOM = full `/room-hire/{geo}/{occasion}` pages exist. VENUE = those URLs 301 to the `/hire` twin. Judgement rows carry my lean + rationale — override where your enquiry data says otherwise. Demotion is a 301, promotion is a flag flip; when unsure, start VENUE (err tight — the policy engine has fewer pages to justify, and promotion is cheap later).

**Summary: 7 firm ROOM · 3 lean-ROOM judgement · 30 VENUE · 3 special.**

## ROOM — firm (the tree's core; room is the product)
| Occasion | Rationale |
|---|---|
| Conference | Archetype. "Conference room hire {city}" is head-term demand; booked by capacity/layout/DDR |
| Meeting | Archetype. Smallest, highest-frequency room booking there is |
| Training | "Training room hire" — classroom layout, daylight, AV; pure room intent |
| Seminar | Training family; theatre/classroom room booking |
| Hybrid Events | Defined by the room's AV capability; inherently room-shaped |
| Pitch Events | A presentation room booking by definition |
| Driver Training | Training family (classroom + parking); niche but room-shaped |

## JUDGEMENT — lean ROOM (recommend eligible, review after 6 months' data)
| Occasion | Lean | Rationale |
|---|---|---|
| Private Dining | ROOM | The private dining *room* is literally the product; "private dining room {city}" has real demand. Strongest of the three |
| Exhibition | ROOM | Exhibitors book *space* by floor area — closer to a room booking than a venue one |
| Film / Photo Shoot | ROOM | Location hire is space-by-space; producers browse individual spaces. Weakest lean — demote if the data's thin |

## VENUE — room-hire URLs 301 to the /hire twin
| Occasion | Note |
|---|---|
| Activity Day | Grounds/activities, not rooms |
| Anniversary Party | Family occasion |
| Awards Ceremony | Judgement-adjacent, but demand phrases are venue-shaped ("awards venue") |
| Baby Shower | Family occasion |
| Banqueting | Suites are rooms, but bookers search venues; venue tree serves it |
| Bar / Bat Mitzvah | Family occasion |
| Birthday Party | Judgement-adjacent; large parties dominate → venue |
| Charity Event | Venue-shaped demand |
| Christening Party | The original evidence case (boardrooms offered for christenings) |
| Christmas Party | Venue-shaped — **and christmasvenues.com territory (ownership map)** |
| Competitive Socialising | The venue *is* the activity |
| Concert / Music Festival | Venue/grounds |
| Corporate Event | Umbrella term; venue-shaped searches |
| Corporate Fun Day | Grounds/activities |
| Dinner Dance | Venue occasion |
| Drinks Reception | Judgement-adjacent; venue searches dominate |
| Engagement Party | Family occasion |
| Esports | Specialist venues |
| Fashion Show | Venue occasion |
| Funeral Wake | Family occasion |
| Gala Dinner | Ballroom-adjacent but "gala dinner venue" is the demand |
| Gender Reveal | Family occasion |
| Hospitality | Venue/box-shaped |
| Kids Party | Family occasion |
| Networking Event | Venue-shaped demand |
| Private Party | Family occasion |
| Product Launch | "Product launch venue" is the search; venue-shaped |
| Prom / Graduation Ball | Venue occasion |
| Summer Party | Venue/grounds |
| Team Building | Activity-shaped → venue |

## SPECIAL — neither flag fits cleanly
| Occasion | Treatment |
|---|---|
| Sustainable Events | Not an occasion — a cross-cutting theme. Recommend: no room tree, and repoint the *venue* tree page as the "sustainable venues" hub powered by the ECOsmart flag (the escalation idea from the sustainability work). One decision, two features |
| Wedding Ceremony | Venue-shaped — **and chooseyourwedding.com territory (ownership map)** |
| Wedding Reception | As above |

## Implementation reminder
One `RoomTreeEligible` column on the occasion taxonomy, read by four systems: the policy engine (page existence), the redirect layer (VENUE rows 301), the T2/T3 "Events we host" links (point at the correct tree per occasion), and the sitemap (eligible pages only). The three cross-site rows (Christmas, both Weddings) additionally await the keyword-ownership decision, which operates on the /hire tree and hubs, not this flag.

## Review trigger
After 6 months live: any lean-ROOM occasion whose room pages draw negligible impressions gets demoted to VENUE; any VENUE occasion generating "room hire"-phrased queries in GSC gets considered for promotion. The flag makes both a data decision, not a project.
