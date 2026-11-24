using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatUp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumeEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumeEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FoodItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    ItemName = table.Column<string>(type: "TEXT", nullable: false),
                    IngredientKey = table.Column<string>(type: "TEXT", nullable: true),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    Outcome = table.Column<string>(type: "TEXT", nullable: false),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumeEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumeEvents_Items_FoodItemId",
                        column: x => x.FoodItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumeEvents_FoodItemId",
                table: "ConsumeEvents",
                column: "FoodItemId");

            // History before this migration only kept the final status, so each finished item becomes one full consume.
            migrationBuilder.Sql("""
                INSERT INTO ConsumeEvents (FoodItemId, ItemName, IngredientKey, Amount, Unit, Outcome, ConsumedAt)
                SELECT Id, Name, IngredientKey, Quantity, Unit, Status, COALESCE(StatusChangedAt, CreatedAt)
                FROM Items
                WHERE Status IN ('Used', 'Discarded');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumeEvents");
        }
    }
}
