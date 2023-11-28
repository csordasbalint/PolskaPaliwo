using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolskaPaliwo.Data.Migrations
{
    public partial class Added_new_field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastFeedback",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastFeedback",
                table: "AspNetUsers");
        }
    }
}
