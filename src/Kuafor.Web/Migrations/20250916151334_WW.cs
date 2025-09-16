using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kuafor.Web.Migrations
{
    /// <inheritdoc />
    public partial class WW : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "CampaignMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MessageBlacklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageBlacklists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageCredits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreditAmount = table.Column<int>(type: "int", nullable: false),
                    CostPerCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageCredits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageCampaignId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MessageContent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageReports_MessageCampaigns_MessageCampaignId",
                        column: x => x.MessageCampaignId,
                        principalTable: "MessageCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WhatsAppTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WhatsAppTemplateParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    ParameterName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParameterType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Example = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppTemplateParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppTemplateParameters_WhatsAppTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "WhatsAppTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WhatsAppTemplateUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppTemplateUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppTemplateUsages_WhatsAppTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "WhatsAppTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageBlacklists_CreatedAt",
                table: "MessageBlacklists",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MessageBlacklists_IsActive",
                table: "MessageBlacklists",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MessageBlacklists_PhoneNumber",
                table: "MessageBlacklists",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MessageCredits_LastUpdated",
                table: "MessageCredits",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_MessageCredits_Type",
                table: "MessageCredits",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_DeliveryStatus",
                table: "MessageReports",
                column: "DeliveryStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_MessageCampaignId",
                table: "MessageReports",
                column: "MessageCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_MessageType",
                table: "MessageReports",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_PhoneNumber",
                table: "MessageReports",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_SentAt",
                table: "MessageReports",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplates_CreatedAt",
                table: "MessageTemplates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplates_IsActive",
                table: "MessageTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplates_Name",
                table: "MessageTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplates_Type",
                table: "MessageTemplates",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppTemplateParameters_TemplateId",
                table: "WhatsAppTemplateParameters",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppTemplates_Category",
                table: "WhatsAppTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppTemplates_IsActive",
                table: "WhatsAppTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppTemplates_Name",
                table: "WhatsAppTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppTemplates_Status",
                table: "WhatsAppTemplates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppTemplateUsages_PhoneNumber",
                table: "WhatsAppTemplateUsages",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppTemplateUsages_SentAt",
                table: "WhatsAppTemplateUsages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppTemplateUsages_TemplateId",
                table: "WhatsAppTemplateUsages",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageBlacklists");

            migrationBuilder.DropTable(
                name: "MessageCredits");

            migrationBuilder.DropTable(
                name: "MessageReports");

            migrationBuilder.DropTable(
                name: "MessageTemplates");

            migrationBuilder.DropTable(
                name: "WhatsAppTemplateParameters");

            migrationBuilder.DropTable(
                name: "WhatsAppTemplateUsages");

            migrationBuilder.DropTable(
                name: "WhatsAppTemplates");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "CampaignMessages");
        }
    }
}
