# CYE Prototype — chooseyourvenue.com redesign in test mode

A static, clickable prototype of the redesigned site. Folder paths mirror **production URLs** exactly, so every prototype page's location is also its future live URL. `{braced}` values in the HTML are data slots the developer maps to view-model fields when converting to Razor.

## What's built (v0.1)

| Template | Page | Path | Notes |
|---|---|---|---|
| T7 Home | ✅ | `/index.html` | Hero search, occasion tiles, featured venues, brand-skin note |
| T5 Taxonomy index | ✅ | `/event-types/` | Occasion grid with venue-count slots |
| T6 Occasion hub | ✅ | `/event-types/christening-party/` | UK-wide hub: locations, featured venues, guide, FAQ |
| T1 Listing | ✅ | `/hire/uk/hampshire/southampton/christening-party/` | Full listing with schema, filters, editorial, FAQ (3 exemplar cards; production loops all 24) |
| T2 Venue detail | ✅ | `/venues/hilton-bournemouth/` | Real data; Hotel JSON-LD; eager LCP hero |
| T3 Venue×occasion (indexed) | ✅ | `/venue/hilton-bournemouth/birthday-party/` | HasContent=1: real £44pp package → self-canonical |
| T3 Venue×occasion (consolidated) | ✅ | `/venue/hilton-bournemouth/christening-party/` | HasContent=0: noindex,follow + canonical→venue — **both policy states demonstrated live** |
| T4 Room detail | ✅ | `/venues/hilton-bournemouth/room-hire/phoxtrott-ballroom/` | Real capacities/dimensions; MeetingRoom JSON-LD |
| T8 Blog index | ✅ | `/blog/` | Categories (chip row), featured post, card grid, crawlable pagination; `Blog` + BreadcrumbList schema |
| T9 Blog article | ✅ | `/blog/how-to-plan-a-christening-party/` | `BlogPosting` + BreadcrumbList + FAQPage schema; author/dates, contents, inline related venues, related posts |
| T10 Offers | ⬜ TODO | — | |
| T6b Destinations | ⬜ TODO | — | Pending destinations-vs-hire canonical decision |
| T11–T13 Static/utility | ⬜ TODO | — | |

The exemplar journey: **Home → Event Types → Christening hub → Southampton listing → venue → occasion page → room** — every link a user or crawler follows exists once. The blog joins it from the News nav link on every page and from the christening hub's planning section; the article links back to the hub, the Southampton listing and Hilton Bournemouth.

## Architecture

- `assets/tokens-cyv.css` — **the brand.** Colours, fonts, radii as CSS custom properties.
- `assets/tokens-christmas.css` — demo skin proving the multi-brand model: to preview christmasvenues, change one `<link>` in any page's head. The sibling sites are this file × 3 (+ taxonomy config server-side).
- `assets/base.css` — every shared component (header, cards, filters, FAQ, packages, footer…). No colours or fonts hardcoded — consumes tokens only.
- Pages contain **structure + content + schema only.** JSON-LD blocks are the production ones (loops abbreviated with comments).
- Yellow "prototype note" boxes explain prototype-only behaviour; delete the `.proto-note` rule and elements for stakeholder demos.

## Run it locally
Root-relative paths need a server (don't open files directly):
```
cd cyv-prototype
python3 -m http.server 8080     # then open http://localhost:8080
```

## Preview site (Cloudflare, auto-deploy)
The repo is connected to Cloudflare (Workers static assets, configured in `wrangler.jsonc`; files in `.assetsignore` are not served): every push to `main` updates the preview link, and every other branch gets its own preview URL. `_headers` and `robots.txt` keep the preview out of search engines; the pages themselves still carry their production robots and canonical tags for the developer.

## Deploy to Cloudflare Pages (manual alternative)
Dashboard → Workers & Pages → Create → Pages → Upload assets → drag this folder in. You get a live `*.pages.dev` URL to review on your phone, run Lighthouse against, and share. (Or connect a Git repo for auto-deploys once this lives in one.)

## Continue building (Claude Code)
Open this folder in Claude Code and work template-by-template. Useful prompts:
- "Build the blog index and article templates (T8/T9) following the patterns in base.css and the existing pages — same header/footer, schema per the T-series docs."
- "Add the destinations hub template once I've decided the canonical rule."
- "Convert /hire/.../christening-party/index.html to a Razor view against this view model: [paste from developer]."

House rules for new pages: URL-mirroring folder + `index.html`; tokens/base stylesheets linked from every head; no colours outside tokens; every page reachable from the journey; JSON-LD server-rendered in the head; `{braced}` for anything dynamic.

## Developer handover
Each page converts to a `.cshtml` view: replace `{braced}` slots with view-model fields, turn repeated cards into `foreach` partials, and wire robots/canonical to the Page Policy Engine (see `page-policy-engine-spec.md` in the project docs). The two T3 pages are the policy engine's two output states rendered literally — use them as the acceptance criteria.

## Performance budget (verify per template before handover)
Lighthouse mobile: LCP < 2.5s · CLS < 0.1 · Performance ≥ 90. The prototype ships with: one webfont family pair (swap), eager+high-priority LCP images only, lazy everything else, explicit image dimensions, no JS.
