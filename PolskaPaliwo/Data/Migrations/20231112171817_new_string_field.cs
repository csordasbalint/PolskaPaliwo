using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolskaPaliwo.Data.Migrations
{
    public partial class new_string_field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SerializedSearchResults",
                table: "AspNetUsers",
                newName: "PreviousIds");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviousIds",
                table: "AspNetUsers",
                newName: "SerializedSearchResults");
        }
    }
}
