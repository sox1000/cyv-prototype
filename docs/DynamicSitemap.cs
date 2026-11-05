// ============================================================================
// DYNAMIC SITEMAP — ASP.NET Core (.NET 6+)
// Generated from the Page Policy Engine: only Status='indexed' pages appear,
// so sitemap, robots directives and canonicals can never disagree.
//
// Endpoints:
//   /sitemap.xml                    -> sitemap index (lists child sitemaps)
//   /sitemaps/{section}-{page}.xml  -> child sitemaps, max 45k URLs each
//
// Sections: static | hubs | listings | venues | venue-occasions | rooms | blog | offers
//
// Registration (Program.cs):
//   builder.Services.AddMemoryCache();
//   builder.Services.AddScoped<ISitemapRepository, SitemapRepository>();
//   builder.Services.AddScoped<SitemapService>();
//   // + MapControllers() picks up SitemapController
//
// robots.txt (per brand host):
//   Sitemap: https://www.chooseyourvenue.com/sitemap.xml
//
// After deploy: submit /sitemap.xml in Search Console (FIX 89) and expire any
// legacy sitemap files still registered there.
// ============================================================================

using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Cye.Web.Sitemaps;

// ---------------------------------------------------------------------------
// Models
// ---------------------------------------------------------------------------
public sealed record SitemapUrl(string Path, DateTime? LastMod, string? ChangeFreq = null, double? Priority = null);
public sealed record SitemapSection(string Name, int PageCount, DateTime? LastMod);

// ---------------------------------------------------------------------------
// Repository — the ONLY part that touches your schema. Example SQL is written
// against the PagePolicy table from the policy-engine spec plus your content
// tables; adapt names to the real schema. Works with Dapper or EF raw SQL.
// ---------------------------------------------------------------------------
public interface ISitemapRepository
{
    Task<int> CountAsync(string section, int brandId);
    Task<IReadOnlyList<SitemapUrl>> GetPageAsync(string section, int brandId, int skip, int take);
    Task<DateTime?> LastModifiedAsync(string section, int brandId);
}

/* Example queries per section (Dapper-style), all policy-driven:

   -- listings: /hire and /room-hire pages the policy engine marks indexed
   SELECT '/' + PageType + '/' + GeoKey + ISNULL('/' + OccasionKey,'') AS Path,
          ComputedAt AS LastMod
   FROM   PagePolicy
   WHERE  BrandId=@brandId AND PageType IN ('hire','room-hire') AND Status='indexed'
   ORDER  BY Path OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;

   -- venues: /venues/{slug}; lastmod = the venue record's UpdatedAt
   SELECT '/venues/' + v.SeoUrl, v.UpdatedAt
   FROM   Venue v WHERE v.BrandId=@brandId AND v.IsPublished=1
   ORDER  BY v.SeoUrl OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;

   -- venue-occasions: ONLY HasContent=1 rows (the indexed T3 state)
   SELECT '/venue/' + v.SeoUrl + '/' + p.OccasionKey,
          GREATEST(v.UpdatedAt, ec.UpdatedAt)          -- SQL Server: use CASE/IIF
   FROM   PagePolicy p
   JOIN   Venue v ON v.Id = p.VenueId
   LEFT  JOIN EditorialContent ec ON ec.VenueId=v.Id AND ec.OccasionKey=p.OccasionKey
   WHERE  p.BrandId=@brandId AND p.PageType='venue-occasion' AND p.Status='indexed'
   ORDER  BY 1 OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;

   -- rooms: /venues/{slug}/room-hire/{roomSlug}/{roomId}
   -- blog:  published posts, lastmod = post UpdatedAt
   -- offers: ONLY offers whose expiry >= today (expired offers leave the
   --         sitemap the day they lapse — the offer-lifecycle rule for free)
   -- hubs:  /event-types, /event-types/{occasion}, /venue-styles/…, /destinations/…
   -- static: /, /about-us, /contact-us, /advertising, etc. (small fixed list)
*/

// ---------------------------------------------------------------------------
// Service — pagination, caching, XML writing
// ---------------------------------------------------------------------------
public sealed class SitemapService
{
    public const int PageSize = 45_000;                    // headroom under the 50k spec limit
    private static readonly TimeSpan CacheFor = TimeSpan.FromHours(6);
    private static readonly string[] Sections =
        { "static", "hubs", "listings", "venues", "venue-occasions", "rooms", "blog", "offers" };

    private readonly ISitemapRepository _repo;
    private readonly IMemoryCache _cache;

    public SitemapService(ISitemapRepository repo, IMemoryCache cache)
        => (_repo, _cache) = (repo, cache);

    public async Task<string> BuildIndexAsync(string baseUrl, int brandId)
    {
        return await _cache.GetOrCreateAsync($"smi:{brandId}", async e =>
        {
            e.AbsoluteExpirationRelativeToNow = CacheFor;
            var sections = new List<SitemapSection>();
            foreach (var s in Sections)
            {
                var count = await _repo.CountAsync(s, brandId);
                if (count == 0) continue;
                sections.Add(new SitemapSection(s,
                    (int)Math.Ceiling(count / (double)PageSize),
                    await _repo.LastModifiedAsync(s, brandId)));
            }

            var sb = new StringBuilder();
            using var w = CreateWriter(sb);
            w.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");
            foreach (var s in sections)
                for (var p = 1; p <= s.PageCount; p++)
                {
                    w.WriteStartElement("sitemap");
                    w.WriteElementString("loc", $"{baseUrl}/sitemaps/{s.Name}-{p}.xml");
                    if (s.LastMod is { } lm) w.WriteElementString("lastmod", lm.ToString("yyyy-MM-dd"));
                    w.WriteEndElement();
                }
            w.WriteEndElement();
            w.Flush();
            return sb.ToString();
        }) ?? "";
    }

    public async Task<string?> BuildSectionAsync(string baseUrl, int brandId, string section, int page)
    {
        if (!Sections.Contains(section) || page < 1) return null;
        return await _cache.GetOrCreateAsync($"sm:{brandId}:{section}:{page}", async e =>
        {
            e.AbsoluteExpirationRelativeToNow = CacheFor;
            var urls = await _repo.GetPageAsync(section, brandId, (page - 1) * PageSize, PageSize);
            if (urls.Count == 0) return null;

            var sb = new StringBuilder();
            using var w = CreateWriter(sb);
            w.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            foreach (var u in urls)
            {
                w.WriteStartElement("url");
                w.WriteElementString("loc", baseUrl + u.Path);
                if (u.LastMod is { } lm) w.WriteElementString("lastmod", lm.ToString("yyyy-MM-dd"));
                if (u.ChangeFreq is not null) w.WriteElementString("changefreq", u.ChangeFreq);
                if (u.Priority is { } pr) w.WriteElementString("priority", pr.ToString("0.0"));
                w.WriteEndElement();
            }
            w.WriteEndElement();
            w.Flush();
            return sb.ToString();
        });
    }

    private static XmlWriter CreateWriter(StringBuilder sb) =>
        XmlWriter.Create(sb, new XmlWriterSettings
        {
            Indent = false,
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        });
}

// ---------------------------------------------------------------------------
// Controller — brand resolved from Host header (multi-brand ready: same code
// serves all five sites; BrandResolver maps hostname -> BrandId + canonical
// base URL, exactly as the page controllers will).
// ---------------------------------------------------------------------------
[ApiController]
public sealed class SitemapController : ControllerBase
{
    private readonly SitemapService _svc;
    public SitemapController(SitemapService svc) => _svc = svc;

    private (int brandId, string baseUrl) Brand()
    {
        // TODO: replace with the shared BrandResolver when multi-brand lands.
        // For now CYV only:
        return (1, "https://www.chooseyourvenue.com");
    }

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Index()
    {
        var (brandId, baseUrl) = Brand();
        return Content(await _svc.BuildIndexAsync(baseUrl, brandId), "application/xml", Encoding.UTF8);
    }

    [HttpGet("/sitemaps/{section}-{page:int}.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Section(string section, int page)
    {
        var (brandId, baseUrl) = Brand();
        var xml = await _svc.BuildSectionAsync(baseUrl, brandId, section, page);
        return xml is null ? NotFound() : Content(xml, "application/xml", Encoding.UTF8);
    }
}

// ============================================================================
// Design notes for the developer
// ----------------------------------------------------------------------------
// 1. POLICY-DRIVEN, ALWAYS. Listings and venue-occasion sections read PagePolicy
//    with Status='indexed'. Never query the venue/geo tables directly for these,
//    or the sitemap will drift from what the pages actually serve.
//
// 2. LASTMOD MUST BE HONEST. Only emit lastmod when a real content change
//    happened (venue edit, editorial generation, package change). Google
//    ignores lastmod from sites that stamp everything "today" — and honest
//    lastmod is what makes the AI-content pipeline's regenerations visible
//    (pipeline writes EditorialContent.UpdatedAt -> lastmod moves -> recrawl).
//
// 3. CACHING. 6h in-memory cache + 1h response cache is plenty; the policy
//    table recomputes nightly. To bust cache after the nightly job or a bulk
//    pipeline run, evict keys "smi:*"/"sm:*" (or just restart the app slot).
//    Cloudflare will also edge-cache these responses — set the page rule to
//    respect the response cache header.
//
// 4. LIMITS. 45k URLs/child keeps headroom under the 50,000-URL / 50MB spec
//    limits. At ~96k indexed pages today you'll see ~2 pages for the largest
//    sections; the index scales automatically as the policy engine prunes or
//    grows the set.
//
// 5. WHAT'S DELIBERATELY EXCLUDED: noindex pages (thin cities, content-less
//    T3s), redirect stubs, enquiry/register/thank-you, parameterised URLs,
//    expired offers. If it's not Status='indexed' (or on the static list),
//    it does not exist as far as the sitemap is concerned.
//
// 6. MULTI-BRAND. When the brand table lands, Brand() reads the resolver and
//    this same controller serves christmasvenues/chooseyourwedding/
//    chooseyourevent sitemaps from their own hosts with zero extra code.
// ============================================================================
