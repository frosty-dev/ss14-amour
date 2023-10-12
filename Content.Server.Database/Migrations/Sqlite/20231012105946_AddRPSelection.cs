using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddRPSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roleplay_selection",
                columns: table => new
                {
                    roleplay_selection_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    profile_id = table.Column<int>(type: "INTEGER", nullable: false),
                    roleplay_selection_name = table.Column<string>(type: "TEXT", nullable: false),
                    roleplay_selection_value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roleplay_selection", x => x.roleplay_selection_id);
                    table.ForeignKey(
                        name: "FK_roleplay_selection_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_roleplay_selection_profile_id_roleplay_selection_name",
                table: "roleplay_selection",
                columns: new[] { "profile_id", "roleplay_selection_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "roleplay_selection");
        }
    }
}
