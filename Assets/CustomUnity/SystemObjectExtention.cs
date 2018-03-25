using System.Linq;
using System.Reflection;

namespace CustomUnity
{
    /// <summary>
    /// object型の拡張メソッド
    /// </summary>
    public static class SystemObjectExtensions
    {
        const string SEPARATOR = ",";
        const string FORMAT = "{0}:{1}";

        /// <summary>
        /// すべての公開フィールドの情報を文字列にして返します
        /// </summary>
        public static string ToStringFields<T>(this T obj)
        {
            return string.Join(SEPARATOR,
                               obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                               .Select(c => string.Format(FORMAT, c.Name, c.GetValue(obj))).ToArray());
        }

        /// <summary>
        /// すべての公開プロパティの情報を文字列にして返します
        /// </summary>
        public static string ToStringProperties<T>(this T obj)
        {
            return string.Join(SEPARATOR,
                               obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                               .Where(c => c.CanRead)
                               .Select(c => string.Format(FORMAT, c.Name, c.GetValue(obj, null))).ToArray());
        }

        /// <summary>
        /// すべての公開フィールドと公開プロパティの情報を文字列にして返します
        /// </summary>
        public static string ToStringReflection<T>(this T obj)
        {
            return string.Join(SEPARATOR, new string[] { obj.ToStringFields(), obj.ToStringProperties() });
        }
    }
}