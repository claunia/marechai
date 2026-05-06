using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareUserRatingsReviewsVotesReportsNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminNotifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkUrl = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkText = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRead = table.Column<bool>(type: "bit(1)", nullable: false),
                    TargetUserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotificationType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminNotifications_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareUserRatings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Rating = table.Column<float>(type: "float", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareUserRatings", x => new { x.UserId, x.SoftwareId });
                    table.ForeignKey(
                        name: "FK_SoftwareUserRatings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareUserRatings_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareUserReviews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    TheGood = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TheBad = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TheUgly = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAnonymous = table.Column<bool>(type: "bit(1)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareUserReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareUserReviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SoftwareUserReviews_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReviewReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReporterId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewId = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsResolved = table.Column<bool>(type: "bit(1)", nullable: false),
                    ResolvedByUserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResolvedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewReports_AspNetUsers_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReviewReports_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReviewReports_SoftwareUserReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "SoftwareUserReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareUserReviewVotes",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewId = table.Column<long>(type: "bigint", nullable: false),
                    IsUpvote = table.Column<bool>(type: "bit(1)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareUserReviewVotes", x => new { x.UserId, x.ReviewId });
                    table.ForeignKey(
                        name: "FK_SoftwareUserReviewVotes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareUserReviewVotes_SoftwareUserReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "SoftwareUserReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_IsRead_TargetUserId",
                table: "AdminNotifications",
                columns: new[] { "IsRead", "TargetUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_TargetUserId",
                table: "AdminNotifications",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReports_IsResolved",
                table: "ReviewReports",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReports_ReporterId_ReviewId",
                table: "ReviewReports",
                columns: new[] { "ReporterId", "ReviewId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReports_ResolvedByUserId",
                table: "ReviewReports",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReports_ReviewId",
                table: "ReviewReports",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareUserRatings_SoftwareId",
                table: "SoftwareUserRatings",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareUserReviews_SoftwareId",
                table: "SoftwareUserReviews",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareUserReviews_UserId",
                table: "SoftwareUserReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareUserReviews_UserId_SoftwareId",
                table: "SoftwareUserReviews",
                columns: new[] { "UserId", "SoftwareId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareUserReviewVotes_ReviewId",
                table: "SoftwareUserReviewVotes",
                column: "ReviewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminNotifications");

            migrationBuilder.DropTable(
                name: "ReviewReports");

            migrationBuilder.DropTable(
                name: "SoftwareUserRatings");

            migrationBuilder.DropTable(
                name: "SoftwareUserReviewVotes");

            migrationBuilder.DropTable(
                name: "SoftwareUserReviews");
        }
    }
}
