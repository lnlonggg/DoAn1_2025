using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_CuaHangDienThoai.Migrations
{
    public partial class AddSoDTToQuanTriVien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoDT",
                table: "QUANTRIVIEN",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoDT",
                table: "QUANTRIVIEN");
        }
    }
}
