using IBaseFramework.Infrastructure;

namespace IBaseFramework.NUnitTest.Entity
{
    [Table("User")]
    public class User : EntityBase
    {
        [Key]
        public long UserId { get; set; }

        public string Name { get; set; }

        public int? Age { get; set; }

        public UserType UserType { get; set; }

    }


    public enum UserType
    {
        VIP = 1,
        Member = 2
    }
}
