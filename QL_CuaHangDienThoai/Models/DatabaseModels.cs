using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_CuaHangDienThoai.Models
{
    [Table("TAIKHOAN")]
    public class TaiKhoan
    {
        [Key]
        [Column("TenDangNhap")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Column("MatKhau")]
        [StringLength(100)]
        [Required]
        public string MatKhau { get; set; } = string.Empty;

        [Column("Email")]
        [StringLength(100)]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Column("VaiTro")]
        [StringLength(20)]
        public string VaiTro { get; set; } = string.Empty;

        // Navigation properties
        public virtual KhachHang? KhachHang { get; set; }
        public virtual QuanTriVien? QuanTriVien { get; set; }
    }

    [Table("KHACHHANG")]
    public class KhachHang
    {
        [Key]
        [Column("MaKH")]
        [StringLength(10)]
        public string MaKH { get; set; } = string.Empty;

        [Column("HoTen")]
        [StringLength(100)]
        public string? HoTen { get; set; }

        [Column("SoDT")]
        [StringLength(15)]
        public string? SoDT { get; set; }

        [Column("Email")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Column("TenDangNhap")]
        [StringLength(50)]
        public string? TenDangNhap { get; set; }

        // Navigation properties
        [ForeignKey("TenDangNhap")]
        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }

    [Table("QUANTRIVIEN")]
    public class QuanTriVien
    {
        [Key]
        [Column("MaQTV")]
        [StringLength(10)]
        public string MaQTV { get; set; } = string.Empty;

        [Column("HoTen")]
        [StringLength(100)]
        public string? HoTen { get; set; }

        [Column("Email")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Column("TenDangNhap")]
        [StringLength(50)]
        public string? TenDangNhap { get; set; }

        // Navigation properties
        [ForeignKey("TenDangNhap")]
        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }

    [Table("DIENTHOAI")]
    public class DienThoai
    {
        [Key]
        [Column("MaDT")]
        [StringLength(10)]
        public string MaDT { get; set; } = string.Empty;

        [Column("TenDT")]
        [StringLength(100)]
        public string? TenDT { get; set; }

        [Column("DonGia")]  // ← Đổi từ DonGia thành DonGia
        [Range(0, double.MaxValue)]
        public decimal DonGia { get; set; }

        [Column("SoLuongTon")]
        [Range(0, int.MaxValue)]
        public int SoLuongTon { get; set; }

        // Navigation properties
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
    }

    [Table("HOADON")]
    public class HoaDon
    {
        [Key]
        [Column("MaHD")]
        [StringLength(10)]
        public string MaHD { get; set; } = string.Empty;

        [Column("MaKH")]
        [StringLength(10)]
        public string? MaKH { get; set; }

        [Column("MaQTV")]
        [StringLength(10)]
        public string? MaQTV { get; set; }

        [Column("NgayLap")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Column("TongTien")]
        public decimal TongTien { get; set; }

        // Navigation properties
        [ForeignKey("MaKH")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("MaQTV")]
        public virtual QuanTriVien? QuanTriVien { get; set; }

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
    }

    [Table("CHITIETHOADON")]
    public class ChiTietHoaDon
    {
        [Column("MaHD")]
        [StringLength(10)]
        public string MaHD { get; set; } = string.Empty;

        [Column("MaDT")]
        [StringLength(10)]
        public string MaDT { get; set; } = string.Empty;

        [Column("SoLuong")]
        [Range(1, int.MaxValue)]
        public int SoLuong { get; set; }

        [Column("DonGia")]
        public decimal DonGia { get; set; }

        [NotMapped]
        public decimal ThanhTien => SoLuong * DonGia;

        // Navigation properties
        [ForeignKey("MaHD")]
        public virtual HoaDon? HoaDon { get; set; }

        [ForeignKey("MaDT")]
        public virtual DienThoai? DienThoai { get; set; }
    }

    [Table("GIOHANG")]
    public class GioHang
    {
        [Key]
        [Column("MaGH")]
        [StringLength(10)]
        public string MaGH { get; set; } = string.Empty;

        [Column("MaKH")]
        [StringLength(10)]
        public string MaKH { get; set; } = string.Empty;

        [Column("NgayTao")]
        public DateTime NgayTao { get; set; }

        // Navigation properties
        [ForeignKey("MaKH")]
        public virtual KhachHang? KhachHang { get; set; }
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
    }

    [Table("CHITIETGIOHANG")]
    public class ChiTietGioHang
    {
        [Key]
        [Column("MaGH")]
        [StringLength(10)]
        public string MaGH { get; set; } = string.Empty;

        [Key]
        [Column("MaDT")]
        [StringLength(10)]
        public string MaDT { get; set; } = string.Empty;

        [Column("SoLuong")]
        public int SoLuong { get; set; }

        [Column("NgayThem")]
        public DateTime NgayThem { get; set; }

        // Navigation properties
        [ForeignKey("MaGH")]
        public virtual GioHang? GioHang { get; set; }

        [ForeignKey("MaDT")]
        public virtual DienThoai? DienThoai { get; set; }

        // Computed property
        public decimal ThanhTien => SoLuong * (DienThoai?.DonGia ?? 0);
    }

    // Enum cho trạng thái thanh toán
    public enum TrangThaiThanhToan
    {
        ChoDuyet,
        DaThanhToan,
        ThatBai,
        DaHuy
    }

    // Model cho thanh toán online
    [Table("THANHTOANTRUCTUYEN")]
    public class ThanhToanTrucTuyen
    {
        [Key]
        [Column("MaThanhToan")]
        [StringLength(50)]
        public string MaThanhToan { get; set; } = string.Empty;

        [Column("MaHD")]
        [StringLength(10)] 
        public string MaHD { get; set; } = string.Empty;

        [Column("SoTien")]
        public decimal SoTien { get; set; }

        [Column("PhuongThucThanhToan")]
        [StringLength(50)]
        public string PhuongThucThanhToan { get; set; } = string.Empty; // VNPay, Momo, etc.

        [Column("TrangThai")]
        public TrangThaiThanhToan TrangThai { get; set; }

        [Column("NgayTao")]
        public DateTime NgayTao { get; set; }

        [Column("NgayCapNhat")]
        public DateTime? NgayCapNhat { get; set; }

        [Column("MaGiaoDichNganHang")]
        [StringLength(100)]
        public string? MaGiaoDichNganHang { get; set; }

        [Column("ThongTinThem")]
        public string? ThongTinThem { get; set; }

        // Navigation properties
        [ForeignKey("MaHD")]
        public virtual HoaDon? HoaDon { get; set; }
    }
}