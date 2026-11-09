using ANDtr.Web.Models;
using ANDtr.Web.Services;
using Xunit;

namespace ANDtr.Tests;

public class NoticeSearchServiceTests
{
    private async Task SeedSampleNotices(ANDtr.Web.Data.AppDbContext db)
    {
        db.RecallNotices.AddRange(
            new RecallNotice
            {
                Id = 1,
                RecallNumber = "Z-1706-2015",
                TradeName = "Flow-i Anesthesia System",
                ProductCode = "BSZ",
                ProductCodeName = "GAS-MACHINE, ANESTHESIA",
                RegulationNumber = "868.5160",
                WebAddress = "https://example.com/1",
                FirmName = "Maquet Cardiovascular Us Sales, Llc",
                RecallClass = "1",
                ClassificationDate = new DateTime(2015, 6, 8),
                ProductDescription = "Inhalation anesthesia system for neonates to adults",
                RecallReason = "Patient cassette can get dislodged which could result in gas leakage within circuit."
            },
            new RecallNotice
            {
                Id = 2,
                RecallNumber = "Z-0290-2013",
                TradeName = "A3/A5 Anesthesia Delivery System",
                ProductCode = "BSZ",
                ProductCodeName = "GAS-MACHINE, ANESTHESIA",
                RegulationNumber = "868.5160",
                WebAddress = "https://example.com/2",
                FirmName = "Mindray DS USA, Inc.",
                RecallClass = "1",
                ClassificationDate = new DateTime(2012, 11, 16),
                ProductDescription = "Administer inhalation anesthetic and maintain ventilation",
                RecallReason = "System leak resulting from carbon dioxide absorber canister gasket."
            },
            new RecallNotice
            {
                Id = 3,
                RecallNumber = "Z-2078-2010",
                TradeName = "Cook Medical Ciaglia Blue Dolphin",
                ProductCode = "JOH",
                ProductCodeName = "TRACHEOSTOMY TUBE AND TUBE CUFF",
                RegulationNumber = "868.5800",
                WebAddress = "https://example.com/3",
                FirmName = "Cook, Inc.",
                RecallClass = "1",
                ClassificationDate = new DateTime(2010, 7, 26),
                ProductDescription = "Percutaneous tracheostomy introducer tray",
                RecallReason = "Covidien Shiley tracheostomy tube associated with cuff leakage as result of pilot balloon."
            },
            new RecallNotice
            {
                Id = 4,
                RecallNumber = "Z-0720-2026",
                TradeName = "Sevoflurane (SEVO) V90 Electronic Vaporizer",
                ProductCode = "BSZ",
                ProductCodeName = "GAS-MACHINE, ANESTHESIA",
                RegulationNumber = "868.5160",
                WebAddress = "https://example.com/4",
                FirmName = "Mindray DS USA, Inc.",
                RecallClass = "2",
                ClassificationDate = new DateTime(2025, 11, 26),
                ProductDescription = "Electronic vaporizer for anesthetic delivery",
                RecallReason = "Potential for anesthesia leakage."
            },
            new RecallNotice
            {
                Id = 5,
                RecallNumber = "Z-0260-2007",
                TradeName = "Bivona Tracheostomy Tube",
                ProductCode = "JOH",
                ProductCodeName = "TRACHEOSTOMY TUBE AND TUBE CUFF",
                RegulationNumber = "868.5800",
                WebAddress = "https://example.com/5",
                FirmName = "Smiths Medical ASD, Inc.",
                RecallClass = "3",
                ClassificationDate = new DateTime(2006, 12, 6),
                ProductDescription = "Custom uncuffed hyperflex tracheostomy tube",
                RecallReason = "Mislabeled as to size: length 47.5mm labeled as 4.75mm."
            }
        );
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Search_ByKeyword_ReturnsMatchingNotices()
    {
        using var db = TestDbContextFactory.Create("Search_Keyword_Db");
        await SeedSampleNotices(db);
        var service = new NoticeSearchService(db);

        var criteria = new NoticeSearchCriteria
        {
            Keyword = "leak",
            SearchInDescription = true,
            SearchInRecallReason = true,
            SearchInTradeName = true
        };

        var result = await service.SearchNoticesAsync(criteria);

        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 3);
        Assert.All(result.Items, item =>
            Assert.True(
                item.RecallReason.Contains("leak", StringComparison.OrdinalIgnoreCase) ||
                item.RecallReason.Contains("leakage", StringComparison.OrdinalIgnoreCase) ||
                (item.TradeName != null && item.TradeName.Contains("leak", StringComparison.OrdinalIgnoreCase))
            )
        );
    }

    [Fact]
    public async Task Search_ByClassFilter_ReturnsOnlySpecifiedClasses()
    {
        using var db = TestDbContextFactory.Create("Search_ClassFilter_Db");
        await SeedSampleNotices(db);
        var service = new NoticeSearchService(db);

        var criteria = new NoticeSearchCriteria
        {
            Classes = new List<string> { "1" }
        };

        var result = await service.SearchNoticesAsync(criteria);

        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, item => Assert.Equal("1", item.RecallClass));
    }

    [Fact]
    public async Task Search_ByPartialWords_FindsMatchingSubstrings()
    {
        using var db = TestDbContextFactory.Create("Search_Partial_Db");
        await SeedSampleNotices(db);
        var service = new NoticeSearchService(db);

        var criteria = new NoticeSearchCriteria
        {
            Keyword = "tracheo",
            MatchPartialWords = true
        };

        var result = await service.SearchNoticesAsync(criteria);

        Assert.True(result.TotalCount >= 2);
        Assert.Contains(result.Items, n => n.RecallNumber == "Z-2078-2010");
        Assert.Contains(result.Items, n => n.RecallNumber == "Z-0260-2007");
    }

    [Fact]
    public async Task Search_ByFirmName_FiltersCorrectly()
    {
        using var db = TestDbContextFactory.Create("Search_Firm_Db");
        await SeedSampleNotices(db);
        var service = new NoticeSearchService(db);

        var criteria = new NoticeSearchCriteria
        {
            FirmName = "Cook, Inc."
        };

        var result = await service.SearchNoticesAsync(criteria);

        Assert.Single(result.Items);
        Assert.Equal("Z-2078-2010", result.Items[0].RecallNumber);
    }

    [Fact]
    public async Task Search_Pagination_ReturnsCorrectSlice()
    {
        using var db = TestDbContextFactory.Create("Search_Paging_Db");
        await SeedSampleNotices(db);
        var service = new NoticeSearchService(db);

        var criteria = new NoticeSearchCriteria
        {
            PageNumber = 1,
            PageSize = 2
        };

        var result = await service.SearchNoticesAsync(criteria);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void SearchViewModel_DefaultsToEmptyKeyword()
    {
        var model = new ANDtr.Web.Features.Search.SearchViewModel();
        Assert.True(string.IsNullOrEmpty(model.Keyword), "SearchViewModel.Keyword must default to empty/null so the search input box is blank.");
    }

    [Fact]
    public void FilterViewModel_DefaultsToEmptyKeywordAndClasses()
    {
        var model = new ANDtr.Web.Features.Filters.FilterViewModel();
        Assert.True(string.IsNullOrEmpty(model.Keyword), "FilterViewModel.Keyword must default to empty/null.");
        Assert.Empty(model.Classes);
    }
}

