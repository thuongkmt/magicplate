using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.RFIDTable.Helper
{
    public static class Expand
    {  
        public static string Hash(this string s) { return BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", "").ToLower(); }
        public static bool IsEmpty(this string s) { return string.IsNullOrEmpty(s); }
        public static string format(this string s, params object[] args) { return string.Format(s, args); }
        public static int ToInt(this object o, int def = 0) { if (o == null || o is DBNull || o.ToString().Trim() == "") return def; return int.Parse(o.ToString()); }
        public static DateTime ToDateTime(this object o) { if (o == null || o is DBNull || o.ToString().Trim() == "") return DateTime.MinValue; return DateTime.Parse(o.ToString()); }
        public static string ToString2(this DateTime o) { return o == DateTime.MinValue || o < new DateTime(2000, 1, 2) ? "" : o.ToString("MM-dd HH:mm:ss"); }
        public static string ToStringFull(this DateTime o) { return o == DateTime.MinValue || o < new DateTime(2000, 1, 2) ? "" : o.ToString("yyyy-MM-dd HH:mm:ss"); }
        public static string Join(this string[] o, string p) { return string.Join(p, o); }
        public static string[] Sort(this string[] o) { Array.Sort(o); return o; }
        public static string Match(this string s, string p) { return Regex.Match(s, p).Value; }
        public static string replace(this string s, string p, string r) { return Regex.Replace(s, p, r); }
        public static string[] Splic(this string s, int length)
        {
            if (s.Length == 0) return new string[0];
            int len = s.Length / length;
            if (s.Length % length > 0) len++;
            len--;
            string[] a = new string[len + 1];
            for (int i = 0; i < len; i++) a[i] = s.Substring(i * length, length);
            a[len] = s.Substring(len * length, s.Length % length > 0 ? s.Length % length : length); 
            return a;
        }
    }
}
