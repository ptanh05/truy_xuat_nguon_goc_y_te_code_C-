using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PharmaDNA.Web.Attributes
{
    public class ValidEthereumAddressAttribute : ValidationAttribute
    {
        private static readonly Regex EthereumAddressRegex = new Regex(
            @"^0x[a-fA-F0-9]{40}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public ValidEthereumAddressAttribute()
        {
            ErrorMessage = "Địa chỉ ví Ethereum không hợp lệ. Phải bắt đầu bằng 0x và có 42 ký tự.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true; // Let Required attribute handle null values

            var address = value.ToString();
            if (string.IsNullOrEmpty(address)) return true;

            return EthereumAddressRegex.IsMatch(address);
        }
    }

    public class ValidBatchNumberAttribute : ValidationAttribute
    {
        private static readonly Regex BatchNumberRegex = new Regex(
            @"^[A-Z0-9]{3,20}$",
            RegexOptions.Compiled
        );

        public ValidBatchNumberAttribute()
        {
            ErrorMessage = "Số lô không hợp lệ. Phải chứa 3-20 ký tự chữ hoa và số.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            var batchNumber = value.ToString();
            if (string.IsNullOrEmpty(batchNumber)) return true;

            return BatchNumberRegex.IsMatch(batchNumber);
        }
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        public int DaysInFuture { get; set; } = 0;

        public FutureDateAttribute()
        {
            ErrorMessage = "Ngày phải trong tương lai.";
        }

        public FutureDateAttribute(int daysInFuture)
        {
            DaysInFuture = daysInFuture;
            ErrorMessage = $"Ngày phải ít nhất {daysInFuture} ngày trong tương lai.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            if (value is DateTime date)
            {
                var minDate = DateTime.UtcNow.AddDays(DaysInFuture);
                return date >= minDate;
            }

            return false;
        }
    }

    public class ValidExpiryDateAttribute : ValidationAttribute
    {
        public string ManufacturingDateProperty { get; set; } = string.Empty;

        public ValidExpiryDateAttribute(string manufacturingDateProperty)
        {
            ManufacturingDateProperty = manufacturingDateProperty;
            ErrorMessage = "Hạn dùng phải sau ngày sản xuất ít nhất 1 ngày.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var expiryDate = (DateTime)value;

            // Get manufacturing date from the same object
            var manufacturingDateProperty = validationContext.ObjectType.GetProperty(ManufacturingDateProperty);
            if (manufacturingDateProperty == null)
            {
                return new ValidationResult($"Property {ManufacturingDateProperty} not found.");
            }

            var manufacturingDate = (DateTime)manufacturingDateProperty.GetValue(validationContext.ObjectInstance)!;

            if (expiryDate <= manufacturingDate)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
