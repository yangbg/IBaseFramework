namespace IBaseFramework.Extension
{
    public class RegexConst
    {
        public const string RegexEmail = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        public const string RegexUrl = @"[a-zA-z]+://[^\s]*";

        public const string RegexMobile = @"/^1(3[0-9]|4[01456879]|5[0-35-9]|6[2567]|7[0-8]|8[0-9]|9[0-35-9])\d{8}$/";

        public const string RegexIp = @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$";

        public const string RegexTel = @"\d{3}-\d{8}|\d{4}-\d{7}";

        public const string RegexPostCode = @"[1-9]\d{5}(?!\d)";//邮编

        public const string RegexChinese = @"^[\u4e00-\u9fa5]+$";

        public const string RegexNormalStr = @"[\w\d_]+";//字母、数字、下划线

        public const string RegexFloat = @"^(-?\d+)(\.\d+)?$";

        public const string RegexInt = @"^-?[1-9]\d*$";

        public const string RegexNumber = @"^(\-|\+)?\d+(\.\d+)?$";

        public const string RegexEnAndNum = @"^[A-Za-z0-9]+$";//英文和数字

        public const string RegexDate = @"[1-2]{1}[0-9]{3}((-|\/|\.){1}(([0]?[1-9]{1})|(1[0-2]{1}))((-|\/|\.){1}((([0]?[1-9]{1})|([1-2]{1}[0-9]{1})|(3[0-1]{1})))( (([0-1]{1}[0-9]{1})|2[0-3]{1}):([0-5]{1}[0-9]{1}):([0-5]{1}[0-9]{1})(\.[0-9]{3})?)?)?)?$";

        public const string RegexBase64Str = @"[A-Za-z0-9\+\/\=]";

        public const string RegexFat = @"^[+]{0,1}(\d){1,3}[ ]?([-]?((\d)|[ ]){1,12})+$";

        public const string RegexBankCard = @"/^([1-9]{1})(\d{15}|\d{18})$/";

        public const string RegexLng = @"^[\-\+]?(0?\d{1,2}\.\d{1,8}|1[0-7]?\d{1}\.\d{1,8}|180\.0{1,8})$";//经度

        public const string RegexLat = @"^[\-\+]?([0-8]?\d{1}\.\d{1,8}|90\.0{1,8})$";//纬度
    }
}