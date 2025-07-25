using System.ComponentModel.DataAnnotations;

namespace QL_CuaHangDienThoai.ViewModels
{
    public class ReportFilterViewModel
    {
        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);

        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; } = DateTime.Today;

        [Display(Name = "Loại báo cáo")]
        public string ReportType { get; set; } = "overview";

        [Display(Name = "Danh mục")]
        public string Category { get; set; } = "all";

        [Display(Name = "Trang")]
        public int Page { get; set; } = 1;

        [Display(Name = "Số bản ghi/trang")]
        public int PageSize { get; set; } = 10;

        // Validation
        public bool IsValidDateRange => FromDate <= ToDate;
        public bool IsValidPage => Page > 0;
        public TimeSpan DateRangeSpan => ToDate - FromDate;
    }

    public class ReportOverviewViewModel
    {
        public decimal TongDoanhThu { get; set; }
        public int TongDonHang { get; set; }
        public decimal DoanhThuTrungBinh { get; set; }
        public int SanPhamSapHet { get; set; }
        public decimal TongGiaTriTonKho { get; set; }
        public int SanPhamTheoCategory { get; set; }
    }

    public class ProductSalesViewModel
    {
        public string MaDT { get; set; } = string.Empty;
        public string TenDT { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal LoiNhuan { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class CustomerViewModel
    {
        public string MaKH { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDT { get; set; } = string.Empty;
        public int SoDonHang { get; set; }
        public decimal TongTien { get; set; }
    }

    public class InventoryViewModel
    {
        public string MaDT { get; set; } = string.Empty;
        public string TenDT { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public int SoLuongTon { get; set; }
        public decimal GiaTriTon { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string AlertLevel => SoLuongTon <= 5 ? "danger" : SoLuongTon <= 10 ? "warning" : "success";
    }

    public class RevenueByDateViewModel
    {
        public DateTime NgayDate { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoDonHang { get; set; }
        public string FormattedDate => NgayDate.ToString("dd/MM");
    }

    public class ProfitReportViewModel
    {
        public string MaDT { get; set; } = string.Empty;
        public string TenDT { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public decimal GiaNhap { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal LoiNhuan { get; set; }
        public double TyLeLoiNhuan => GiaBan > 0 ? (double)((GiaBan - GiaNhap) / GiaBan * 100) : 0;
    }

    public class MonthlyStatsViewModel
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoDonHang { get; set; }
        public decimal DoanhThuTrungBinh { get; set; }
        public string FormattedMonth => $"{Thang:D2}/{Nam}";
    }

    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    }

    public class DetailedReportViewModel
    {
        public ReportFilterViewModel Filter { get; set; } = new();
        public ReportOverviewViewModel Overview { get; set; } = new();
        public List<RevenueByDateViewModel> RevenueByDate { get; set; } = new();
        public List<ProductSalesViewModel> TopProducts { get; set; } = new();
        public List<CustomerViewModel> TopCustomers { get; set; } = new();
        public List<InventoryViewModel> InventoryStatus { get; set; } = new();
        public List<ProfitReportViewModel> ProfitReport { get; set; } = new();
        public List<MonthlyStatsViewModel> MonthlyStats { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
        public List<string> AvailableCategories { get; set; } = new();

        // Trạng thái báo cáo
        public bool HasData => Overview.TongDonHang > 0;
        public string ReportTitle => GetReportTitle();
        public string DateRangeText => $"{Filter.FromDate:dd/MM/yyyy} - {Filter.ToDate:dd/MM/yyyy}";

        private string GetReportTitle()
        {
            return Filter.ReportType switch
            {
                "daily" => "Báo cáo theo ngày",
                "monthly" => "Báo cáo theo tháng",
                "profit" => "Báo cáo lợi nhuận",
                "products" => "Báo cáo sản phẩm",
                _ => "Báo cáo tổng quan"
            };
        }
    }

    // Enums cho các tùy chọn
    public enum ReportType
    {
        [Display(Name = "Tổng quan")]
        Overview,

        [Display(Name = "Theo ngày")]
        Daily,

        [Display(Name = "Theo tháng")]
        Monthly,

        [Display(Name = "Lợi nhuận")]
        Profit,

        [Display(Name = "Sản phẩm")]
        Products
    }

    public enum ProductCategory
    {
        [Display(Name = "Tất cả")]
        All,

        [Display(Name = "iPhone")]
        iPhone,

        [Display(Name = "Samsung")]
        Samsung,

        [Display(Name = "Xiaomi")]
        Xiaomi,

        [Display(Name = "OPPO")]
        OPPO,

        [Display(Name = "Khác")]
        Other
    }

    // Extension methods để hỗ trợ
    public static class ReportExtensions
    {
        public static string GetCategoryDisplayName(this string category)
        {
            return category.ToLower() switch
            {
                "all" => "Tất cả",
                "iphone" => "iPhone",
                "samsung" => "Samsung",
                "xiaomi" => "Xiaomi",
                "oppo" => "OPPO",
                _ => category.ToUpper()
            };
        }

        public static string GetAlertClass(this int soLuongTon)
        {
            return soLuongTon <= 5 ? "table-danger" : soLuongTon <= 10 ? "table-warning" : "";
        }

        public static string GetBadgeClass(this int soLuongTon)
        {
            return soLuongTon <= 5 ? "bg-danger" : soLuongTon <= 10 ? "bg-warning" : "bg-success";
        }

        public static bool IsInCategory(this string productName, string category)
        {
            if (string.IsNullOrEmpty(productName) || category == "all")
                return true;

            return productName.ToLower().Contains(category.ToLower());
        }
    }
}