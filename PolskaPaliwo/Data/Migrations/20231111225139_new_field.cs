using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolskaPaliwo.Data.Migrations
{
    public partial class new_field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SerializedSearchResults",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerializedSearchResults",
                table: "AspNetUsers");
        }
    }
}
