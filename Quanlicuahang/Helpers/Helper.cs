using Quanlicuahang.Enum;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Quanlicuahang.Helpers
{
    public static class EnumHelper
    {
        private static string NormalizeKey(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var s = input.Trim().ToLowerInvariant();
            s = s.Replace("_", "").Replace("-", "").Replace(" ", "");

            var normalized = s.Normalize(NormalizationForm.FormD);
            var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }

        public static string GetDisplayName(System.Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? value.ToString();
        }

        public static string GetPaymentMethodName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "Tiền mặt",
                PaymentMethod.BankTransfer => "Chuyển khoản",
                PaymentMethod.Card => "Thẻ tín dụng/ghi nợ",
                PaymentMethod.Momo => "Ví MoMo",
                PaymentMethod.VnPay => "Ví VNPay",
                PaymentMethod.Zalo => "Ví ZaloPay",
                _ => method.ToString()
            };
        }

        public static string GetPaymentStatusName(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "Đang thanh toán",
                PaymentStatus.Completed => "Đã thanh toán",
                PaymentStatus.Cancelled => "Đã hủy",
                PaymentStatus.Failed => "Chưa thanh toán",
                _ => status.ToString()
            };
        }

        public static bool TryParsePaymentMethod(string? input, out PaymentMethod method)
        {
            method = PaymentMethod.Cash;
            var key = NormalizeKey(input);
            if (string.IsNullOrEmpty(key)) return false;

            if (System.Enum.TryParse<PaymentMethod>(input, true, out var parsed))
            {
                method = parsed;
                return true;
            }

            return key switch
            {
                "tienmat" => (method = PaymentMethod.Cash) == PaymentMethod.Cash,
                "cash" => (method = PaymentMethod.Cash) == PaymentMethod.Cash,
                "chuyenkhoan" => (method = PaymentMethod.BankTransfer) == PaymentMethod.BankTransfer,
                "banktransfer" => (method = PaymentMethod.BankTransfer) == PaymentMethod.BankTransfer,
                "thetindung/ghino" => (method = PaymentMethod.Card) == PaymentMethod.Card,
                "thetindungghino" => (method = PaymentMethod.Card) == PaymentMethod.Card,
                "card" => (method = PaymentMethod.Card) == PaymentMethod.Card,
                "vimomo" => (method = PaymentMethod.Momo) == PaymentMethod.Momo,
                "momo" => (method = PaymentMethod.Momo) == PaymentMethod.Momo,
                "vivnpay" => (method = PaymentMethod.VnPay) == PaymentMethod.VnPay,
                "vnpay" => (method = PaymentMethod.VnPay) == PaymentMethod.VnPay,
                "vizalopay" => (method = PaymentMethod.Zalo) == PaymentMethod.Zalo,
                "zalopay" => (method = PaymentMethod.Zalo) == PaymentMethod.Zalo,
                "zalo" => (method = PaymentMethod.Zalo) == PaymentMethod.Zalo,
                _ => false
            };
        }

        public static bool TryParsePaymentStatus(string? input, out PaymentStatus status)
        {
            status = PaymentStatus.Pending;
            var key = NormalizeKey(input);
            if (string.IsNullOrEmpty(key)) return false;

            if (System.Enum.TryParse<PaymentStatus>(input, true, out var parsed))
            {
                status = parsed;
                return true;
            }

            return key switch
            {
                "chuoithanhtoan" => (status = PaymentStatus.Failed) == PaymentStatus.Failed,
                "chuathanhtoan" => (status = PaymentStatus.Failed) == PaymentStatus.Failed,
                "unpaid" => (status = PaymentStatus.Failed) == PaymentStatus.Failed,
                "dangthanhtoan" => (status = PaymentStatus.Pending) == PaymentStatus.Pending,
                "pending" => (status = PaymentStatus.Pending) == PaymentStatus.Pending,
                "processing" => (status = PaymentStatus.Pending) == PaymentStatus.Pending,
                "dathanhtoan" => (status = PaymentStatus.Completed) == PaymentStatus.Completed,
                "completed" => (status = PaymentStatus.Completed) == PaymentStatus.Completed,
                "paid" => (status = PaymentStatus.Completed) == PaymentStatus.Completed,
                "dahuy" => (status = PaymentStatus.Cancelled) == PaymentStatus.Cancelled,
                "cancelled" => (status = PaymentStatus.Cancelled) == PaymentStatus.Cancelled,
                "canceled" => (status = PaymentStatus.Cancelled) == PaymentStatus.Cancelled,
                _ => false
            };
        }

        public static List<object> GetPaymentMethodOptions()
        {
            return System.Enum.GetValues(typeof(PaymentMethod))
                .Cast<PaymentMethod>()
                .Select(m => new
                {
                    Code = m.ToString(),
                    Name = GetPaymentMethodName(m)
                })
                .ToList<object>();
        }

        public static List<object> GetPaymentStatusOptions()
        {
            return new List<object>
            {
                new { Code = PaymentStatus.Failed.ToString(), Name = GetPaymentStatusName(PaymentStatus.Failed) },
                new { Code = PaymentStatus.Pending.ToString(), Name = GetPaymentStatusName(PaymentStatus.Pending) },
                new { Code = PaymentStatus.Completed.ToString(), Name = GetPaymentStatusName(PaymentStatus.Completed) },
                new { Code = PaymentStatus.Cancelled.ToString(), Name = GetPaymentStatusName(PaymentStatus.Cancelled) }
            };
        }

        public static object GetPaymentMethods()
        {
            return GetPaymentMethodOptions();
        }

        public static object GetPaymentStatuses()
        {
            return GetPaymentStatusOptions();
        }

        public static bool TryParseOrderStatus(string? input, out OrderStatus status)
        {
            status = OrderStatus.Pending;
            var key = NormalizeKey(input);
            if (string.IsNullOrEmpty(key)) return false;

            if (System.Enum.TryParse<OrderStatus>(input, true, out var parsed))
            {
                status = parsed;
                return true;
            }

            return key switch
            {
                "dangcho" => (status = OrderStatus.Pending) == OrderStatus.Pending,
                "pending" => (status = OrderStatus.Pending) == OrderStatus.Pending,
                "daxacnhan" => (status = OrderStatus.Confirmed) == OrderStatus.Confirmed,
                "confirmed" => (status = OrderStatus.Confirmed) == OrderStatus.Confirmed,
                "dathanhtoan" => (status = OrderStatus.Paid) == OrderStatus.Paid,
                "paid" => (status = OrderStatus.Paid) == OrderStatus.Paid,
                "dahuy" => (status = OrderStatus.Cancelled) == OrderStatus.Cancelled,
                "cancelled" => (status = OrderStatus.Cancelled) == OrderStatus.Cancelled,
                _ => false
            };
        }

        public static List<object> GetAllPermissions()
        {
            return System.Enum.GetValues(typeof(Permission))
                .Cast<Permission>()
                .Select(p => new
                {
                    Code = p.ToString(),
                    Name = GetDisplayName(p)
                })
                .ToList<object>();
        }

        public static void PrintAllPermissions()
        {
            var list = GetAllPermissions();
            Console.WriteLine("Danh sách quyền:");
            foreach (dynamic item in list)
            {
                Console.WriteLine($"- {item.Code}: {item.Name}");
            }
        }

    }
}
