using Microsoft.Extensions.Logging.Abstractions;
using ANDtr.Web.Services;
using Xunit;

namespace ANDtr.Tests;

public class ImportServiceTests
{
    private readonly string _defaultDataFilePath;

    public ImportServiceTests()
    {
        // Locate FDAdataset.xlsx in solution data folder
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "..", "..", "..", "..", "data", "FDAdataset.xlsx"),
            Path.Combine(baseDir, "..", "..", "data", "FDAdataset.xlsx"),
            Path.Combine(baseDir, "data", "FDAdataset.xlsx"),
            "/Users/suryanathsunil/Downloads/member_wireframes/ANDtr/data/FDAdataset.xlsx"
        };

        _defaultDataFilePath = candidates.FirstOrDefault(File.Exists) ?? candidates.Last();
    }

    [Fact]
    public async Task ImportFromStream_ParsesRealDataset_MatchesExpectedCounts()
    {
        if (!File.Exists(_defaultDataFilePath))
        {
            return;
        }

        using var db = TestDbContextFactory.Create("Import_RealDataset_Db");
        var testEnv = new TestWebHostEnvironment
        {
            ContentRootPath = Path.GetDirectoryName(_defaultDataFilePath)!
        };

        var service = new ImportService(db, testEnv, NullLogger<ImportService>.Instance);

        await using var stream = File.OpenRead(_defaultDataFilePath);
        var result = await service.ImportFromStreamAsync(stream, "FDAdataset.xlsx", "test-user-id");

        Assert.True(result.Success, result.Message);
        Assert.Equal(616, result.RowsRead);
        Assert.Equal(616, result.RowsImported);
        Assert.Equal(33, result.DuplicatesFlagged);
        Assert.Equal(127, result.BlankPostedDate);
        Assert.Equal(116, result.BlankTerminationDate);
        Assert.Equal(31, result.BlankTradeName);
        Assert.Equal(9, result.BlankFeiNumber);
        Assert.True(result.TextFieldsCleaned > 0, "Expected _x000D_ and artifact text fields to be cleaned.");

        // Verify database counts
        var stats = await service.GetImportStatsAsync();
        Assert.Equal(616, stats.TotalNotices);
        Assert.Equal(175, stats.Class1Count);
        Assert.Equal(429, stats.Class2Count);
        Assert.Equal(12, stats.Class3Count);
        Assert.Equal(89, stats.TotalFirms);
        Assert.Equal(12, stats.TotalProductCodes);
    }

    [Fact]
    public async Task ImportFromStream_IsIdempotent_SkipsAlreadyImportedNotices()
    {
        if (!File.Exists(_defaultDataFilePath)) return;

        using var db = TestDbContextFactory.Create("Import_Idempotent_Db");
        var testEnv = new TestWebHostEnvironment
        {
            ContentRootPath = Path.GetDirectoryName(_defaultDataFilePath)!
        };

        var service = new ImportService(db, testEnv, NullLogger<ImportService>.Instance);

        // Run 1
        await using (var stream1 = File.OpenRead(_defaultDataFilePath))
        {
            var r1 = await service.ImportFromStreamAsync(stream1, "FDAdataset.xlsx", "test-user");
            Assert.Equal(616, r1.RowsImported);
            Assert.Equal(0, r1.RowsSkipped);
        }

        // Run 2: same file with skipExisting = true
        await using (var stream2 = File.OpenRead(_defaultDataFilePath))
        {
            var r2 = await service.ImportFromStreamAsync(stream2, "FDAdataset.xlsx", "test-user", skipExisting: true);
            Assert.Equal(0, r2.RowsImported);
            Assert.Equal(616, r2.RowsSkipped);
        }

        // Database total count must still be exactly 616
        var stats = await service.GetImportStatsAsync();
        Assert.Equal(616, stats.TotalNotices);
    }
}
