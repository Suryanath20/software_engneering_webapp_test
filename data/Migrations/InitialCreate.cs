using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ANDtr.Web.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FullName = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CompanyName = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                MustChangePassword = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                SecurityStamp = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "RecallNotices",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ProductCode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ProductCodeName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RegulationNumber = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                WebAddress = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RecallNumber = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ProductDescription = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TradeName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RecallClass = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ClassificationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                PostedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                TerminationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                FeiNumber = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FirmName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RecallReason = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                IsDuplicateRecallNumber = table.Column<bool>(type: "tinyint(1)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecallNotices", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ClaimType = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ClaimType = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Name = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Value = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "ImportRecords",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ImportedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                ImportedById = table.Column<string>(type: "varchar(255)", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RowsRead = table.Column<int>(type: "int", nullable: false),
                RowsImported = table.Column<int>(type: "int", nullable: false),
                RowsSkipped = table.Column<int>(type: "int", nullable: false),
                DuplicatesFlagged = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Details = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImportRecords", x => x.Id);
                table.ForeignKey(
                    name: "FK_ImportRecords_AspNetUsers_ImportedById",
                    column: x => x.ImportedById,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "RiskReviews",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Summary = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FindingsHtml = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, defaultValue: "Draft")
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedById = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RiskReviews", x => x.Id);
                table.ForeignKey(
                    name: "FK_RiskReviews_AspNetUsers_CreatedById",
                    column: x => x.CreatedById,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Comments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ReviewId = table.Column<int>(type: "int", nullable: false),
                AuthorId = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Body = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                AuthorRole = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Comments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Comments_AspNetUsers_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Comments_RiskReviews_ReviewId",
                    column: x => x.ReviewId,
                    principalTable: "RiskReviews",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "ReviewNotices",
            columns: table => new
            {
                ReviewId = table.Column<int>(type: "int", nullable: false),
                NoticeId = table.Column<int>(type: "int", nullable: false),
                EmployeeNote = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                AddedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReviewNotices", x => new { x.ReviewId, x.NoticeId });
                table.ForeignKey(
                    name: "FK_ReviewNotices_RecallNotices_NoticeId",
                    column: x => x.NoticeId,
                    principalTable: "RecallNotices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ReviewNotices_RiskReviews_ReviewId",
                    column: x => x.ReviewId,
                    principalTable: "RiskReviews",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "ReviewShares",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ReviewId = table.Column<int>(type: "int", nullable: false),
                CustomerId = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                SharedById = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                SharedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                Message = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RevokedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReviewShares", x => x.Id);
                table.ForeignKey(
                    name: "FK_ReviewShares_AspNetUsers_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ReviewShares_AspNetUsers_SharedById",
                    column: x => x.SharedById,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ReviewShares_RiskReviews_ReviewId",
                    column: x => x.ReviewId,
                    principalTable: "RiskReviews",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Comments_AuthorId",
            table: "Comments",
            column: "AuthorId");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_ReviewId",
            table: "Comments",
            column: "ReviewId");

        migrationBuilder.CreateIndex(
            name: "IX_ImportRecords_ImportedById",
            table: "ImportRecords",
            column: "ImportedById");

        migrationBuilder.CreateIndex(
            name: "IX_RecallNotices_FirmName",
            table: "RecallNotices",
            column: "FirmName");

        migrationBuilder.CreateIndex(
            name: "IX_RecallNotices_ProductCode",
            table: "RecallNotices",
            column: "ProductCode");

        migrationBuilder.CreateIndex(
            name: "IX_RecallNotices_RecallClass",
            table: "RecallNotices",
            column: "RecallClass");

        migrationBuilder.CreateIndex(
            name: "IX_RecallNotices_RecallNumber",
            table: "RecallNotices",
            column: "RecallNumber");

        migrationBuilder.CreateIndex(
            name: "IX_RecallNotices_RecallNumber_ProductCode_ClassificationDate",
            table: "RecallNotices",
            columns: new[] { "RecallNumber", "ProductCode", "ClassificationDate" });

        migrationBuilder.CreateIndex(
            name: "IX_ReviewNotices_NoticeId",
            table: "ReviewNotices",
            column: "NoticeId");

        migrationBuilder.CreateIndex(
            name: "IX_ReviewShares_CustomerId",
            table: "ReviewShares",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_ReviewShares_ReviewId_CustomerId_RevokedAt",
            table: "ReviewShares",
            columns: new[] { "ReviewId", "CustomerId", "RevokedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ReviewShares_SharedById",
            table: "ReviewShares",
            column: "SharedById");

        migrationBuilder.CreateIndex(
            name: "IX_RiskReviews_CreatedById",
            table: "RiskReviews",
            column: "CreatedById");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AspNetRoleClaims");
        migrationBuilder.DropTable(name: "AspNetUserClaims");
        migrationBuilder.DropTable(name: "AspNetUserLogins");
        migrationBuilder.DropTable(name: "AspNetUserRoles");
        migrationBuilder.DropTable(name: "AspNetUserTokens");
        migrationBuilder.DropTable(name: "Comments");
        migrationBuilder.DropTable(name: "ImportRecords");
        migrationBuilder.DropTable(name: "ReviewNotices");
        migrationBuilder.DropTable(name: "ReviewShares");
        migrationBuilder.DropTable(name: "AspNetRoles");
        migrationBuilder.DropTable(name: "RecallNotices");
        migrationBuilder.DropTable(name: "RiskReviews");
        migrationBuilder.DropTable(name: "AspNetUsers");
    }
}
