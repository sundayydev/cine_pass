using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_CinePass.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "push_enable",
                table: "notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "push_enable",
                table: "notification_settings");
        }
    }
}
