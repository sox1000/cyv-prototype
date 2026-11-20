# CYE Prototype — project brief for Claude Code

Static clickable prototype of the chooseyourvenue.com redesign. Folder paths mirror
production URLs. This repo is the design handover for a .NET/SQL Server/Elasticsearch
integration by external developers — every page converts to a Razor view.

## House rules (do not deviate)
- Every page: URL-mirroring folder + index.html; link /assets/tokens-cyv.css then /assets/base.css
- NO colours/fonts outside tokens files; components go in base.css, never per-page (small page-specific styles allowed in a <style> block only while experimental, then migrate)
- `{braced}` values = data slots for view models; realistic sample values on screen with the slot name in an HTML comment
- All schema (JSON-LD) server-rendered in <head>; abbreviated loops marked with comments
- British English everywhere; occasion register per docs/occasion-config-filter-registry-spec.md (celebratory/corporate/sombre)
- Conditional sections: render only when data exists (see room page FAQs/pricing pattern)
- Every new page must be linked from the click journey the same session it's created
- Performance: no JS, one font pair, eager+fetchpriority only on LCP images, lazy+dimensions everywhere else
- Yellow .proto-note boxes explain prototype-only behaviour
- URLs match the live .NET routes exactly (sox1000/chooseyourvenue, RouteConfigExtensions.cs + controller attributes), no trailing slashes, so launch keeps every indexed URL: /hire/{country}/{county}/{city}/{category}, /venues/{venue}, /venues/{venue}/room-hire/{room}/{id}, /venue/{venue}/{eventType}, /room-hire/{country}/{county}/{city}/{category}, /event-types/{category}, /blog, /blog/{category}, /blog/{newsId}/{slug}, /about-us, /contact-us, /advertising, /venue-styles, /venue-offers, /venue-offers/{country}/{county}/{city}, /venue-offers/details/{id}/{seoUrl}. Blog page 2+ proposed as /blog?page={n} (live has load-more only)

## Build status
DONE: home / event-types index / christening hub / venue listing (banded results,
glance card, ECOsmart) / room listing (conference) / wake listing (crematoriums,
sombre register) / venue detail (12 rooms, ECOsmart, sticky aside) / venue×occasion
×2 (indexed + noindex policy states) / room detail (layout gallery, pricing, auto-FAQs) /
blog index T8 (/blog) + article T9 (/blog/612/how-to-plan-a-christening-party) — v0.8 URLs match live routes /
offers index T10a (/venue-offers) + detail T10b (/venue-offers/details/4127/christmas-party-nights-2026) — v0.9

TODO (in order): destinations hub (T6b — pending canonical decision) · static set (about, contact,
advertising) · enquiry flow + thank-you (noindex) · per-template integration specs
into docs/specs/ (fields consumed, conditionals, ES queries per docs/occasion-config
spec, schema blocks)

## Key docs (in /docs)
occasion-config-filter-registry-spec.md — filters/occasions are CONFIG, not code
page-policy-engine-spec.md — indexability rules; band-2 never counts
room-tree-eligibility-43-occasions.md — which occasions get /room-hire pages
urgent-seo-fixes + template-architecture — background/audit context
DynamicSitemap.cs — sitemap implementation for the developer

## Deploy
Cloudflare (Workers & Pages) — drag folder into New deployment, or connect this
repo to Pages for auto-deploy. Verify with hard refresh.
