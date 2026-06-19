using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace FinanceSystem.Infrastructure
{
    /// <summary>
    /// Custom binder cho decimal: chấp nhận dấu chấm (.) hoặc dấu phẩy (,)
    /// làm phân cách hàng nghìn theo kiểu VN, ví dụ "500.000.000" → 500000000.
    /// </summary>
    public class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(
                bindingContext.ModelName, valueProviderResult);

            var raw = valueProviderResult.FirstValue;

            if (string.IsNullOrWhiteSpace(raw))
            {
                bindingContext.Result = ModelBindingResult.Success(0m);
                return Task.CompletedTask;
            }

            // Chuẩn hoá: xoá khoảng trắng, xử lý format VN (500.000.000 hoặc 500,000,000)
            var cleaned = raw.Trim();

            // Nếu có cả dấu chấm lẫn dấu phẩy, xác định loại phân cách
            bool hasDot   = cleaned.Contains('.');
            bool hasComma = cleaned.Contains(',');

            string normalized;

            if (hasDot && hasComma)
            {
                // Ví dụ: "1.000.000,50" (VN) hoặc "1,000,000.50" (EN)
                int lastDot   = cleaned.LastIndexOf('.');
                int lastComma = cleaned.LastIndexOf(',');

                if (lastDot > lastComma)
                {
                    // Dấu chấm cuối cùng → thập phân kiểu EN: "1,000,000.50"
                    normalized = cleaned.Replace(",", "");
                }
                else
                {
                    // Dấu phẩy cuối cùng → thập phân kiểu VN: "1.000.000,50"
                    normalized = cleaned.Replace(".", "").Replace(",", ".");
                }
            }
            else if (hasDot)
            {
                // Chỉ có dấu chấm: có thể là "500.000.000" (hàng nghìn) hoặc "3.14" (thập phân)
                var parts = cleaned.Split('.');
                bool allPartsAre3Digits = parts.Length > 1
                    && parts.Skip(1).All(p => p.Length == 3);

                normalized = allPartsAre3Digits
                    ? cleaned.Replace(".", "")   // hàng nghìn VN
                    : cleaned;                    // số thập phân thường
            }
            else if (hasComma)
            {
                // Chỉ có dấu phẩy: "500,000,000" (hàng nghìn EN) hoặc "3,14" (thập phân VN)
                var parts = cleaned.Split(',');
                bool allPartsAre3Digits = parts.Length > 1
                    && parts.Skip(1).All(p => p.Length == 3);

                normalized = allPartsAre3Digits
                    ? cleaned.Replace(",", "")   // hàng nghìn EN
                    : cleaned.Replace(",", ".");  // thập phân VN
            }
            else
            {
                normalized = cleaned; // số nguyên thuần
            }

            if (decimal.TryParse(normalized, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            else
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.ModelName,
                    $"Giá trị '{raw}' không hợp lệ. Vui lòng nhập số, ví dụ: 500000000");
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Provider đăng ký DecimalModelBinder cho mọi kiểu decimal và decimal?
    /// </summary>
    public class DecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var type = context.Metadata.ModelType;
            if (type == typeof(decimal) || type == typeof(decimal?))
                return new DecimalModelBinder();

            return null;
        }
    }
}
