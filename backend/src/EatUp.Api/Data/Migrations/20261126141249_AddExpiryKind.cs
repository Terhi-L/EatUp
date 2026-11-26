using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatUp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExpiryKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpiryKind",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: "UseBy");

            // The catalogue's best-before foods when this ran; later catalogue changes only reach new items.
            migrationBuilder.Sql("""
                UPDATE Items SET ExpiryKind = 'BestBefore'
                WHERE IngredientKey IN (
                    'egg', 'milk', 'cream-cheese', 'cheese', 'yogurt', 'butter', 'cream',
                    'avocado', 'banana', 'apple', 'berries', 'lemon', 'tomato', 'bell-pepper', 'onion', 'garlic',
                    'mushroom', 'zucchini', 'carrot', 'potato', 'cucumber', 'bread',
                    'pasta', 'rice', 'flour', 'oats', 'chickpeas', 'lentils', 'tofu',
                    'soy-sauce', 'peanut-butter', 'coconut-milk', 'canned-tomatoes');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryKind",
                table: "Items");
        }
    }
}
