using System.Collections.Generic;

namespace IBaseFramework.DapperExtension
{
    /// <summary>
    /// 自定义pageModel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T>
    {
        /// <summary>
        /// 每页的的数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页码
        /// </summary>
        public int TotalPage
        {
            get
            {
                var totalPage = TotalCount / PageSize;
                if (TotalCount % PageSize != 0)
                    totalPage++;

                return (int)totalPage;
            }
        }

        /// <summary>
        /// 是否存在上一页
        /// </summary>
        public bool HasPrevious => PageIndex - 1 >= 1;

        /// <summary>
        /// 是否存在下一页
        /// </summary>
        public bool HasNext => PageIndex + 1 <= TotalPage;

        /// <summary>
        ///数据集合
        /// </summary>
        public IEnumerable<T> Items { get; set; }
    }

}
