using System;

namespace IBaseFramework.IdHelper
{
    public class IdHelper
    {
        public static int WorkId { get; set; }
        public static int DatacenterId { get; set; }

        [ThreadStatic]
        static IdWorker _idWorker;
        private static IdWorker IdWorker
        {
            get
            {
                if (_idWorker != null)
                    return _idWorker;

                var workerId = 1;
                var datacenterId = 1;

                if (WorkId > 0)
                    workerId = WorkId;
                if (DatacenterId > 0)
                    datacenterId = DatacenterId;

                _idWorker = new IdWorker(workerId, datacenterId, 0);
                return _idWorker;
            }
        }

        public static long LongId => IdWorker.NextId();

        public static Guid Guid => System.Guid.NewGuid();

        public static string Guid32 => System.Guid.NewGuid().ToString("N");

        public static string GuidStr => System.Guid.NewGuid().ToString();
    }
}
