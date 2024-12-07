namespace Lotec.Utils.Extensions {
    public static class StringExtensions {
        /// <summary>
        /// Truncate a string to maxLength
        /// </summary>
        /// <param name="maxLength"></param>
        /// <returns>A new string that is maximum maxLength long</returns>
        public static string Truncate(this string str, int maxLength) {
            if (string.IsNullOrEmpty(str)) return str;

            return str.Substring(0, System.Math.Min(str.Length, maxLength));
        }
    }
}
