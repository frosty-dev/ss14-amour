using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class RPMigrationEblya : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_roleplay_info_profile_id_name",
                table: "roleplay_info");

            migrationBuilder.CreateIndex(
                name: "IX_roleplay_info_profile_id_name",
                table: "roleplay_info",
                columns: new[] { "profile_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_roleplay_info_profile_id_name",
                table: "roleplay_info");

            migrationBuilder.CreateIndex(
                name: "IX_roleplay_info_profile_id_name",
                table: "roleplay_info",
                columns: new[] { "profile_id", "name" });
        }
    }
}
