using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class NoticeSearchService : INoticeSearchService
{
    private readonly AppDbContext _db;

    // Dictionary of related medical/technical terms for "Include related terms" feature
    private static readonly Dictionary<string, string[]> RelatedTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        { "leak", new[] { "leakage", "leaks", "compromised seal", "fluid loss" } },
        { "software", new[] { "firmware", "bug", "reboot", "shutdown", "code" } },
        { "battery", new[] { "power", "cell", "backup", "charging", "voltage" } },
        { "cuff", new[] { "inflatable", "tube", "balloon", "flange", "cannula" } },
        { "valve", new[] { "switch", "shuttle", "stopcock", "flowmeter" } },
        { "ventilation", new[] { "ventilator", "resuscitation", "breathing", "hypoventilation", "hypoxia" } },
        { "sterile", new[] { "sterility", "endotoxin", "contamination", "unsterile", "pouch" } },
        { "tracheostomy", new[] { "tracheotomy", "laryngectomy", "airway", "cannula", "bivona", "shiley" } }
    };

    public NoticeSearchService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<RecallNotice>> SearchNoticesAsync(NoticeSearchCriteria criteria)
    {
        var query = _db.RecallNotices.AsNoTracking().AsQueryable();

        // 1. Keyword search
        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var rawKeyword = criteria.Keyword.Trim();
            var searchTerms = new List<string> { rawKeyword };

            if (criteria.IncludeRelatedTerms)
            {
                foreach (var term in rawKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (RelatedTerms.TryGetValue(term, out var synonyms))
                    {
                        searchTerms.AddRange(synonyms);
                    }
                }
            }

            searchTerms = searchTerms.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var param = System.Linq.Expressions.Expression.Parameter(typeof(RecallNotice), "n");
            System.Linq.Expressions.Expression? combined = null;
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

            foreach (var term in searchTerms)
            {
                var termConst = System.Linq.Expressions.Expression.Constant(term, typeof(string));

                var recallNumProp = System.Linq.Expressions.Expression.Property(param, nameof(RecallNotice.RecallNumber));
                System.Linq.Expressions.Expression termExpr = System.Linq.Expressions.Expression.Call(recallNumProp, containsMethod, termConst);

                if (criteria.SearchInFirmName)
                {
                    var firmProp = System.Linq.Expressions.Expression.Property(param, nameof(RecallNotice.FirmName));
                    termExpr = System.Linq.Expressions.Expression.OrElse(termExpr, System.Linq.Expressions.Expression.Call(firmProp, containsMethod, termConst));
                }

                if (criteria.SearchInTradeName)
                {
                    var tradeProp = System.Linq.Expressions.Expression.Property(param, nameof(RecallNotice.TradeName));
                    var notNull = System.Linq.Expressions.Expression.NotEqual(tradeProp, System.Linq.Expressions.Expression.Constant(null, typeof(string)));
                    var callContains = System.Linq.Expressions.Expression.Call(tradeProp, containsMethod, termConst);
                    termExpr = System.Linq.Expressions.Expression.OrElse(termExpr, System.Linq.Expressions.Expression.AndAlso(notNull, callContains));
                }

                if (criteria.SearchInDescription)
                {
                    var descProp = System.Linq.Expressions.Expression.Property(param, nameof(RecallNotice.ProductDescription));
                    termExpr = System.Linq.Expressions.Expression.OrElse(termExpr, System.Linq.Expressions.Expression.Call(descProp, containsMethod, termConst));
                }

                if (criteria.SearchInRecallReason)
                {
                    var reasonProp = System.Linq.Expressions.Expression.Property(param, nameof(RecallNotice.RecallReason));
                    termExpr = System.Linq.Expressions.Expression.OrElse(termExpr, System.Linq.Expressions.Expression.Call(reasonProp, containsMethod, termConst));
                }

                combined = combined == null ? termExpr : System.Linq.Expressions.Expression.OrElse(combined, termExpr);
            }

            if (combined != null)
            {
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<RecallNotice, bool>>(combined, param);
                query = query.Where(lambda);
            }
        }

        // 2. Class filter
        if (criteria.Classes != null && criteria.Classes.Count > 0)
        {
            var normClasses = criteria.Classes.Select(c => c.Trim().Replace("Class ", "").Trim()).ToList();
            query = query.Where(n => normClasses.Contains(n.RecallClass));
        }

        // 3. Firm name filter
        if (!string.IsNullOrWhiteSpace(criteria.FirmName) && !criteria.FirmName.StartsWith("All firms", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.FirmName == criteria.FirmName);
        }

        // 4. Product code filter
        if (!string.IsNullOrWhiteSpace(criteria.ProductCode) && !criteria.ProductCode.StartsWith("All product", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.ProductCode == criteria.ProductCode);
        }

        // 5. Date range
        if (criteria.FromDate.HasValue)
        {
            query = query.Where(n => n.ClassificationDate >= criteria.FromDate.Value);
        }

        if (criteria.ToDate.HasValue)
        {
            query = query.Where(n => n.ClassificationDate <= criteria.ToDate.Value);
        }

        // 6. Total count before paging
        var totalCount = await query.CountAsync();

        // 7. Sorting
        query = criteria.SortBy switch
        {
            "date_asc" => query.OrderBy(n => n.ClassificationDate).ThenBy(n => n.RecallNumber),
            "firm_asc" => query.OrderBy(n => n.FirmName).ThenByDescending(n => n.ClassificationDate),
            "firm_desc" => query.OrderByDescending(n => n.FirmName).ThenByDescending(n => n.ClassificationDate),
            "class_asc" => query.OrderBy(n => n.RecallClass).ThenByDescending(n => n.ClassificationDate),
            "class_desc" => query.OrderByDescending(n => n.RecallClass).ThenByDescending(n => n.ClassificationDate),
            "recall_asc" => query.OrderBy(n => n.RecallNumber),
            _ => query.OrderByDescending(n => n.ClassificationDate).ThenBy(n => n.RecallNumber) // date_desc default
        };

        // 8. Paging
        var pageNumber = Math.Max(1, criteria.PageNumber);
        var pageSize = criteria.PageSize > 0 ? Math.Min(criteria.PageSize, 100) : 20;

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<RecallNotice>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<List<string>> GetAllFirmsAsync()
    {
        return await _db.RecallNotices
            .AsNoTracking()
            .Select(n => n.FirmName)
            .Where(f => !string.IsNullOrEmpty(f))
            .Distinct()
            .OrderBy(f => f)
            .ToListAsync();
    }

    public async Task<List<string>> GetAllProductCodesAsync()
    {
        return await _db.RecallNotices
            .AsNoTracking()
            .Select(n => n.ProductCode)
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .OrderBy(p => p)
            .ToListAsync();
    }

    public async Task<RecallNotice?> GetNoticeByIdAsync(int id)
    {
        return await _db.RecallNotices
            .Include(n => n.ReviewNotices)
                .ThenInclude(rn => rn.Review)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<RecallNotice?> GetNoticeByRecallNumberAsync(string recallNumber)
    {
        return await _db.RecallNotices
            .Include(n => n.ReviewNotices)
                .ThenInclude(rn => rn.Review)
            .FirstOrDefaultAsync(n => n.RecallNumber == recallNumber);
    }

    public async Task<List<RecallNotice>> GetNoticesByIdsAsync(IEnumerable<int> ids)
    {
        return await _db.RecallNotices
            .AsNoTracking()
            .Where(n => ids.Contains(n.Id))
            .ToListAsync();
    }
}
