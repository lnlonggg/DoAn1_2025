using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Data;
using QL_CuaHangDienThoai.Helpers;
using QL_CuaHangDienThoai.Models;
using QL_CuaHangDienThoai.ViewModels;

namespace QL_CuaHangDienThoai.Controllers
{
    [Authorize(Policy = "CustomerOnly")]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly VnPayHelper _vnPayHelper;

        public CheckoutController(ApplicationDbContext context, VnPayHelper vnPayHelper)
        {
            _context = context;
            _vnPayHelper = vnPayHelper;
        }

        // GET: Checkout
        public async Task<IActionResult> Index()
        {
            var cart = await GetUserCart();
            if (cart == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            // Lấy thông tin khách hàng để điền sẵn form
            var khachHang = await GetCurrentCustomer();
            var model = new CheckoutViewModel
            {
                Cart = cart,
                HoTenNguoiNhan = khachHang?.HoTen ?? "",
                SoDienThoai = khachHang?.SoDT ?? "",
                DiaChiGiaoHang = "", 
                PhuongThucThanhToan = "COD"
            };

            return View(model);
        }

        // POST: Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            model.Cart = await GetUserCart();

            if (model.Cart == null || !model.Cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra tồn kho
            foreach (var item in model.Cart.Items)
            {
                var product = await _context.DienThoais.FindAsync(item.MaDT);
                if (product == null || product.SoLuongTon < item.SoLuong)
                {
                    ModelState.AddModelError("", $"Sản phẩm {item.TenDT} không đủ hàng trong kho.");
                    return View(model);
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var khachHang = await GetCurrentCustomer();
                if (khachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return View(model);
                }

                string? maNhanVien = null;
                if (User.HasClaim("VaiTro", "admin") || User.HasClaim("VaiTro", "nhanvien"))
                {
                    var tenDangNhap = User.Identity.Name;
                    var nhanVien = await _context.QuanTriViens
                        .FirstOrDefaultAsync(q => q.TenDangNhap == tenDangNhap);
                    maNhanVien = nhanVien?.MaQTV;
                }

                // Tạo hóa đơn
                var orderId = await GenerateOrderId();
                var hoaDon = new HoaDon
                {
                    MaHD = orderId,
                    MaKH = khachHang.MaKH,
                    MaQTV = maNhanVien,
                    NgayLap = DateTime.Now,
                    TongTien = model.Cart.TongTien
                };

                _context.HoaDons.Add(hoaDon);

                _context.HoaDons.Add(hoaDon);

                // Tạo chi tiết hóa đơn và cập nhật tồn kho
                foreach (var item in model.Cart.Items)
                {
                    var product = await _context.DienThoais.FindAsync(item.MaDT);
                    if (product != null)
                    {
                        var chiTiet = new ChiTietHoaDon
                        {
                            MaHD = orderId,
                            MaDT = item.MaDT,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia
                        };
                        _context.ChiTietHoaDons.Add(chiTiet);

                        // Cập nhật tồn kho
                        product.SoLuongTon -= item.SoLuong;
                        _context.Update(product);
                    }
                }

                await _context.SaveChangesAsync();

                // Xử lý thanh toán
                if (model.PhuongThucThanhToan == "VNPay")
                {
                    // Tạo giao dịch thanh toán online
                    var paymentId = Guid.NewGuid().ToString();
                    var payment = new ThanhToanTrucTuyen
                    {
                        MaThanhToan = paymentId,
                        MaHD = orderId,
                        SoTien = model.Cart.TongTien,
                        PhuongThucThanhToan = "VNPay",
                        TrangThai = TrangThaiThanhToan.ChoDuyet,
                        NgayTao = DateTime.Now
                    };
                    _context.ThanhToanTrucTuyens.Add(payment);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Chuyển đến VNPay
                    var returnUrl = Url.Action("PaymentCallback", "Checkout", null, Request.Scheme);
                    var paymentUrl = _vnPayHelper.CreatePaymentUrl(
                        orderId: orderId,
                        amount: model.Cart.TongTien,
                        orderInfo: $"Thanh toan don hang {orderId}",
                        returnUrl: returnUrl!,
                        ipAddress: GetClientIP()
                    );

                    return Redirect(paymentUrl);
                }
                else if (model.PhuongThucThanhToan == "QR")
                {
                    // Chuyển khoản QR
                    var paymentId = Guid.NewGuid().ToString();
                    var payment = new ThanhToanTrucTuyen
                    {
                        MaThanhToan = paymentId,
                        MaHD = orderId,
                        SoTien = model.Cart.TongTien,
                        PhuongThucThanhToan = "QR",
                        TrangThai = TrangThaiThanhToan.ChoDuyet,
                        NgayTao = DateTime.Now,
                        ThongTinThem = "Chờ khách hàng chuyển khoản và admin xác nhận"
                    };
                    _context.ThanhToanTrucTuyens.Add(payment);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Đã tạo đơn hàng {orderId}. Vui lòng chuyển khoản theo QR code.";
                    return RedirectToAction("QRPayment", new { id = orderId });
                }
                else
                {
                    // COD - Thanh toán khi nhận hàng
                    await transaction.CommitAsync();
                    await ClearUserCart();

                    TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: {orderId}";
                    return RedirectToAction("OrderSuccess", new { id = orderId });
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại.";
                return View(model);
            }
        }
        // Callback từ VNPay
        public async Task<IActionResult> PaymentCallback()
        {
            var vnPayResponse = _vnPayHelper.ProcessCallback(Request.Query);

            // Log để debug
            Console.WriteLine($"VNPay Response: Success={vnPayResponse.IsSuccess}, OrderId={vnPayResponse.OrderId}, ResponseCode={vnPayResponse.ResponseCode}");

            if (!string.IsNullOrEmpty(vnPayResponse.OrderId))
            {
                var payment = await _context.ThanhToanTrucTuyens
                    .FirstOrDefaultAsync(p => p.MaHD == vnPayResponse.OrderId);

                if (payment != null)
                {
                    if (vnPayResponse.IsSuccess && vnPayResponse.IsSuccessfulPayment)
                    {
                        // Thanh toán thành công
                        payment.TrangThai = TrangThaiThanhToan.DaThanhToan;
                        payment.NgayCapNhat = DateTime.Now;
                        payment.MaGiaoDichNganHang = vnPayResponse.PaymentId;
                        payment.ThongTinThem = $"Ngân hàng: {vnPayResponse.BankCode}, Phương thức: {vnPayResponse.PaymentMethod}";

                        _context.Update(payment);
                        await _context.SaveChangesAsync();
                        await ClearUserCart();

                        TempData["SuccessMessage"] = "Thanh toán VNPay thành công!";
                        return RedirectToAction("OrderSuccess", new { id = vnPayResponse.OrderId });
                    }
                    else
                    {
                        // Thanh toán thất bại hoặc sai chữ ký
                        payment.TrangThai = TrangThaiThanhToan.ThatBai;
                        payment.NgayCapNhat = DateTime.Now;

                        var errorMessage = !vnPayResponse.IsSuccess ? "Sai chữ ký hoặc dữ liệu không hợp lệ" : GetVnPayErrorMessage(vnPayResponse.ResponseCode);
                        payment.ThongTinThem = $"Mã lỗi: {vnPayResponse.ResponseCode} - {errorMessage}";

                        _context.Update(payment);
                        await _context.SaveChangesAsync();

                        TempData["ErrorMessage"] = $"Thanh toán thất bại: {errorMessage}";
                        return RedirectToAction("PaymentFailed", new { id = vnPayResponse.OrderId });
                    }
                }
                else
                {
                    // Không tìm thấy giao dịch
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin giao dịch.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Trường hợp không có OrderId
            TempData["ErrorMessage"] = "Thông tin giao dịch không đầy đủ.";
            return RedirectToAction("Index", "Cart");
        }
        private string GetVnPayErrorMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
                _ => "Giao dịch thất bại"
            };
        }

        public async Task<IActionResult> OrderSuccess(string id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(ct => ct.DienThoai)
                .Include(h => h.KhachHang)
                .FirstOrDefaultAsync(h => h.MaHD == id);

            if (hoaDon == null)
                return NotFound();

            return View(hoaDon);
        }

        public async Task<IActionResult> PaymentFailed(string id)
        {
            var payment = await _context.ThanhToanTrucTuyens
                .Include(p => p.HoaDon)
                .FirstOrDefaultAsync(p => p.MaHD == id);

            return View(payment);
        }

        // Helper methods
        private async Task<CartViewModel?> GetUserCart()
        {
            var tenDangNhap = User.Identity.Name;
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.TenDangNhap == tenDangNhap);

            if (khachHang == null) return null;

            var cart = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaKH == khachHang.MaKH);

            if (cart == null) return new CartViewModel();

            var cartItems = await _context.ChiTietGioHangs
                .Where(ct => ct.MaGH == cart.MaGH)
                .Include(ct => ct.DienThoai)
                .ToListAsync();

            return new CartViewModel
            {
                MaGH = cart.MaGH,
                Items = cartItems.Select(ct => new CartItemViewModel
                {
                    MaDT = ct.MaDT,
                    TenDT = ct.DienThoai?.TenDT ?? "",
                    HinhAnh = ImageHelper.GetProductImage(ct.MaDT),
                    DonGia = ct.DienThoai?.DonGia ?? 0,
                    SoLuong = ct.SoLuong,
                    SoLuongTon = ct.DienThoai?.SoLuongTon ?? 0
                }).ToList()
            };
        }

        private async Task<KhachHang?> GetCurrentCustomer()
        {
            var tenDangNhap = User.Identity.Name;
            return await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.TenDangNhap == tenDangNhap);
        }

        private async Task ClearUserCart()
        {
            var cart = await GetUserCart();
            if (cart?.MaGH != null)
            {
                var cartItems = await _context.ChiTietGioHangs
                    .Where(ct => ct.MaGH == cart.MaGH)
                    .ToListAsync();

                _context.ChiTietGioHangs.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<string> GenerateOrderId()
        {
            var lastOrder = await _context.HoaDons
                .OrderByDescending(h => h.MaHD)
                .FirstOrDefaultAsync();

            if (lastOrder == null)
                return "HD001";

            if (int.TryParse(lastOrder.MaHD.Substring(2), out int lastNumber))
                return $"HD{(lastNumber + 1):D3}";

            return $"HD{DateTime.Now:yyyyMMddHHmmss}";
        }

        private string GetClientIP()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }

        // Trang hiển thị QR code
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> QRPayment(string id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(ct => ct.DienThoai)
                .Include(h => h.KhachHang)
                .FirstOrDefaultAsync(h => h.MaHD == id);

            if (hoaDon == null)
                return NotFound();

            // Kiểm tra quyền truy cập
            var tenDangNhap = User.Identity.Name;
            if (hoaDon.KhachHang?.TenDangNhap != tenDangNhap)
                return Forbid();

            var payment = await _context.ThanhToanTrucTuyens
                .FirstOrDefaultAsync(p => p.MaHD == id && p.PhuongThucThanhToan == "QR");

            ViewBag.Payment = payment;
            return View(hoaDon);
        }

        // Kiểm tra trạng thái thanh toán (AJAX)
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> CheckPaymentStatus(string orderId)
        {
            var payment = await _context.ThanhToanTrucTuyens
                .FirstOrDefaultAsync(p => p.MaHD == orderId && p.PhuongThucThanhToan == "QR");

            if (payment == null)
                return Json(new { status = "not_found" });

            return Json(new
            {
                status = payment.TrangThai.ToString(),
                message = payment.TrangThai switch
                {
                    TrangThaiThanhToan.ChoDuyet => "Đang chờ xác nhận thanh toán",
                    TrangThaiThanhToan.DaThanhToan => "Thanh toán thành công",
                    TrangThaiThanhToan.ThatBai => "Thanh toán thất bại",
                    TrangThaiThanhToan.DaHuy => "Đơn hàng đã bị hủy",
                    _ => "Không xác định"
                }
            });
        }
    }
}