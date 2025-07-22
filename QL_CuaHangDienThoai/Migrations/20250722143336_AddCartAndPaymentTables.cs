using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_CuaHangDienThoai.Migrations
{
    public partial class AddCartAndPaymentTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GIOHANG",
                columns: table => new
                {
                    MaGH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaKH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GIOHANG", x => x.MaGH);
                    table.ForeignKey(
                        name: "FK_GIOHANG_KHACHHANG_MaKH",
                        column: x => x.MaKH,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "THANHTOANTRUCTUYEN",
                columns: table => new
                {
                    MaThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaHD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaGiaoDichNganHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ThongTinThem = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THANHTOANTRUCTUYEN", x => x.MaThanhToan);
                    table.ForeignKey(
                        name: "FK_THANHTOANTRUCTUYEN_HOADON_MaHD",
                        column: x => x.MaHD,
                        principalTable: "HOADON",
                        principalColumn: "MaHD",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CHITIETGIOHANG",
                columns: table => new
                {
                    MaGH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaDT = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHITIETGIOHANG", x => new { x.MaGH, x.MaDT });
                    table.ForeignKey(
                        name: "FK_CHITIETGIOHANG_DIENTHOAI_MaDT",
                        column: x => x.MaDT,
                        principalTable: "DIENTHOAI",
                        principalColumn: "MaDT",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CHITIETGIOHANG_GIOHANG_MaGH",
                        column: x => x.MaGH,
                        principalTable: "GIOHANG",
                        principalColumn: "MaGH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETGIOHANG_MaDT",
                table: "CHITIETGIOHANG",
                column: "MaDT");

            migrationBuilder.CreateIndex(
                name: "IX_GIOHANG_MaKH",
                table: "GIOHANG",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_THANHTOANTRUCTUYEN_MaHD",
                table: "THANHTOANTRUCTUYEN",
                column: "MaHD");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHITIETGIOHANG");

            migrationBuilder.DropTable(
                name: "THANHTOANTRUCTUYEN");

            migrationBuilder.DropTable(
                name: "GIOHANG");
        }
    }
}
