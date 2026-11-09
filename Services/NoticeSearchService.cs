using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class NoticeSearchService : INoticeSearchService
{
    private readonly AppDbContext _db;

    // Medical synonyms for keyword search expansion
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

        // Search by keyword and synonyms
        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var raw = criteria.Keyword.Trim();
            var terms = new List<string> { raw };

            if (criteria.IncludeRelatedTerms)
            {
                foreach (var word in raw.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (RelatedTerms.TryGetValue(word, out var synonyms))
                    {
                        terms.AddRange(synonyms);
                    }
                }
            }

            var uniqueTerms = terms.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            query = query.Where(BuildSearchFilter(uniqueTerms, criteria));
        }

        // Filter by recall class
        if (criteria.Classes != null && criteria.Classes.Count > 0)
        {
            var normClasses = criteria.Classes.Select(c => c.Trim().Replace("Class ", "").Trim()).ToList();
            query = query.Where(n => normClasses.Contains(n.RecallClass));
        }

        // Filter by firm name
        if (!string.IsNullOrWhiteSpace(criteria.FirmName) && !criteria.FirmName.StartsWith("All firms", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.FirmName == criteria.FirmName);
        }

        // Filter by product code
        if (!string.IsNullOrWhiteSpace(criteria.ProductCode) && !criteria.ProductCode.StartsWith("All product", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.ProductCode == criteria.ProductCode);
        }

        // Filter by classification date range
        if (criteria.FromDate.HasValue)
        {
            query = query.Where(n => n.ClassificationDate >= criteria.FromDate.Value);
        }

        if (criteria.ToDate.HasValue)
        {
            query = query.Where(n => n.ClassificationDate <= criteria.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        // Sorting
        query = criteria.SortBy switch
        {
            "date_asc" => query.OrderBy(n => n.ClassificationDate).ThenBy(n => n.RecallNumber),
            "firm_asc" => query.OrderBy(n => n.FirmName).ThenByDescending(n => n.ClassificationDate),
            "firm_desc" => query.OrderByDescending(n => n.FirmName).ThenByDescending(n => n.ClassificationDate),
            "class_asc" => query.OrderBy(n => n.RecallClass).ThenByDescending(n => n.ClassificationDate),
            "class_desc" => query.OrderByDescending(n => n.RecallClass).ThenByDescending(n => n.ClassificationDate),
            "recall_asc" => query.OrderBy(n => n.RecallNumber),
            _ => query.OrderByDescending(n => n.ClassificationDate).ThenBy(n => n.RecallNumber)
        };

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

    private static Expression<Func<RecallNotice, bool>> BuildSearchFilter(List<string> terms, NoticeSearchCriteria criteria)
    {
        var param = Expression.Parameter(typeof(RecallNotice), "n");
        Expression? body = null;
        var contains = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

        foreach (var term in terms)
        {
            var termConst = Expression.Constant(term, typeof(string));
            var recall = Expression.Call(Expression.Property(param, nameof(RecallNotice.RecallNumber)), contains, termConst);
            Expression match = recall;

            if (criteria.SearchInFirmName)
            {
                var firm = Expression.Call(Expression.Property(param, nameof(RecallNotice.FirmName)), contains, termConst);
                match = Expression.OrElse(match, firm);
            }

            if (criteria.SearchInTradeName)
            {
                var trade = Expression.Property(param, nameof(RecallNotice.TradeName));
                var notNull = Expression.NotEqual(trade, Expression.Constant(null, typeof(string)));
                var tradeMatch = Expression.AndAlso(notNull, Expression.Call(trade, contains, termConst));
                match = Expression.OrElse(match, tradeMatch);
            }

            if (criteria.SearchInDescription)
            {
                var desc = Expression.Call(Expression.Property(param, nameof(RecallNotice.ProductDescription)), contains, termConst);
                match = Expression.OrElse(match, desc);
            }

            if (criteria.SearchInRecallReason)
            {
                var reason = Expression.Call(Expression.Property(param, nameof(RecallNotice.RecallReason)), contains, termConst);
                match = Expression.OrElse(match, reason);
            }

            body = body == null ? match : Expression.OrElse(body, match);
        }

        return Expression.Lambda<Func<RecallNotice, bool>>(body ?? Expression.Constant(true), param);
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
