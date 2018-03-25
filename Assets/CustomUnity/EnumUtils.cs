using UnityEngine;

namespace CustomUnity
{
    public sealed class EnumUtils
    {
        public static string GetName<T>(T value) where T : struct, System.IComparable, System.IFormattable, System.IConvertible
        {
            return System.Enum.GetName(typeof(T), value);
        }

        public static string Format<T>(T value, string format) where T : struct, System.IComparable, System.IFormattable, System.IConvertible
        {
            return System.Enum.Format(typeof(T), value, format);
        }

        public static T Parse<T>(string value) where T : struct, System.IComparable, System.IFormattable, System.IConvertible
        {
            return Parse<T>(value, false);
        }

        public static T Parse<T>(string value, bool ignoreCase) where T : struct, System.IComparable, System.IFormattable, System.IConvertible
        {
            return (T)System.Enum.Parse(typeof(T), value, false);
        }

        public static T ToObject<T>(object value) where T : struct, System.IComparable, System.IFormattable, System.IConvertible
        {
            return (T)System.Enum.ToObject(typeof(T), value);
        }
    }

    /// <summary>
    /// フラグ(System.Flags属性を持ったEnum)表示するための属性
    /// ただしそのEnumはマスクのような複合フラグを定義に含んではいけない(EditorGUI.MaskFieldが正しく処理できない)
    /// </summary>
	public class EnumFlagsAttribute : PropertyAttribute
    {
    }

    public class ReadOnlyEnumFlagsAttribute : PropertyAttribute
    {
    }
}