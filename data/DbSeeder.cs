using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var importService = scope.ServiceProvider.GetRequiredService<IImportService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // 1. Roles
        string[] roles = { "Employee", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Users
        var usersToSeed = new[]
        {
            (Email: "employee@andtr.test", Password: "Employee#123", Name: "Alex Taylor", Company: "ANDtr Risk Team", Role: "Employee"),
            (Email: "customer@andtr.test", Password: "Customer#123", Name: "Jordan Smith", Company: "Northfield Medical Group", Role: "Customer")
        };

        ApplicationUser? primaryEmployee = null;
        ApplicationUser? primaryCustomer = null;
        ApplicationUser? secondCustomer = null;

        foreach (var u in usersToSeed)
        {
            var existing = await userManager.FindByEmailAsync(u.Email);
            if (existing == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = u.Email,
                    Email = u.Email,
                    FullName = u.Name,
                    CompanyName = u.Company,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                };

                var createRes = await userManager.CreateAsync(newUser, u.Password);
                if (createRes.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, u.Role);
                    existing = newUser;
                }
            }

            if (u.Email == "employee@andtr.test") primaryEmployee = existing;
            if (u.Email == "customer@andtr.test") primaryCustomer = existing;
            if (u.Email == "customer2@andtr.test") secondCustomer = existing;
        }

        // 3. Auto-import notices if empty
        if (!await db.RecallNotices.AnyAsync())
        {
            logger.LogInformation("No notices found in database. Running auto-import from data/FDAdataset.xlsx...");
            var importRes = await importService.ImportFromDefaultFileAsync(primaryEmployee?.Id, skipExisting: true);
            logger.LogInformation("Auto-import completed: {Result}", importRes.Message);
        }

        // 4. Seed sample reviews if empty
        if (!await db.RiskReviews.AnyAsync() && primaryEmployee != null)
        {
            logger.LogInformation("Seeding initial risk reviews, findings, shares, and comments matching wireframes...");

            // Fetch notice IDs
            var noticeLeak1 = await db.RecallNotices.FirstOrDefaultAsync(n => n.RecallNumber == "Z-1706-2015");
            var noticeLeak2 = await db.RecallNotices.FirstOrDefaultAsync(n => n.RecallNumber == "Z-0290-2013");
            var noticeLeak3 = await db.RecallNotices.FirstOrDefaultAsync(n => n.RecallNumber == "Z-0291-2013");

            var trachNotices = await db.RecallNotices
                .Where(n => n.RecallNumber == "Z-2078-2010" || n.RecallNumber == "Z-2084-2010" ||
                            n.RecallNumber == "Z-2082-2010" || n.RecallNumber == "Z-2083-2010" ||
                            n.RecallNumber == "Z-2090-2010")
                .ToListAsync();

            var autoTransNotices = await db.RecallNotices
                .Where(n => n.ProductCode == "CAC")
                .Take(4)
                .ToListAsync();

            var ventNotices = await db.RecallNotices
                .Where(n => n.ProductCode == "BTL")
                .Take(6)
                .ToListAsync();

            // Review 1: Anesthesia gas machine leak review
            var rev1 = new RiskReview
            {
                Title = "Anesthesia gas machine leak review",
                Summary = "Comprehensive review of anesthesia workstation gas circuit leaks, cassette dislodgement risk and manifold seal integrity across Flow-i and A3/A5 series devices.",
                FindingsHtml = "<p>Flow-i Anesthesia System (Maquet Cardiovascular Us Sales, Llc). In some Flow-i Anesthesia Systems, patient cassette can get dislodged which could result in gas leakage within the system internal circuit potentially leading to a stop in ventilation.</p><p><b>Recommendations:</b></p><ul><li>Recommend checking current stock and confirming whether any affected units are in service.</li><li>Perform pre-use manual leak check procedures as specified in device alert.</li><li>Ensure backup manual resuscitator is readily accessible in operating theatres.</li></ul>",
                Status = "Published",
                CreatedById = primaryEmployee.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            };

            if (noticeLeak1 != null) rev1.ReviewNotices.Add(new ReviewNotice { NoticeId = noticeLeak1.Id, EmployeeNote = "High risk: patient cassette dislodgement can halt ventilation." });
            if (noticeLeak2 != null) rev1.ReviewNotices.Add(new ReviewNotice { NoticeId = noticeLeak2.Id, EmployeeNote = "Canister gasket leak identified during automatic test." });
            if (noticeLeak3 != null) rev1.ReviewNotices.Add(new ReviewNotice { NoticeId = noticeLeak3.Id, EmployeeNote = "Requires startup leak check compliance inspection." });

            db.RiskReviews.Add(rev1);
            await db.SaveChangesAsync();

            // Shares for Review 1
            if (primaryCustomer != null)
            {
                db.ReviewShares.Add(new ReviewShare
                {
                    ReviewId = rev1.Id,
                    CustomerId = primaryCustomer.Id,
                    SharedById = primaryEmployee.Id,
                    SharedAt = DateTime.UtcNow.AddDays(-3),
                    Message = "Please review the anesthesia leak findings applicable to your operating rooms."
                });
            }

            if (secondCustomer != null)
            {
                db.ReviewShares.Add(new ReviewShare
                {
                    ReviewId = rev1.Id,
                    CustomerId = secondCustomer.Id,
                    SharedById = primaryEmployee.Id,
                    SharedAt = DateTime.UtcNow.AddDays(-4),
                    Message = "Urgent review regarding Flow-i gas machines."
                });
            }

            // Comments for Review 1
            if (primaryCustomer != null)
            {
                db.Comments.Add(new Comment
                {
                    ReviewId = rev1.Id,
                    AuthorId = primaryCustomer.Id,
                    AuthorRole = "Customer",
                    Body = "Can you confirm whether units with lot numbers starting 41 are affected? We have two machines in service.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                });
            }

            db.Comments.Add(new Comment
            {
                ReviewId = rev1.Id,
                AuthorId = primaryEmployee.Id,
                AuthorRole = "Employee",
                Body = "The notice lists the affected models only. We have asked the firm to confirm lot numbers and will update this review.",
                CreatedAt = DateTime.UtcNow.AddDays(-2).AddHours(2)
            });

            if (secondCustomer != null)
            {
                db.Comments.Add(new Comment
                {
                    ReviewId = rev1.Id,
                    AuthorId = secondCustomer.Id,
                    AuthorRole = "Customer",
                    Body = "Thanks. Is there a recommended interim check while we wait?",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                });
            }

            // Review 2: Tracheostomy tube cuff failures
            var rev2 = new RiskReview
            {
                Title = "Tracheostomy tube cuff failures",
                Summary = "Safety assessment of cuff leakage in percutaneous tracheostomy introducer kits and pilot balloon inflation assemblies.",
                FindingsHtml = "<p>Assessment of Covidien Shiley and Cook Medical Ciaglia tracheostomy kits indicated cuff leakage risks from pilot balloon assembly failure.</p><ul><li>Inspect all tracheal kits for pilot balloon seal integrity.</li><li>Ensure immediate availability of replacement cuffs in ICU and wards.</li></ul>",
                Status = "Published",
                CreatedById = primaryEmployee.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-9),
                UpdatedAt = DateTime.UtcNow.AddDays(-9)
            };

            foreach (var tn in trachNotices)
            {
                rev2.ReviewNotices.Add(new ReviewNotice { NoticeId = tn.Id, EmployeeNote = "Cuff leakage during mechanical ventilation risk." });
            }

            db.RiskReviews.Add(rev2);
            await db.SaveChangesAsync();

            if (primaryCustomer != null)
            {
                db.ReviewShares.Add(new ReviewShare
                {
                    ReviewId = rev2.Id,
                    CustomerId = primaryCustomer.Id,
                    SharedById = primaryEmployee.Id,
                    SharedAt = DateTime.UtcNow.AddDays(-9),
                    Message = "Shared for ICU clinical device team review."
                });

                db.Comments.Add(new Comment
                {
                    ReviewId = rev2.Id,
                    AuthorId = primaryCustomer.Id,
                    AuthorRole = "Customer",
                    Body = "Please add the recall classification dates to the summary so we can plan our review.",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                });
            }

            // Review 3: Autotransfusion apparatus alarms
            var rev3 = new RiskReview
            {
                Title = "Autotransfusion apparatus alarms",
                Summary = "Investigation into blood recovery suction line errors, bowl seal abrasions, and fluid containment issues.",
                FindingsHtml = "<p>Internal testing indicated insufficient suction of shed blood from reservoir and potential saline leaks at Y-connectors.</p>",
                Status = "Draft",
                CreatedById = primaryEmployee.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-16),
                UpdatedAt = DateTime.UtcNow.AddDays(-12)
            };

            foreach (var an in autoTransNotices)
            {
                rev3.ReviewNotices.Add(new ReviewNotice { NoticeId = an.Id, EmployeeNote = "Autotransfusion line suction check required." });
            }

            db.RiskReviews.Add(rev3);

            // Review 4: Emergency ventilator recalls 2025
            var rev4 = new RiskReview
            {
                Title = "Emergency ventilator recalls 2025",
                Summary = "Safety monitoring of transport and emergency powered resuscitators regarding circuit flow controllers and inadvertent knob movements.",
                FindingsHtml = "<p>Potential non-cycling or continuous positive gas flow when switched to ventilate mode on portable emergency resuscitators.</p>",
                Status = "Draft",
                CreatedById = primaryEmployee.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-23),
                UpdatedAt = DateTime.UtcNow.AddDays(-23)
            };

            foreach (var vn in ventNotices)
            {
                rev4.ReviewNotices.Add(new ReviewNotice { NoticeId = vn.Id, EmployeeNote = "Transport ventilator circuit review." });
            }

            db.RiskReviews.Add(rev4);

            await db.SaveChangesAsync();
            logger.LogInformation("Initial reviews and comments successfully seeded.");
        }
    }
}
