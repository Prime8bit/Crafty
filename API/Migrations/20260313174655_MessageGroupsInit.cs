using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class MessageGroupsInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageGroups",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageGroups", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "MessageConnections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    MessageGroupName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageConnections_MessageGroups_MessageGroupName",
                        column: x => x.MessageGroupName,
                        principalTable: "MessageGroups",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageConnections_MessageGroupName",
                table: "MessageConnections",
                column: "MessageGroupName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageConnections");

            migrationBuilder.DropTable(
                name: "MessageGroups");
        }
    }
}
