using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PutAway.Server.Data.Migrations
{
    public partial class SampleData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "BuyDate", "Description", "ItemId", "Name", "Price", "YearsOfWarranty" },
                values: new object[] { 1, null, "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor.", null, "Item Name 1", null, null });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "BuyDate", "Description", "ItemId", "Name", "Price", "YearsOfWarranty" },
                values: new object[] { 2, null, "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor.", null, "Item Name 2", null, null });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "BuyDate", "Description", "ItemId", "Name", "Price", "YearsOfWarranty" },
                values: new object[] { 3, null, "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor.", null, "Item Name 3", null, null });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "BuyDate", "Description", "ItemId", "Name", "Price", "YearsOfWarranty" },
                values: new object[] { 4, null, "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor.", null, "Item Name 4", null, null });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "BuyDate", "Description", "ItemId", "Name", "Price", "YearsOfWarranty" },
                values: new object[] { 5, null, "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor.", null, "Item Name 5", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
