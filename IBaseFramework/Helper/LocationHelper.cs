using System;
using System.Collections.Generic;
using IBaseFramework.Helper.GeoHash;

namespace IBaseFramework.Helper
{
    /// <summary>
    /// 位置帮助类
    /// </summary>
    public class LocationHelper
    {
        #region 计算经纬度距离
        /// <summary>
        /// 计算经纬度距离
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        /// <returns></returns>
        public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var theta = lon1 - lon2;
            var dist = Math.Sin(Deg2Rad(lat1)) * Math.Sin(Deg2Rad(lat2)) + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Cos(Deg2Rad(theta));
            dist = Math.Acos(dist);
            dist = Rad2deg(dist);
            dist = dist * 60 * 1.1515;

            dist *= 1.609344;//转换成千米

            return dist;
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private static double Deg2Rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private static double Rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        #endregion

        #region 生成 GeoHashcode
        /// <summary>
        /// 生成 GeoHashcode
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <returns></returns>
        public static string GetGeoHashcode(double lat, double lng)
        {
            return GeoHashHelper.Encode(lat, lng);
        }
        #endregion

        #region 查找附近的区域的 GeoHashcode
        /// <summary>
        /// 查找附近的区域的 GeoHashcode
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <returns></returns>
        public static List<string> GetAroundGeoHashcode(double lat, double lng)
        {
            return GeoHashHelper.Around(lat, lng);
        } 
        #endregion
    }
}