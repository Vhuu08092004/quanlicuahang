using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Quanlicuahang.DTOs.Invoice;
using Quanlicuahang.DTOs.Order;
using System.Globalization;

namespace Quanlicuahang.Services
{

    /// Tạo PDF hóa đơn đơn hàng (QuestPDF).
    /// Lưu ý font: ưu tiên Arial (Windows). Nếu môi trường không có font Unicode,
    /// tiếng Việt có thể hiển thị không đầy đủ.

    public static class OrderInvoicePdf
    {
        public static byte[] Generate(OrderDto order, InvoiceSettingDto? invoiceSetting = null)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            // QuestPDF: license Community/MIT, bật mode Community để tránh warning runtime
            QuestPDF.Settings.License = LicenseType.Community;

            var culture = CultureInfo.GetCultureInfo("vi-VN");

            var storeName = invoiceSetting?.StoreName ?? "";
            var storeAddress = invoiceSetting?.StoreAddress ?? "";
            var storePhone = invoiceSetting?.Phone ?? "";
            var footerNote = "Cảm ơn Quý khách!";

            var gross = order.Items?.Sum(x => x.UnitPrice * x.Quantity) ?? 0m;
            var discount = order.DiscountAmount;
            var net = gross - discount;

            string Money(decimal v) => string.Format(culture, "{0:n0} ₫", v);
            string Dt(DateTime v) => v.ToLocalTime().ToString("dd/MM/yyyy HH:mm", culture);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));

                    page.Header().Column(col =>
                    {
                        // Thông tin cửa hàng với box đẹp
                        if (!string.IsNullOrWhiteSpace(storeName) ||
                            !string.IsNullOrWhiteSpace(storeAddress) ||
                            !string.IsNullOrWhiteSpace(storePhone))
                        {
                            col.Item().Background(Colors.Blue.Lighten5)
                                .Border(1)
                                .BorderColor(Colors.Blue.Lighten2)
                                .Padding(12)
                                .Column(storeInfo =>
                                {
                                    storeInfo.Spacing(4);

                                    if (!string.IsNullOrWhiteSpace(storeName))
                                    {
                                        storeInfo.Item().AlignCenter()
                                            .Text(storeName)
                                            .FontSize(16)
                                            .SemiBold()
                                            .FontColor(Colors.Blue.Darken3);
                                    }

                                    if (!string.IsNullOrWhiteSpace(storeAddress))
                                    {
                                        storeInfo.Item().AlignCenter()
                                            .Text(storeAddress)
                                            .FontSize(11)
                                            .FontColor(Colors.Grey.Darken2);
                                    }

                                    if (!string.IsNullOrWhiteSpace(storePhone))
                                    {
                                        storeInfo.Item().AlignCenter()
                                            .Text($"📞 {storePhone}")
                                            .FontSize(11)
                                            .FontColor(Colors.Grey.Darken2);
                                    }
                                });

                            col.Item().PaddingTop(8);
                        }

                        col.Item().AlignCenter().Text("HÓA ĐƠN BÁN HÀNG").FontSize(18).SemiBold();
                        col.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text(t =>
                            {
                                t.Span("Mã đơn: ").SemiBold();
                                t.Span(order.Code);
                            });
                            r.RelativeItem().AlignRight().Text(t =>
                            {
                                t.Span("Ngày: ").SemiBold();
                                t.Span(Dt(order.CreatedAt));
                            });
                        });
                        col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Text("Thông tin khách hàng").SemiBold();
                        col.Item().Background(Colors.Grey.Lighten5).Padding(8).Column(info =>
                        {
                            info.Spacing(2);
                            info.Item().Text($"Tên: {order.CustomerName ?? "Khách lẻ"}");
                            if (!string.IsNullOrWhiteSpace(order.CustomerPhone))
                                info.Item().Text($"SĐT: {order.CustomerPhone}");
                            if (!string.IsNullOrWhiteSpace(order.CustomerAddress))
                                info.Item().Text($"Địa chỉ: {order.CustomerAddress}");
                            if (!string.IsNullOrWhiteSpace(order.Note))
                                info.Item().Text($"Ghi chú: {order.Note}");
                        });

                        col.Item().Text("Chi tiết sản phẩm").SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35);    // STT 
                                columns.ConstantColumn(90);    // Mã 
                                columns.RelativeColumn(2);      // Tên sản phẩm 
                                columns.ConstantColumn(50);     // SL
                                columns.ConstantColumn(100);    // Đơn giá
                                columns.ConstantColumn(110);    // Thành tiền 
                            });

                            static IContainer CellStyle(IContainer c) =>
                                c.Border(1)
                                 .BorderColor(Colors.Grey.Lighten2)
                                 .PaddingVertical(8)
                                 .PaddingHorizontal(8);

                            static IContainer HeaderCellStyle(IContainer c) =>
                                c.Border(1)
                                 .BorderColor(Colors.Grey.Lighten2)
                                 .Background(Colors.Blue.Lighten5)
                                 .PaddingVertical(8)
                                 .PaddingHorizontal(8);

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("STT").SemiBold().FontSize(10);
                                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Mã SP").SemiBold().FontSize(10);
                                header.Cell().Element(HeaderCellStyle).Text("Tên sản phẩm").SemiBold().FontSize(10);
                                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("SL").SemiBold().FontSize(10);
                                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Đơn giá").SemiBold().FontSize(10);
                                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Thành tiền").SemiBold().FontSize(10);
                            });

                            var items = order.Items ?? new List<OrderItemDto>();
                            for (int i = 0; i < items.Count; i++)
                            {
                                var it = items[i];
                                var lineTotal = it.UnitPrice * it.Quantity;

                                table.Cell().Element(CellStyle).AlignCenter().Text((i + 1).ToString()).FontSize(10);
                                table.Cell().Element(CellStyle).Text(it.ProductCode ?? it.ProductId).FontSize(10);
                                table.Cell().Element(CellStyle).Text(it.ProductName ?? "").FontSize(10);
                                table.Cell().Element(CellStyle).AlignCenter().Text(it.Quantity.ToString(culture)).FontSize(10);
                                table.Cell().Element(CellStyle).AlignRight().Text(Money(it.UnitPrice)).FontSize(10);
                                table.Cell().Element(CellStyle).AlignRight().Text(Money(lineTotal)).SemiBold().FontSize(10);
                            }
                        });

                        col.Item().AlignRight().PaddingTop(8).Column(sum =>
                        {
                            sum.Spacing(2);
                            sum.Item().Row(r =>
                            {
                                r.ConstantItem(160).AlignRight().Text("Tạm tính:").SemiBold();
                                r.ConstantItem(120).AlignRight().Text(Money(gross));
                            });
                            sum.Item().Row(r =>
                            {
                                r.ConstantItem(160).AlignRight().Text("Giảm giá:").SemiBold();
                                r.ConstantItem(120).AlignRight().Text(Money(discount));
                            });
                            sum.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            sum.Item().Row(r =>
                            {
                                r.ConstantItem(160).AlignRight().Text("Tổng thanh toán:").FontSize(12).SemiBold();
                                r.ConstantItem(120).AlignRight().Text(Money(net)).FontSize(12).SemiBold();
                            });
                            sum.Item().PaddingTop(6).AlignRight().Text($"Trạng thái: {order.Status}");
                        });

                        if (order.Payments != null && order.Payments.Count > 0)
                        {
                            col.Item().PaddingTop(10).Text("Thông tin thanh toán").SemiBold();
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(110);
                                    columns.ConstantColumn(140);
                                });

                                static IContainer CellStyle(IContainer c) =>
                                    c.Border(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(6).PaddingHorizontal(6);

                                table.Header(h =>
                                {
                                    h.Cell().Element(CellStyle).Background(Colors.Grey.Lighten4).Text("Phương thức").SemiBold();
                                    h.Cell().Element(CellStyle).Background(Colors.Grey.Lighten4).AlignRight().Text("Số tiền").SemiBold();
                                    h.Cell().Element(CellStyle).Background(Colors.Grey.Lighten4).Text("Thời gian").SemiBold();
                                });

                                foreach (var p in order.Payments.Where(x => !x.IsDeleted))
                                {
                                    table.Cell().Element(CellStyle).Text(p.PaymentMethod);
                                    table.Cell().Element(CellStyle).AlignRight().Text(Money(p.Amount));
                                    table.Cell().Element(CellStyle).Text(Dt(p.PaymentDate));
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(footerNote).FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            });

            return doc.GeneratePdf();
        }
    }
}

