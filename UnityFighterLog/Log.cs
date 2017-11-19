using System.Diagnostics;
using UnityEngine;

namespace UnityFighters
{
    static public class Log
    {
        //
        // 概要:
        //     ///
        //     Logs message to the Unity Console.
        //     ///
        //
        // パラメーター:
        //   message:
        //     String or object to be converted to string representation for display.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Print(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        //
        // 概要:
        //     ///
        //     Logs message to the Unity Console.
        //     ///
        //
        // パラメーター:
        //   message:
        //     String or object to be converted to string representation for display.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Print(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        //
        // 概要:
        //     ///
        //     Logs message to the Unity Console.
        //     ///
        //
        // パラメーター:
        //   message:
        //     String or object to be converted to string representation for display.
        //
        //   context:
        //     Object to which the message applies.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Print(Object context, object message)
        {
            UnityEngine.Debug.Log(message, context);
        }

        //
        // 概要:
        //     ///
        //     Logs message to the Unity Console.
        //     ///
        //
        // パラメーター:
        //   message:
        //     String or object to be converted to string representation for display.
        //
        //   context:
        //     Object to which the message applies.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Print(Object context, string message)
        {
            UnityEngine.Debug.Log(message, context);
        }

        //
        // 概要:
        //     ///
        //     Logs a formatted message to the Unity Console.
        //     ///
        //
        // パラメーター:
        //   format:
        //     A composite format string.
        //
        //   args:
        //     Format arguments.
        //
        //   context:
        //     Object to which the message applies.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Print(Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(context, format, args);
        }

        //
        // 概要:
        //     ///
        //     Logs a formatted message to the Unity Console.
        //     ///
        //
        // パラメーター:
        //   format:
        //     A composite format string.
        //
        //   args:
        //     Format arguments.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Print(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }

        //
        // 概要:
        //     ///
        //     A variant of Debug.Log that logs an error message to the console.
        //     ///
        //
        // パラメーター:
        //   message:
        //     String or object to be converted to string representation for display.
        //
        //   context:
        //     Object to which the message applies.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Error(Object context, object message)
        {
            UnityEngine.Debug.LogError(message, context);
        }

        //
        // 概要:
        //     ///
        //     A variant of Debug.Log that logs an error message to the console.
        //     ///
        //
        // パラメーター:
        //   message:
        //     String or object to be converted to string representation for display.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Error(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        //
        // 概要:
        //     ///
        //     A variant of Debug.Log that logs an error message to the console.
        //     ///
        //
        // パラメーター:
        //   message:
        //     String or object to be converted to string representation for display.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        //
        // 概要:
        //     ///
        //     Logs a formatted error message to the Unity console.
        //     ///
        //
        // パラメーター:
        //   format:
        //     A composite format string.
        //
        //   args:
        //     Format arguments.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Error(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(format, args);
        }

        //
        // 概要:
        //     ///
        //     Logs a formatted error message to the Unity console.
        //     ///
        //
        // パラメーター:
        //   format:
        //     A composite format string.
        //
        //   args:
        //     Format arguments.
        //
        //   context:
        //     Object to which the message applies.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Error(Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(context, format, args);
        }

        //
        // 概要:
        //     ///
        //     A variant of Debug.Log that logs an error message to the console.
        //     ///
        //
        // パラメーター:
        //   context:
        //     Object to which the message applies.
        //
        //   exception:
        //     Runtime Exception.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Exception(Object context, System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception, context);
        }
        
        //
        // 概要:
        //     ///
        //     A variant of Debug.Log that logs an error message to the console.
        //     ///
        //
        // パラメーター:
        //   exception:
        //     Runtime Exception.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Exception(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        //
        // 概要:
        //     ///
        //     A variant of Debug.Log that logs a warning message to the console.
        //     ///
        //
        // パラメーター:
        //   message:
        //     String or object to be converted to string representation for display.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        //
        // 概要:
        //     ///
        //     A variant of Debug.Log that logs a warning message to the console.
        //     ///
        //
        // パラメーター:
        //   message:
        //     String or object to be converted to string representation for display.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        //
        // 概要:
        //     ///
        //     A variant of Debug.Log that logs a warning message to the console.
        //     ///
        //
        // パラメーター:
        //   message:
        //     String or object to be converted to string representation for display.
        //
        //   context:
        //     Object to which the message applies.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(Object context, object message)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }

        //
        // 概要:
        //     ///
        //     Logs a formatted warning message to the Unity Console.
        //     ///
        //
        // パラメーター:
        //   format:
        //     A composite format string.
        //
        //   args:
        //     Format arguments.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(format, args);
        }

        //
        // 概要:
        //     ///
        //     Logs a formatted warning message to the Unity Console.
        //     ///
        //
        // パラメーター:
        //   format:
        //     A composite format string.
        //
        //   args:
        //     Format arguments.
        //
        //   context:
        //     Object to which the message applies.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(context, format, args);
        }
    }
}
