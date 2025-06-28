using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriathlonTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddIsEncryptedToConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEncrypted",
                table: "Configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEncrypted",
                table: "Configurations");
        }
    }
}
