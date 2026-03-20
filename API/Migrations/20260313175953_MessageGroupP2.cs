using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class MessageGroupP2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageConnections_MessageGroups_MessageGroupName",
                table: "MessageConnections");

            migrationBuilder.AlterColumn<string>(
                name: "MessageGroupName",
                table: "MessageConnections",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageConnections_MessageGroups_MessageGroupName",
                table: "MessageConnections",
                column: "MessageGroupName",
                principalTable: "MessageGroups",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageConnections_MessageGroups_MessageGroupName",
                table: "MessageConnections");

            migrationBuilder.AlterColumn<string>(
                name: "MessageGroupName",
                table: "MessageConnections",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageConnections_MessageGroups_MessageGroupName",
                table: "MessageConnections",
                column: "MessageGroupName",
                principalTable: "MessageGroups",
                principalColumn: "Name");
        }
    }
}
