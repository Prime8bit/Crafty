using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class SearchImageRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Crafts_CraftMedia_SearchMediaId",
                table: "Crafts");

            migrationBuilder.RenameColumn(
                name: "SearchMediaId",
                table: "Crafts",
                newName: "SearchImageId");

            migrationBuilder.RenameIndex(
                name: "IX_Crafts_SearchMediaId",
                table: "Crafts",
                newName: "IX_Crafts_SearchImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Crafts_CraftMedia_SearchImageId",
                table: "Crafts",
                column: "SearchImageId",
                principalTable: "CraftMedia",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Crafts_CraftMedia_SearchImageId",
                table: "Crafts");

            migrationBuilder.RenameColumn(
                name: "SearchImageId",
                table: "Crafts",
                newName: "SearchMediaId");

            migrationBuilder.RenameIndex(
                name: "IX_Crafts_SearchImageId",
                table: "Crafts",
                newName: "IX_Crafts_SearchMediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Crafts_CraftMedia_SearchMediaId",
                table: "Crafts",
                column: "SearchMediaId",
                principalTable: "CraftMedia",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
