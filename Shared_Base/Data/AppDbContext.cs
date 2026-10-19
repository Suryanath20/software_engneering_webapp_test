using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Models;

namespace ANDtr.Web.Data;

/// <summary>
/// Main EF Core database context configured for MySQL 8 with Identity.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<RecallNotice> RecallNotices => Set<RecallNotice>();
    public DbSet<RiskReview> RiskReviews => Set<RiskReview>();
    public DbSet<ReviewNotice> ReviewNotices => Set<ReviewNotice>();
    public DbSet<ReviewShare> ReviewShares => Set<ReviewShare>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ImportRecord> ImportRecords => Set<ImportRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // RecallNotice configuration & indexes
        builder.Entity<RecallNotice>(b =>
        {
            b.HasKey(n => n.Id);
            b.Property(n => n.RecallNumber).HasMaxLength(64).IsRequired();
            b.Property(n => n.ProductCode).HasMaxLength(16).IsRequired();
            b.Property(n => n.RecallClass).HasMaxLength(8).IsRequired();
            b.Property(n => n.FirmName).HasMaxLength(255).IsRequired();
            b.Property(n => n.TradeName).HasMaxLength(500);
            b.Property(n => n.ProductCodeName).HasMaxLength(255);
            b.Property(n => n.RegulationNumber).HasMaxLength(64);
            b.Property(n => n.WebAddress).HasMaxLength(500);
            b.Property(n => n.FeiNumber).HasMaxLength(64);

            b.HasIndex(n => n.RecallNumber);
            b.HasIndex(n => n.FirmName);
            b.HasIndex(n => n.RecallClass);
            b.HasIndex(n => n.ProductCode);
            b.HasIndex(n => new { n.RecallNumber, n.ProductCode, n.ClassificationDate });
        });

        // RiskReview configuration
        builder.Entity<RiskReview>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Title).HasMaxLength(255).IsRequired();
            b.Property(r => r.Status).HasMaxLength(32).IsRequired().HasDefaultValue("Draft");

            b.HasOne(r => r.CreatedBy)
                .WithMany(u => u.CreatedReviews)
                .HasForeignKey(r => r.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReviewNotice composite key & relationships
        builder.Entity<ReviewNotice>(b =>
        {
            b.HasKey(rn => new { rn.ReviewId, rn.NoticeId });

            b.HasOne(rn => rn.Review)
                .WithMany(r => r.ReviewNotices)
                .HasForeignKey(rn => rn.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(rn => rn.Notice)
                .WithMany(n => n.ReviewNotices)
                .HasForeignKey(rn => rn.NoticeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ReviewShare configuration
        builder.Entity<ReviewShare>(b =>
        {
            b.HasKey(rs => rs.Id);

            b.HasOne(rs => rs.Review)
                .WithMany(r => r.Shares)
                .HasForeignKey(rs => rs.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(rs => rs.Customer)
                .WithMany(u => u.ReceivedShares)
                .HasForeignKey(rs => rs.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(rs => rs.SharedBy)
                .WithMany(u => u.InitiatedShares)
                .HasForeignKey(rs => rs.SharedById)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(rs => new { rs.ReviewId, rs.CustomerId, rs.RevokedAt });
        });

        // Comment configuration
        builder.Entity<Comment>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.AuthorRole).HasMaxLength(32).IsRequired();

            b.HasOne(c => c.Review)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ImportRecord configuration
        builder.Entity<ImportRecord>(b =>
        {
            b.HasKey(ir => ir.Id);
            b.Property(ir => ir.FileName).HasMaxLength(255).IsRequired();
            b.Property(ir => ir.Status).HasMaxLength(64).IsRequired();

            b.HasOne(ir => ir.ImportedBy)
                .WithMany()
                .HasForeignKey(ir => ir.ImportedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
