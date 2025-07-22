using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_CuaHangDienThoai.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DIENTHOAI",
                columns: table => new
                {
                    MaDT = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenDT = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DIENTHOAI", x => x.MaDT);
                });

            migrationBuilder.CreateTable(
                name: "TAIKHOAN",
                columns: table => new
                {
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAIKHOAN", x => x.TenDangNhap);
                    table.CheckConstraint("CK_TaiKhoan_VaiTro", "[VaiTro] IN ('khach', 'admin', 'nhanvien')");
                });

            migrationBuilder.CreateTable(
                name: "KHACHHANG",
                columns: table => new
                {
                    MaKH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDT = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KHACHHANG", x => x.MaKH);
                    table.ForeignKey(
                        name: "FK_KHACHHANG_TAIKHOAN_TenDangNhap",
                        column: x => x.TenDangNhap,
                        principalTable: "TAIKHOAN",
                        principalColumn: "TenDangNhap");
                });

            migrationBuilder.CreateTable(
                name: "QUANTRIVIEN",
                columns: table => new
                {
                    MaQTV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QUANTRIVIEN", x => x.MaQTV);
                    table.ForeignKey(
                        name: "FK_QUANTRIVIEN_TAIKHOAN_TenDangNhap",
                        column: x => x.TenDangNhap,
                        principalTable: "TAIKHOAN",
                        principalColumn: "TenDangNhap");
                });

            migrationBuilder.CreateTable(
                name: "HOADON",
                columns: table => new
                {
                    MaHD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaKH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaQTV = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HOADON", x => x.MaHD);
                    table.ForeignKey(
                        name: "FK_HOADON_KHACHHANG_MaKH",
                        column: x => x.MaKH,
                        principalTable: "KHACHHANG",
                        principalColumn: "MaKH");
                    table.ForeignKey(
                        name: "FK_HOADON_QUANTRIVIEN_MaQTV",
                        column: x => x.MaQTV,
                        principalTable: "QUANTRIVIEN",
                        principalColumn: "MaQTV");
                });

            migrationBuilder.CreateTable(
                name: "CHITIETHOADON",
                columns: table => new
                {
                    MaHD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaDT = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHITIETHOADON", x => new { x.MaHD, x.MaDT });
                    table.ForeignKey(
                        name: "FK_CHITIETHOADON_DIENTHOAI_MaDT",
                        column: x => x.MaDT,
                        principalTable: "DIENTHOAI",
                        principalColumn: "MaDT",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CHITIETHOADON_HOADON_MaHD",
                        column: x => x.MaHD,
                        principalTable: "HOADON",
                        principalColumn: "MaHD",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETHOADON_MaDT",
                table: "CHITIETHOADON",
                column: "MaDT");

            migrationBuilder.CreateIndex(
                name: "IX_HOADON_MaKH",
                table: "HOADON",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_HOADON_MaQTV",
                table: "HOADON",
                column: "MaQTV");

            migrationBuilder.CreateIndex(
                name: "IX_KHACHHANG_TenDangNhap",
                table: "KHACHHANG",
                column: "TenDangNhap",
                unique: true,
                filter: "[TenDangNhap] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QUANTRIVIEN_TenDangNhap",
                table: "QUANTRIVIEN",
                column: "TenDangNhap",
                unique: true,
                filter: "[TenDangNhap] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHITIETHOADON");

            migrationBuilder.DropTable(
                name: "DIENTHOAI");

            migrationBuilder.DropTable(
                name: "HOADON");

            migrationBuilder.DropTable(
                name: "KHACHHANG");

            migrationBuilder.DropTable(
                name: "QUANTRIVIEN");

            migrationBuilder.DropTable(
                name: "TAIKHOAN");
        }
    }
}
