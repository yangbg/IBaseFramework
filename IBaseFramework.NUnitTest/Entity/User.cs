using IBaseFramework.Base;
using IBaseFramework.DapperExtension;

namespace IBaseFramework.NUnitTest.Entity
{
    [Table("User")]
    public class User : EntityBase
    {
        [Key]
        public long UserId { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }
    }
}
