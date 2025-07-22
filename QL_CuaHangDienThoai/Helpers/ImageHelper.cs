namespace QL_CuaHangDienThoai.Helpers
{
    public static class ImageHelper
    {
        public static string GetProductImage(string maDT)
        {
            // Kiểm tra file ảnh có tồn tại trong thư mục không
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            foreach (var ext in imageExtensions)
            {
                var imagePath = $"/images/products/{maDT.ToLower()}{ext}";
                var physicalPath = Path.Combine("wwwroot", imagePath.TrimStart('/'));

                if (File.Exists(physicalPath))
                {
                    return imagePath;
                }
            }

            // Nếu không có file local, dùng URL mặc định
            return GetProductImageUrl(maDT);
        }

        // Giữ lại method này để tương thích với Views cũ
        public static string GetProductImageUrl(string maDT)
        {
            return maDT.ToUpper() switch
            {
                "DT01" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-14-pro-max_2.png",
                "DT02" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-s24_1.png",
                "DT03" => "https://cdn.cellphones.com.vn/media/catalog/product/x/i/xiaomi-redmi-note-13_1.png",
                _ => "https://via.placeholder.com/300x300?text=No+Image"
            };
        }

        public static async Task<string?> SaveProductImage(IFormFile file, string maDT)
        {
            if (file == null || file.Length == 0)
                return null;

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Chỉ cho phép file ảnh (.jpg, .jpeg, .png, .gif)");
            }

            // Tạo tên file
            var fileName = $"{maDT.ToLower()}{extension}";
            var filePath = Path.Combine("wwwroot/images/products", fileName);

            // Tạo thư mục nếu chưa có
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            // Xóa file cũ nếu có
            DeleteOldProductImage(maDT);

            // Lưu file mới
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/products/{fileName}";
        }

        public static void DeleteOldProductImage(string maDT)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            foreach (var ext in imageExtensions)
            {
                var filePath = Path.Combine("wwwroot/images/products", $"{maDT.ToLower()}{ext}");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}