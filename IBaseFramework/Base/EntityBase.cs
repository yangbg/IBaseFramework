using System;

namespace IBaseFramework.Base
{
    public partial class EntityBase
    {
        /// <summary>
        /// 版本号（控制并发）
        /// </summary>
        public DateTime Version { get; set; }

        /// <summary>
        /// 数据创建用户ID
        /// </summary>
        public long CreateUserId { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 数据最后的修改用户ID
        /// </summary>
        public long UpdateUserId { get; set; }

        /// <summary>
        /// 数据最后的修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 设置创建记录公共字段
        /// </summary>
        /// <param name="userId"></param>
        internal void SetCreateAudit(long userId)
        {
            var now = DateTime.Now;
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
        /// <param name="userId"></param>
        internal void SetUpdateAudit(long userId)
        {
            var now = DateTime.Now;
            UpdateUserId = userId;
            UpdateTime = now;
            Version = now;
        }
    }
}
