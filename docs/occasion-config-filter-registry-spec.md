# Occasion Configuration & Filter Registry — Developer Specification
## One listing template + per-occasion config = all 43 occasions

Purpose: eliminate per-occasion template work. The listing template (T1/T1-R) renders entirely from an **OccasionConfig** record; filters render from a **FilterRegistry** that maps each filter to an Elasticsearch field. Adding or tuning an occasion is a data change, not a code change.

---

## 1. OccasionConfig — one record per occasion

```json
{
  "occasionKey": "funeral-wake",
  "displayName": "Funeral Wake",
  "roomTreeEligible": false,                  // per the 43-occasion sheet
  "register": "sombre",                        // "celebratory" | "corporate" | "sombre"
  "eyebrow": "Funeral wakes",
  "nearbyBand": { "enabled": true, "radiusMiles": 10, "maxResults": 4 },
  "minCapacityFloor": 20,                      // relevance floor (kills boardrooms-for-christenings)
  "filters": ["near_poi", "room_features_wake", "gathering_size", "accreditations"],
  "extraSections": ["crematoriums"],           // conditional page sections
  "defaultSort": "distance_from_poi",
  "faqSet": "wake",                            // question-bank key for auto-FAQs
  "editorialAngle": "proximity to service, short notice, buffet catering, elderly guests"
}
```

```json
{
  "occasionKey": "birthday-party",
  "displayName": "Birthday Party",
  "roomTreeEligible": false,
  "register": "celebratory",
  "nearbyBand": { "enabled": true, "radiusMiles": 15, "maxResults": 6 },
  "minCapacityFloor": 15,
  "filters": ["search_radius", "venue_type", "capacity", "facilities", "occasion_detail_birthday", "accreditations"],
  "extraSections": [],
  "defaultSort": "distance",
  "faqSet": "birthday",
  "subOccasions": ["18th", "21st", "30th", "40th", "50th", "60th", "milestone"]
  // subOccasions power the occasion_detail filter now; the policy engine may
  // mint /birthday-party/{sub}/ landing pages later where demand + inventory justify
}
```

`register` drives copy templates only (celebratory / corporate / sombre variants of the intro fallback, CTA wording, glance-card labels). `extraSections` names conditional partials (crematoriums renders only when the city has POI records of type crematorium/cemetery).

## 2. FilterRegistry — each filter group maps to Elasticsearch

```json
{
  "filterKey": "capacity",
  "label": "Capacity",
  "control": "radio",
  "esField": "max_capacity",
  "esQueryType": "range",
  "options": [
    { "label": "Up to 50", "range": [0, 50] },
    { "label": "50–100", "range": [50, 100] },
    { "label": "100–300", "range": [100, 300] },
    { "label": "300+", "range": [300, null] }
  ]
}
```

```json
{
  "filterKey": "near_poi",
  "label": "Near",
  "control": "select",
  "esQueryType": "geo_distance",
  "esField": "location",
  "optionsSource": "poi_table",                // crematoriums for wake; churches later
  "poiType": ["crematorium", "cemetery"],
  "defaultRadiusMiles": 2
}
```

```json
{
  "filterKey": "occasion_detail_birthday",
  "label": "Birthday type",
  "control": "checkbox",
  "esField": "occasion_tags",
  "esQueryType": "terms",
  "options": [ {"label":"18th","value":"birthday-18"}, {"label":"21st","value":"birthday-21"},
               {"label":"30th","value":"birthday-30"}, {"label":"40th+","value":"birthday-40plus"},
               {"label":"Kids-friendly","value":"birthday-kids"} ]
}
```

Other registry entries follow the same shape: `venue_type` (terms on style tags), `facilities` (terms), `room_features_wake` (terms: private_room, buffet, disabled_access, parking), `gathering_size` (range with wake-tuned bands), `accreditations` (term: ecosmart=true), `search_radius` (geo_distance from page geo), layout filters for the room tree (terms on room layout capacities).

**Rendering rule:** the filter panel is a loop over `OccasionConfig.filters` → registry lookup → render control → build ES bool query from the same records. One component, one query-builder, every occasion.

## 3. Results query — the banded model, one ES query shape

```
Band 1 (local):   filter: geo scope = city  AND occasion match AND capacity >= minCapacityFloor
                  sort: tier desc (premium first), then relevance/distance
                  render: premium → full cards; basic tier → compact rows
Band 2 (nearby):  filter: tier = premium AND geo_distance <= nearbyBand.radiusMiles
                  AND NOT in band 1; sort: distance asc; size: maxResults
INDEXABILITY:     PagePolicy.InventoryCount = band 1 count ONLY (never band 2)
Room tree:        same shapes against the rooms index; venue-diversity via
                  ES field collapse on venue_id (max 3 per venue, page 1)
```

## 4. POI table (crematoriums now, extensible)

```sql
CREATE TABLE Poi (
  Id int, Type varchar(30),        -- 'crematorium' | 'cemetery' | future: 'church'
  Name nvarchar(200), Address nvarchar(300),
  CitySeoUrl varchar(100), CountySeoUrl varchar(100),
  Lat float, Lng float, IsActive bit
)
```
CYV's own copy (no external dependency). Powers: the wake page section, the `near_poi` filter, per-venue "x miles from {nearest POI}" (computed), and future policy-minted "{occasion} venues near {POI}" landing pages. The church version of the same table upgrades the christening page's "near your church" from copy to feature — same mechanism, zero new code.

## 5. What the developer builds, once
1. OccasionConfig + FilterRegistry + Poi tables (+ admin-free seeding script from this spec)
2. Filter-panel partial + ES query-builder reading the registry
3. Banded-results query + the band-1-only inventory rule into PagePolicy
4. Register-keyed copy variants (3 registers × ~6 strings)
5. Conditional section partials keyed by extraSections

Everything after that — new occasions, retuned radii, birthday milestones, new filters, church proximity — is rows, not releases. That is the cost-saving: the marginal cost of occasion #44 is one JSON record.
