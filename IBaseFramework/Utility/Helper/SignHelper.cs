using System;
using System.Collections.Generic;
using System.Linq;
using IBaseFramework.Encryption;

namespace IBaseFramework.Utility.Helper
{
    public class SignHelper
    {
        private SignHelper()
        {
        }

        public static SignHelper Instance()
        {
            return new SignHelper();
        }

        /// <summary>
        /// 根据POST参数对象获取
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="privateKey"></param>
        /// <param name="privateValue"></param>
        /// <returns></returns>
        public string GetSign(Dictionary<string, object> postData, string privateKey = "", string privateValue = "")
        {
            if (postData == null)
                return String.Empty;

            var sortDic = new SortedDictionary<string, object>();
            foreach (var keyValuePair in postData)
            {
                sortDic.Add(keyValuePair.Key, keyValuePair.Value);
            }
            if (!privateKey.IsNullOrEmpty() && !privateValue.IsNullOrEmpty())
                sortDic.Add(privateKey, privateValue);
            var str = string.Join("&", sortDic.Select(u => u.Key + "=" + u.Value));
            var signature = EncryptionFactory.Md5Encrypt(str);
            return signature;
        }
    }
}
