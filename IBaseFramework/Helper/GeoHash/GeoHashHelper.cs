using System.Collections.Generic;
using System.Text;

namespace IBaseFramework.Helper.GeoHash
{
    public class GeoHashHelper
    {
        private const double MaxLat = 90;
        private const double MinLat = -90;
        private const double MaxLng = 180;
        private const double MinLng = -180;
        private const int Length = 20;
        private const double LatUnit = (MaxLat - MinLat) / (1 << 20);
        private const double LngUnit = (MaxLng - MinLng) / (1 << 20);
        private static readonly string[] Base32Lookup = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "b", "c", "d", "e", "f", "g", "h", "j", "k", "m", "n", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        private static void Convert(double min, double max, double value, List<char> list)
        {
            while (true)
            {
                if (list.Count > (Length - 1))
                {
                    return;
                }

                var mid = (max + min) / 2;
                if (value < mid)
                {
                    list.Add('0');
                    max = mid;
                }
                else
                {
                    list.Add('1');
                    min = mid;
                }
            }
        }

        private static string Base32Encode(string str)
        {
            var sb = new StringBuilder();
            for (var start = 0; start < str.Length; start += 5)
            {
                var unit = str.Substring(start, 5);
                sb.Append(Base32Lookup[ConvertToIndex(unit)]);
            }
            return sb.ToString();
        }

        private static int ConvertToIndex(string str)
        {
            var length = str.Length;
            var result = 0;
            for (var index = 0; index < length; index++)
            {
                result += str[index] == '0' ? 0 : 1 << (length - 1 - index);
            }
            return result;
        }

        public static string Encode(double lat, double lng)
        {
            var latList = new List<char>();
            var lngList = new List<char>();
            Convert(MinLat, MaxLat, lat, latList);
            Convert(MinLng, MaxLng, lng, lngList);
            var sb = new StringBuilder();
            for (var index = 0; index < latList.Count; index++)
            {
                sb.Append(lngList[index]).Append(latList[index]);
            }
            return Base32Encode(sb.ToString());
        }

        public static List<string> Around(double lat, double lng)
        {
            var list = new List<string>
            {
                Encode(lat, lng),
                Encode(lat + LatUnit, lng),
                Encode(lat - LatUnit, lng),
                Encode(lat, lng + LngUnit),
                Encode(lat, lng - LngUnit),
                Encode(lat + LatUnit, lng + LngUnit),
                Encode(lat + LatUnit, lng - LngUnit),
                Encode(lat - LatUnit, lng + LngUnit),
                Encode(lat - LatUnit, lng - LngUnit)
            };
            return list;
        }
    }
}