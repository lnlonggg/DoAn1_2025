using System.ComponentModel.DataAnnotations;

namespace QL_CuaHangDienThoai.ViewModels
{
    public class CartViewModel
    {
        public string MaGH { get; set; } = string.Empty;
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal TongTien => Items.Sum(i => i.ThanhTien);
        public int TongSoLuong => Items.Sum(i => i.SoLuong);
    }

    public class CartItemViewModel
    {
        public string MaDT { get; set; } = string.Empty;
        public string TenDT { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public int SoLuongTon { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class AddToCartViewModel
    {
        [Required]
        public string MaDT { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; } = 1;
    }

    public class CheckoutViewModel
    {
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

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = string.Empty;

        // Thông tin giỏ hàng
        public CartViewModel Cart { get; set; } = new CartViewModel();
    }
}