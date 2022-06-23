using System;
using IBaseFramework.Infrastructure;

namespace IBaseFramework.Id
{
    public class IdHelper
    {
        private static int _workId = 1;
        private static int _datacenterId = 1;
        private static SqlProvider _type = SqlProvider.MySQL;

        public static void SetWorkerId(int workId, int datacenterId = 1)
        {
            if (workId > 0)
                _workId = workId;

            if (datacenterId > 0)
                _datacenterId = datacenterId;
        }

        public static void SetGuidDbType(SqlProvider sqlProvider)
        {
            _type = sqlProvider;
        }

        private static readonly Lazy<IdWorker> LazyWorker = new Lazy<IdWorker>(() =>
        {
            return new IdWorker(_workId, _datacenterId, 0);
        });

        public static long LongId => LazyWorker.Value.NextId();

        public static Guid SequentialGuid => new SequentialGuidWorker().Create(_type);

        public static Guid Guid => Guid.NewGuid();

        public static string Guid32 => Guid.ToString("N");
        public static string SequentialGuid32 => SequentialGuid.ToString("N");
    }
}
