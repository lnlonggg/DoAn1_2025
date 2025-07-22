using System.Security.Cryptography;
using System.Text;

namespace QL_CuaHangDienThoai.Helpers
{
    public class VnPayHelper
    {
        private readonly string _tmnCode;
        private readonly string _hashSecret;
        private readonly string _baseUrl;

        public VnPayHelper(IConfiguration configuration)
        {
            _tmnCode = configuration["VnPay:TmnCode"] ?? "0ZE53AQG";
            _hashSecret = configuration["VnPay:HashSecret"] ?? "U89C105M6Q347VMKQOUW0JSGDXIVO8BA";
            _baseUrl = configuration["VnPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        }

        public string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, string returnUrl, string ipAddress)
        {
            var requestData = new Dictionary<string, string>
            {
                {"vnp_Version", "2.1.0"},
                {"vnp_Command", "pay"},
                {"vnp_TmnCode", _tmnCode},
                {"vnp_Amount", ((long)(amount * 100)).ToString()},
                {"vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")},
                {"vnp_CurrCode", "VND"},
                {"vnp_IpAddr", ipAddress},
                {"vnp_Locale", "vn"},
                {"vnp_OrderInfo", orderInfo},
                {"vnp_OrderType", "other"},
                {"vnp_ReturnUrl", returnUrl},
                {"vnp_TxnRef", orderId},
                {"vnp_SecureHashType", "HMACSHA512"}
            };

            //// Sắp xếp và tạo query string
            //var sortedParams = requestData.OrderBy(x => x.Key);
            //var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            //// Tạo secure hash
            //var secureHash = CreateSecureHash(queryString);

            //return $"{_baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
            
            
            var sortedParams = requestData.OrderBy(x => x.Key);

            // Chuỗi để hash (KHÔNG escape)
            var hashString = string.Join("&", sortedParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));


            // Chuỗi để gửi lên URL (CÓ escape)
            var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            var secureHash = CreateSecureHash(hashString);

            return $"{_baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
           
        }

        public VnPayResponse ProcessCallback(IQueryCollection queryCollection)
        {
            var response = new VnPayResponse();

            try
            {
                // Lấy tất cả tham số trừ vnp_SecureHash
                var responseData = queryCollection
                    .Where(x => x.Key.StartsWith("vnp_") && x.Key != "vnp_SecureHash")
                    .ToDictionary(x => x.Key, x => x.Value.ToString());

                // Sắp xếp và tạo hash string
                var sortedParams = responseData.OrderBy(x => x.Key);
                //var hashString = string.Join("&", sortedParams.Select(x => $"{x.Key}={x.Value}"));
                var hashString = string.Join("&", sortedParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

                // Tạo secure hash để so sánh
                var calculatedHash = CreateSecureHash(hashString);
                var receivedHash = queryCollection["vnp_SecureHash"].ToString();

                response.IsSuccess = calculatedHash.Equals(receivedHash, StringComparison.InvariantCultureIgnoreCase);
                response.OrderId = queryCollection["vnp_TxnRef"];
                response.PaymentId = queryCollection["vnp_TransactionNo"];
                response.ResponseCode = queryCollection["vnp_ResponseCode"];

                if (decimal.TryParse(queryCollection["vnp_Amount"], out var amountValue))
                {
                    response.Amount = amountValue / 100;
                }

                response.PaymentMethod = queryCollection["vnp_CardType"];
                response.BankCode = queryCollection["vnp_BankCode"];
                response.PayDate = queryCollection["vnp_PayDate"];
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessage = ex.Message;
            }

            return response;
        }

        private string CreateSecureHash(string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_hashSecret);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }

    public class VnPayResponse
    {
        public bool IsSuccess { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string PayDate { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsSuccessfulPayment => IsSuccess && ResponseCode == "00";
    }
}