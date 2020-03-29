namespace IBaseFramework.BaseService
{
    /// <summary>
    /// 分页实体
    /// </summary>
    public class Pager : RequestBase
    {
        private int _pageIndex;
        private int _pageSize;

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex
        {
            get
            {
                if (_pageIndex < 1)
                    return 1;

                return _pageIndex;
            }

            set => _pageIndex = value;
        }

        /// <summary>
        /// 页容量
        /// </summary>
        public int PageSize
        {
            get
            {
                if (_pageSize < 1)
                {
                    _pageSize = 10;
                }

                return _pageSize;
            }

            set => _pageSize = value;
        }
    }
}