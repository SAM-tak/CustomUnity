using System.Diagnostics;
using UnityEngine;

namespace CustomUnity
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
        static public void Info(object message)
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
        static public void Info(string message)
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
        static public void Info(Object context, object message)
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
        static public void Info(Object context, string message)
        {
            UnityEngine.Debug.Log(message, context);
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
        //
        //   context:
        //     Object to which the message applies.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Error(Object context, string message)
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
        public static void Warning(Object context, string message)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }

        //
        // 概要:
        //     ///
        //     Logs a trace message to the Unity Console.
        //     ///
        //
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Trace()
        {
            var callerFrame = new StackFrame(1, true);
            UnityEngine.Debug.Log($"Pass {callerFrame.GetMethod().Name} (at {callerFrame.GetFileName()}:{callerFrame.GetFileLineNumber()})");
        }

        //
        // 概要:
        //     ///
        //     Logs a trace message to the Unity Console.
        //     ///
        //
        // パラメーター:
        //   context:
        //     Object to which the message applies.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Trace(Object context)
        {
            var callerFrame = new StackFrame(1, true);
            var callerMethod = callerFrame.GetMethod();
            UnityEngine.Debug.Log($"Pass {callerMethod.DeclaringType.Name}.{callerMethod.Name} (at {callerFrame.GetFileName()}:{callerFrame.GetFileLineNumber()})", context);
        }
    }
}
