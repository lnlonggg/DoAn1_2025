using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Models;

namespace QL_CuaHangDienThoai.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<QuanTriVien> QuanTriViens { get; set; }
        public DbSet<DienThoai> DienThoais { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính composite cho ChiTietHoaDon
            modelBuilder.Entity<ChiTietHoaDon>()
                .HasKey(c => new { c.MaHD, c.MaDT });

            // Cấu hình decimal precision
            modelBuilder.Entity<DienThoai>()
                .Property(d => d.DonGia)
                .HasPrecision(12, 2);

            modelBuilder.Entity<HoaDon>()
                .Property(h => h.TongTien)
                .HasPrecision(12, 2);

            modelBuilder.Entity<ChiTietHoaDon>()
                .Property(c => c.DonGia)
                .HasPrecision(12, 2);

            // Cấu hình check constraints
            modelBuilder.Entity<TaiKhoan>()
                .HasCheckConstraint("CK_TaiKhoan_VaiTro", "[VaiTro] IN ('khach', 'admin', 'nhanvien')");

            // Cấu hình relationships
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.TaiKhoan)
                .WithOne(t => t.KhachHang)
                .HasForeignKey<KhachHang>(k => k.TenDangNhap);

            modelBuilder.Entity<QuanTriVien>()
                .HasOne(q => q.TaiKhoan)
                .WithOne(t => t.QuanTriVien)
                .HasForeignKey<QuanTriVien>(q => q.TenDangNhap);

            modelBuilder.Entity<HoaDon>()
                .HasOne(h => h.KhachHang)
                .WithMany(k => k.HoaDons)
                .HasForeignKey(h => h.MaKH);

            modelBuilder.Entity<HoaDon>()
                .HasOne(h => h.QuanTriVien)
                .WithMany(q => q.HoaDons)
                .HasForeignKey(h => h.MaQTV);

            modelBuilder.Entity<ChiTietHoaDon>()
                .HasOne(c => c.HoaDon)
                .WithMany(h => h.ChiTietHoaDons)
                .HasForeignKey(c => c.MaHD);

            modelBuilder.Entity<ChiTietHoaDon>()
                .HasOne(c => c.DienThoai)
                .WithMany(d => d.ChiTietHoaDons)
                .HasForeignKey(c => c.MaDT);
        }
    }
}