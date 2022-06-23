using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IBaseFramework.Extension
{
    public class ChineseAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return Regex.IsMatch(value.ToString(), RegexConst.RegexChinese) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "只能输入中文" : ErrorMessage);
        }
    }

    public class TelAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return Regex.IsMatch(value.ToString(), RegexConst.RegexTel) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "座机不正确" : ErrorMessage);
        }
    }

    public class IdCardAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return value.ToString().IsIdCard() ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "身份证不正确" : ErrorMessage);
        }
    }

    public class MobileAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return Regex.IsMatch(value.ToString(), RegexConst.RegexMobile) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "手机号码不正确" : ErrorMessage);
        }
    }

    public class EmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return Regex.IsMatch(value.ToString(), RegexConst.RegexEmail) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "邮箱不正确" : ErrorMessage);
        }
    }

    public class EnAndNumAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return Regex.IsMatch(value.ToString(), RegexConst.RegexEnAndNum) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "只能输入英文和数字" : ErrorMessage);
        }
    }

    public class PostCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return Regex.IsMatch(value.ToString(), RegexConst.RegexPostCode) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "邮编不正确" : ErrorMessage);
        }
    }

    public class FaxAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return Regex.IsMatch(value.ToString(), RegexConst.RegexFat) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "传真不正确" : ErrorMessage);
        }
    }

    public class IpAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return Regex.IsMatch(value.ToString(), RegexConst.RegexIp) ? ValidationResult.Success : new ValidationResult(string.IsNullOrEmpty(ErrorMessage) ? "IP地址不正确" : ErrorMessage);
        }
    }
}