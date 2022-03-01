using Microsoft.EntityFrameworkCore.Migrations;

namespace DanLiris.Admin.Web.Migrations
{
    public partial class add_customscategorytoall : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomsCategory",
                table: "GarmentSewingOutItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsCategory",
                table: "GarmentSewingInItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsCategory",
                table: "GarmentLoadingItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsCategory",
                table: "GarmentFinishingOutItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsCategory",
                table: "GarmentFinishingInItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsCategory",
                table: "GarmentFinishedGoodStocks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsCategory",
                table: "GarmentFinishedGoodStockHistories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsCategory",
                table: "GarmentExpenditureGoodReturnItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsCategory",
                table: "GarmentExpenditureGoodItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsCategory",
                table: "GarmentAdjustmentItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomsCategory",
                table: "GarmentSewingOutItems");

            migrationBuilder.DropColumn(
                name: "CustomsCategory",
                table: "GarmentSewingInItems");

            migrationBuilder.DropColumn(
                name: "CustomsCategory",
                table: "GarmentLoadingItems");

            migrationBuilder.DropColumn(
                name: "CustomsCategory",
                table: "GarmentFinishingOutItems");

            migrationBuilder.DropColumn(
                name: "CustomsCategory",
                table: "GarmentFinishingInItems");

            migrationBuilder.DropColumn(
                name: "CustomsCategory",
                table: "GarmentFinishedGoodStocks");

            migrationBuilder.DropColumn(
                name: "CustomsCategory",
                table: "GarmentFinishedGoodStockHistories");

            migrationBuilder.DropColumn(
                name: "CustomsCategory",
                table: "GarmentExpenditureGoodReturnItems");

            migrationBuilder.DropColumn(
                name: "CustomsCategory",
                table: "GarmentExpenditureGoodItems");

            migrationBuilder.DropColumn(
                name: "CustomsCategory",
                table: "GarmentAdjustmentItems");
        }
    }
}
