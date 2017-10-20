using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Models.Extensions {
    public static class StringExtensions {
        public static string ToMD5Hash(this string _string, string _hexaDecimalFormat = "x2") {
            var md5 = MD5.Create();
            var sb = new StringBuilder();
            var enumerator = md5.ComputeHash(Encoding.ASCII.GetBytes(_string)).GetEnumerator();
            while (enumerator.MoveNext()) {
                sb.Append(((byte)enumerator.Current).ToString(_hexaDecimalFormat));
            }
            return sb.ToString();
        }
    }
}
