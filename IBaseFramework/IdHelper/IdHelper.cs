using System;
using IBaseFramework.Ioc;

namespace IBaseFramework.IdHelper
{
    public class IdHelper
    {        
        public static int WorkId { get; set; }
        public static int DatacenterId { get; set; }

        public static IdHelper Instance => (Singleton<IdHelper>.Instance ?? (Singleton<IdHelper>.Instance = new IdHelper()));

        private static readonly Lazy<IdWorker> LazyWorker = new Lazy<IdWorker>(() =>
        {
            var workerId = 1;
            var datacenterId = 1;
            var sequence = 0;

            if (WorkId > 0)
                workerId = WorkId;
            if (DatacenterId > 0)
                datacenterId = DatacenterId;

            return new IdWorker(workerId, datacenterId, sequence);
        });


        private static IdWorker IdWorker => LazyWorker.Value;

        public long LongId => IdWorker.NextId();

        public Guid Guid => System.Guid.NewGuid();

        public string Guid32 => System.Guid.NewGuid().ToString("N");

        public string GuidStr => System.Guid.NewGuid().ToString();
    }
}
