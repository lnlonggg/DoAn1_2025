namespace QL_CuaHangDienThoai.Helpers
{
    public static class ImageHelper
    {
        public static string GetProductImage(string maDT)
        {
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
            return GetProductImageUrl(maDT);
        }

        public static string GetProductImageUrl(string maDT)
        {
            return maDT.ToUpper() switch
            {
                // Dữ liệu cũ
                "DT01" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-14-pro-max_2.png",
                "DT02" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-s24_1.png",
                "DT03" => "https://cdn.cellphones.com.vn/media/catalog/product/x/i/xiaomi-redmi-note-13_1.png",

                // Samsung (DT050-DT059) - Dùng ảnh từ CellphoneS
                "DT050" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-s24-ultra_3.png",
                "DT051" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-s24-plus_2.png",
                "DT052" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-s24_1.png",
                "DT053" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-s23-fe_1.png",
                "DT054" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-a55-5g_2.png",
                "DT055" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-a35-5g_1.png",
                "DT056" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-a25-5g_1.png",
                "DT057" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-a15-4g_1.png",
                "DT058" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-z-fold-6_1.png",
                "DT059" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-z-flip-6_2.png",

                // iPhone (DT060-DT069) - Dùng ảnh từ CellphoneS
                "DT060" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_3.png",
                "DT061" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro_4.png",
                "DT062" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-15-plus_1.png",
                "DT063" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-15_3.png",
                "DT064" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-14-pro-max_2.png",
                "DT065" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-14_7.png",
                "DT066" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-13_2.png",
                "DT067" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-13-mini_1.png",
                "DT068" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-12_4.png",
                "DT069" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-se-2022_2.png",

                // Xiaomi (DT070-DT079)
                "DT070" => "https://cdn.cellphones.com.vn/media/catalog/product/x/i/xiaomi-14-ultra_1.png",
                "DT071" => "https://cdn.cellphones.com.vn/media/catalog/product/x/i/xiaomi-14_1.png",
                "DT072" => "https://cdn.cellphones.com.vn/media/catalog/product/x/i/xiaomi-13t-pro_1.png",
                "DT073" => "https://cdn.cellphones.com.vn/media/catalog/product/x/i/xiaomi-13t_1.png",
                "DT074" => "https://cdn.cellphones.com.vn/media/catalog/product/r/e/redmi-note-13-pro-plus-5g_1.png",
                "DT075" => "https://cdn.cellphones.com.vn/media/catalog/product/r/e/redmi-note-13-pro-5g_1.png",
                "DT076" => "https://cdn.cellphones.com.vn/media/catalog/product/x/i/xiaomi-redmi-note-13_1.png",
                "DT077" => "https://cdn.cellphones.com.vn/media/catalog/product/r/e/redmi-13c_1.png",
                "DT078" => "https://cdn.cellphones.com.vn/media/catalog/product/p/o/poco-x6-pro-5g_1.png",
                "DT079" => "https://cdn.cellphones.com.vn/media/catalog/product/p/o/poco-f6-5g_1.png",

                // OPPO (DT080-DT089)
                "DT080" => "https://cdn.cellphones.com.vn/media/catalog/product/o/p/oppo-find-x7-ultra_1.png",
                "DT081" => "https://cdn.cellphones.com.vn/media/catalog/product/o/p/oppo-find-x7_1.png",
                "DT082" => "https://cdn.cellphones.com.vn/media/catalog/product/o/p/oppo-reno12-f-5g_1.png",
                "DT083" => "https://cdn.cellphones.com.vn/media/catalog/product/o/p/oppo-reno12-5g_1.png",
                "DT084" => "https://cdn.cellphones.com.vn/media/catalog/product/o/p/oppo-reno11-f-5g_1.png",
                "DT085" => "https://cdn.cellphones.com.vn/media/catalog/product/o/p/oppo-a98-5g_1.png",
                "DT086" => "https://cdn.cellphones.com.vn/media/catalog/product/o/p/oppo-a78-5g_1.png",
                "DT087" => "https://cdn.cellphones.com.vn/media/catalog/product/o/p/oppo-a58-4g_1.png",
                "DT088" => "https://cdn.cellphones.com.vn/media/catalog/product/o/p/oppo-a18_1.png",
                "DT089" => "https://cdn.cellphones.com.vn/media/catalog/product/o/p/oppo-find-n3-flip_1.png",

                // Vivo (DT090-DT096)
                "DT090" => "https://cdn.cellphones.com.vn/media/catalog/product/v/i/vivo-x100-pro_1.png",
                "DT091" => "https://cdn.cellphones.com.vn/media/catalog/product/v/i/vivo-x100_1.png",
                "DT092" => "https://cdn.cellphones.com.vn/media/catalog/product/v/i/vivo-v30e_1.png",
                "DT093" => "https://cdn.cellphones.com.vn/media/catalog/product/v/i/vivo-v29e_1.png",
                "DT094" => "https://cdn.cellphones.com.vn/media/catalog/product/v/i/vivo-y36-4g_1.png",
                "DT095" => "https://cdn.cellphones.com.vn/media/catalog/product/v/i/vivo-y28-5g_1.png",
                "DT096" => "https://cdn.cellphones.com.vn/media/catalog/product/v/i/vivo-y18_1.png",

                // Realme (DT097-DT101)
                "DT097" => "https://cdn.cellphones.com.vn/media/catalog/product/r/e/realme-gt6_1.png",
                "DT098" => "https://cdn.cellphones.com.vn/media/catalog/product/r/e/realme-12-pro-plus-5g_1.png",
                "DT099" => "https://cdn.cellphones.com.vn/media/catalog/product/r/e/realme-12-5g_1.png",
                "DT100" => "https://cdn.cellphones.com.vn/media/catalog/product/r/e/realme-c67-5g_1.png",
                "DT101" => "https://cdn.cellphones.com.vn/media/catalog/product/r/e/realme-c65-5g_1.png",

                // OnePlus (DT102-DT105)
                "DT102" => "https://cdn.cellphones.com.vn/media/catalog/product/o/n/oneplus-12_1.png",
                "DT103" => "https://cdn.cellphones.com.vn/media/catalog/product/o/n/oneplus-11r-5g_1.png",
                "DT104" => "https://cdn.cellphones.com.vn/media/catalog/product/o/n/oneplus-nord-ce4-5g_1.png",
                "DT105" => "https://cdn.cellphones.com.vn/media/catalog/product/o/n/oneplus-nord-n30-5g_1.png",

                // Huawei/Honor (DT106-DT111)
                "DT106" => "https://cdn.cellphones.com.vn/media/catalog/product/h/u/huawei-p60-pro_1.png",
                "DT107" => "https://cdn.cellphones.com.vn/media/catalog/product/h/u/huawei-nova-12i_1.png",
                "DT108" => "https://cdn.cellphones.com.vn/media/catalog/product/h/u/huawei-y9a_1.png",
                "DT109" => "https://cdn.cellphones.com.vn/media/catalog/product/h/o/honor-magic6-pro_1.png",
                "DT110" => "https://cdn.cellphones.com.vn/media/catalog/product/h/o/honor-x9b-5g_1.png",
                "DT111" => "https://cdn.cellphones.com.vn/media/catalog/product/h/o/honor-x7b_1.png",

                // Limited Edition (DT112-DT114)
                "DT112" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_3.png",
                "DT113" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-s24-ultra_3.png",
                "DT114" => "https://cdn.cellphones.com.vn/media/catalog/product/x/i/xiaomi-14-ultra_1.png",

                // Budget Brands (DT115-DT117)
                "DT115" => "https://cdn.cellphones.com.vn/media/catalog/product/n/o/nokia-c32_1.png",
                "DT116" => "https://cdn.cellphones.com.vn/media/catalog/product/i/t/itel-a70_1.png",
                "DT117" => "https://cdn.cellphones.com.vn/media/catalog/product/m/a/masstel-n540_1.png",

                // Refurbished (DT118-DT119)
                "DT118" => "https://cdn.cellphones.com.vn/media/catalog/product/i/p/iphone-11_1_1.png",
                "DT119" => "https://cdn.cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-s22_1.png",

                // Other Brands (DT120-DT125)
                "DT120" => "https://cdn.cellphones.com.vn/media/catalog/product/s/o/sony-xperia-5-v_1.png",
                "DT121" => "https://cdn.cellphones.com.vn/media/catalog/product/n/o/nothing-phone-2a_1.png",
                "DT122" => "https://cdn.cellphones.com.vn/media/catalog/product/g/o/google-pixel-8a_1.png",
                "DT123" => "https://cdn.cellphones.com.vn/media/catalog/product/a/s/asus-rog-phone-8_1.png",
                "DT124" => "https://cdn.cellphones.com.vn/media/catalog/product/m/o/motorola-edge-50-pro_1.png",
                "DT125" => "https://cdn.cellphones.com.vn/media/catalog/product/t/e/tecno-camon-30-pro_1.png",

                _ => "https://via.placeholder.com/300x300/f8f9fa/6c757d?text=No+Image"
            };
        }

        public static async Task<string?> SaveProductImage(IFormFile file, string maDT)
        {
            if (file == null || file.Length == 0)
                return null;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Chỉ cho phép file ảnh (.jpg, .jpeg, .png, .gif)");
            }
            var fileName = $"{maDT.ToLower()}{extension}";
            var filePath = Path.Combine("wwwroot/images/products", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            DeleteOldProductImage(maDT);
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