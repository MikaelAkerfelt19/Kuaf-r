using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kuafor.Web.Migrations
{
    /// <inheritdoc />
    public partial class OL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveredCount",
                table: "MessageCampaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FailedCount",
                table: "MessageCampaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SentCount",
                table: "MessageCampaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredCount",
                table: "MessageCampaigns");

            migrationBuilder.DropColumn(
                name: "FailedCount",
                table: "MessageCampaigns");

            migrationBuilder.DropColumn(
                name: "SentCount",
                table: "MessageCampaigns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Customers");
        }
    }
}
