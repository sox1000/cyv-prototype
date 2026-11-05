# Page Policy Engine — Specification
## Inventory-driven page existence, indexability and canonicalisation
For the developer: one mechanism replacing per-page decisions across all listing trees, controllable via data. Resolves: zero-inventory pages (international bloat, ~15% of index), thin-city pages, T3 boilerplate mass (~56% of index), and the legacy-straggler class permanently.

---

## 1. Concept
A nightly-computed policy table answers three questions for every potential listing URL:
1. **Does this page exist?** (200 vs 301-to-parent)
2. **Is it indexable?** (index,follow vs noindex,follow)
3. **What is its canonical?** (self vs parent)

Controllers consult the table; nobody hand-manages pages again. Thresholds live in config (per brand), so policy changes are data changes.

## 2. Policy table
Computed nightly (SQL job or scheduled task) from the venue/room tables:

```sql
CREATE TABLE PagePolicy (
    PageType        varchar(20),   -- 'hire' | 'room-hire' | 'venue-occasion'
    GeoKey          varchar(200),  -- 'uk/hampshire/southampton' (null for venue-occasion)
    OccasionKey     varchar(100),  -- 'christening-party' (null for plain geo pages)
    VenueId         int NULL,      -- for venue-occasion rows
    InventoryCount  int,           -- venues (hire) or rooms (room-hire) matching scope
    HasContent      bit,           -- package OR generated editorial exists (T3/T1)
    Status          varchar(20),   -- 'indexed' | 'reachable-noindex' | 'redirect'
    RedirectTarget  varchar(500) NULL,  -- nearest ancestor with Status='indexed'
    ComputedAt      datetime
)
```

## 3. Rules (initial thresholds — config, not code)

### Listing pages (/hire and /room-hire trees)
| InventoryCount | Status | Behaviour |
|---|---|---|
| 0 | redirect | 301 → nearest ancestor with inventory (city→county→country→occasion hub). Never 404/500. |
| 1–4 | reachable-noindex | 200, robots `noindex,follow`, canonical self. Linked normally so venues aren't orphaned. |
| ≥5 | indexed | 200, `index,follow`, canonical self, eligible for editorial pipeline. |

Per-occasion overrides: room-hire pages exist only for room-appropriate occasions (per the /hire-vs-/room-hire decision doc); family occasions 301 to the /hire twin regardless of count.

### T3 venue×occasion pages
| Condition | Status | Behaviour |
|---|---|---|
| HasContent = 1 (package OR generated occasion copy) | indexed | Self-canonical, index,follow |
| HasContent = 0 | reachable-noindex + canonical → /venues/{slug} | Page still serves (users arrive from venue page); Google consolidates to the venue |

As the content pipeline generates copy, HasContent flips and pages earn independence automatically — the safe gradual rollout.

### International
Same rules, same table — no special casing needed. Fisk, Missouri has 0 venues → redirect to /hire/usa (or the highest US level with inventory; if none, the venue directory). If international inventory is ever acquired, pages appear automatically.

## 4. Implementation notes (.NET)
- **Lookup**: action filter / middleware on listing + T3 controllers: fetch PagePolicy row (cache in memory, refresh on recompute); emit 301, or set robots/canonical view-model fields consumed by the head partial (the same partial from the T1 drop).
- **Recompute**: nightly job; also triggerable on venue publish/unpublish so new inventory surfaces same-day.
- **Sitemaps**: generated FROM this table — Status='indexed' rows only. Sitemap and robots directives can never disagree again.
- **Internal links**: "Explore locally" link lists filter to Status='indexed' targets (linking noindexed/redirecting pages wastes crawl).
- **Safety**: never combine with robots.txt Disallow (established principle); redirect chains forbidden — RedirectTarget must itself have Status='indexed'.
- **Monitoring**: expose counts per status as a simple report (or hub page): total indexed / noindexed / redirecting per brand. This is Ben's control panel.

## 5. Rollout order
1. Compute table read-only; review counts (expect: indexed total drops well below 96k — intended).
2. Enable for international trees first (lowest risk, biggest bloat).
3. Enable listing thresholds UK-wide.
4. Enable T3 conditional canonical (largest index change — watch GSC for 4–6 weeks).
5. Wire sitemaps to the table.

## 6. Portfolio extension
Add BrandId to the table + per-brand threshold config. christmasvenues/chooseyourwedding/chooseyourevent run the same engine filtered to their occasion sets. The HUB reads across brands. One mechanism, one control surface, five sites.
