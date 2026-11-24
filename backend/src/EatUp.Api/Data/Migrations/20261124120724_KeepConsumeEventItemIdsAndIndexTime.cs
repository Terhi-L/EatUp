using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatUp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class KeepConsumeEventItemIdsAndIndexTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsumeEvents_Items_FoodItemId",
                table: "ConsumeEvents");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumeEvents_ConsumedAt",
                table: "ConsumeEvents",
                column: "ConsumedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConsumeEvents_ConsumedAt",
                table: "ConsumeEvents");

            // Events kept the ids of items deleted since the upgrade; the restored foreign key needs them cleared.
            migrationBuilder.Sql("UPDATE ConsumeEvents SET FoodItemId = NULL WHERE FoodItemId IS NOT NULL AND FoodItemId NOT IN (SELECT Id FROM Items);");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumeEvents_Items_FoodItemId",
                table: "ConsumeEvents",
                column: "FoodItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
