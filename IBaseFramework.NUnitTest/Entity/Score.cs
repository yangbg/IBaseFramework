using IBaseFramework.Base;
using IBaseFramework.Infrastructure;

namespace IBaseFramework.NUnitTest.Entity
{
    public class Score : EntityBaseEmpty
    {
        [Key]
        public string Id { get; set; }

        public long UserId { get; set; }
        public decimal ScoreData { get; set; }
    }
}