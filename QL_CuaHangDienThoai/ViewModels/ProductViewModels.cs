using System.ComponentModel.DataAnnotations;

namespace QL_CuaHangDienThoai.ViewModels
{
    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã điện thoại")]
        [StringLength(10)]
        [Display(Name = "Mã điện thoại")]
        public string MaDT { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(100)]
        [Display(Name = "Tên sản phẩm")]
        public string TenDT { get; set; } = string.Empty;

        [Display(Name = "Hình ảnh sản phẩm")]
        public IFormFile? HinhAnhFile { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
        [Display(Name = "Giá bán (VNĐ)")]
        public decimal DonGia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Số lượng tồn kho")]
        public int SoLuongTon { get; set; }
    }

    public class EditProductViewModel
    {
        [Required]
        public string MaDT { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [Display(Name = "Tên sản phẩm")]
        public string TenDT { get; set; } = string.Empty;

        [Display(Name = "Hình ảnh hiện tại")]
        public string? HinhAnhHienTai { get; set; }

        [Display(Name = "Hình ảnh mới")]
        public IFormFile? HinhAnhFile { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Display(Name = "Giá bán (VNĐ)")]
        public decimal DonGia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
        [Display(Name = "Số lượng tồn kho")]
        public int SoLuongTon { get; set; }
    }

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