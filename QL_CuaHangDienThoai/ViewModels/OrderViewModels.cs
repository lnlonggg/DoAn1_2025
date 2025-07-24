using System.ComponentModel.DataAnnotations;

namespace QL_CuaHangDienThoai.ViewModels
{
    public class CreateOrderViewModel
    {
        [Required]
        public string MaDT { get; set; } = string.Empty;

        public string? TenDT { get; set; }

        public decimal DonGia { get; set; }

        public int SoLuongTon { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string HoTenNguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string DiaChiGiaoHang { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        public decimal ThanhTien => SoLuong * DonGia;
    }
}
