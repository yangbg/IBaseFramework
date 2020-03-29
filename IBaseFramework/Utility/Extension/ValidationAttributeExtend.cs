using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IBaseFramework.Utility.Extension
{
    public class ChineseAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            var reg = @"[\u4e00-\u9fa5]";
            return Regex.IsMatch(value.ToString(), reg) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "只能输入中文" : ErrorMessage);
        }
    }

    public class TelAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            var reg = @"\d{3}-\d{8}|\d{4}-\{7,8}";
            return Regex.IsMatch(value.ToString(), reg) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "座机不正确" : ErrorMessage);
        }
    }

    public class IdCardAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            var reg = @"(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)";
            return Regex.IsMatch(value.ToString(), reg) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "身份证不正确" : ErrorMessage);
        }
    }

    public class MobileAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            var reg = @"0?(13|14|15|17|18|19)[0-9]{9}";
            return Regex.IsMatch(value.ToString(), reg) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "手机号码不正确" : ErrorMessage);
        }
    }

    public class EmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            var reg = @"[\w!#$%&'*+/=?^_`{|}~-]+(?:\.[\w!#$%&'*+/=?^_`{|}~-]+)*@(?:[\w](?:[\w-]*[\w])?\.)+[\w](?:[\w-]*[\w])?";
            return Regex.IsMatch(value.ToString(), reg) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "邮箱不正确" : ErrorMessage);
        }
    }

    public class EnAndNumAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            var reg = @"^[A-Za-z0-9]+$";
            return Regex.IsMatch(value.ToString(), reg) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "只能输入英文和数字" : ErrorMessage);
        }
    }

    public class PostCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            var reg = @"[1-9]\d{5}(?!\d)";
            return Regex.IsMatch(value.ToString(), reg) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "邮编不正确" : ErrorMessage);
        }
    }

    public class FaxAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            var reg = @"^[+]{0,1}(\d){1,3}[ ]?([-]?((\d)|[ ]){1,12})+$";
            return Regex.IsMatch(value.ToString(), reg) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "传真不正确" : ErrorMessage);
        }
    }

    public class IpAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            var reg = @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$";
            return Regex.IsMatch(value.ToString(), reg) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "IP地址不正确" : ErrorMessage);
        }
    }
}