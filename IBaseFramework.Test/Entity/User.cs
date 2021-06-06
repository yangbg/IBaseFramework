using IBaseFramework.Base;
using IBaseFramework.DapperExtension;

namespace IBaseFramework.Test.Entity
{
    [Table("Users")]
    public class User : EntityBase
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int AddressId { get; set; }

        public int PhoneId { get; set; }

        public int OfficePhoneId { get; set; }
    }
}
