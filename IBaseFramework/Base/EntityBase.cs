using System;
using IBaseFramework.Infrastructure;

namespace IBaseFramework.Base
{
    public abstract class EntityBaseEmpty
    {
        /// <summary>
        /// 是否删除
        /// </summary>
        [DeleteAttribute]
        public bool IsDeleted { get; set; }

        internal virtual void SetCreateAudit()
        {
            IsDeleted = false;
        }

        internal virtual void SetUpdateAudit()
        {
        }
    }

    public abstract class EntityBase<TKey> : EntityBaseEmpty
    {
        /// <summary>
        /// 版本号（控制并发）
        /// </summary>
        public DateTime Version { get; set; }

        /// <summary>
        /// 数据创建用户ID
        /// </summary>
        public TKey CreateUserId { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 数据最后的修改用户ID
        /// </summary>
        public TKey UpdateUserId { get; set; }

        /// <summary>
        /// 数据最后的修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 设置创建记录公共字段
        /// </summary>
        internal override void SetCreateAudit()
        {
            var now = DateTime.Now;
            var userId = Equals(this.CreateUserId, default(TKey)) ? (TKey)DapperContext.GetUserId() : this.CreateUserId;

            IsDeleted = false;
            CreateUserId = userId;
            CreateTime = now;
            UpdateUserId = userId;
            UpdateTime = now;
            Version = now;
        }

        /// <summary>
        /// 设置修改记录公共字段
        /// </summary>
        internal override void SetUpdateAudit()
        {
            var now = DateTime.Now;
            var userId = (TKey)DapperContext.GetUserId();

            UpdateUserId = userId;
            UpdateTime = now;
            Version = now;
        }
    }
}
