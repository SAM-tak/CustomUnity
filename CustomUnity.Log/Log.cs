using System.Diagnostics;
using UnityEngine;

// These wrappers ship as a precompiled plugin, so #if cannot adapt them to the Unity
// version that consumes them - whatever was defined when the dll was built is baked in.
// [Conditional] is different: the caller's compiler evaluates it, and several of them
// are OR'd together. Listing both instrumentation symbols therefore covers every
// supported editor from one dll:
//   UNITY_INCLUDE_INSTRUMENTATION  Unity 6.6+, defined by the Debug/Checked/Instrumented
//                                  Managed Code Variant (the default Release defines it
//                                  for no variant, so diagnostics need Checked or above).
//   DEVELOPMENT_BUILD              Unity 6.5 and older, where the symbol above does not
//                                  exist yet; deprecated in 6.6 and removed in 6.8.
// Drop DEVELOPMENT_BUILD once package.json requires 6000.6 or newer.
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        static public void Info(object message)
        {
            if (!PassFilter(message)) return;
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        static public void Info(string message)
        {
            if(!PassFilter(message)) return;
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        static public void Info(UnityEngine.Object context, object message)
        {
            if(!PassFilter(context, message)) return;
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        static public void Info(UnityEngine.Object context, string message)
        {
            if(!PassFilter(context, message)) return;
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        public static void Error(object message)
        {
            if(!PassFilter(message)) return;
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        public static void Error(string message)
        {
            if(!PassFilter(message)) return;
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        public static void Error(UnityEngine.Object context, object message)
        {
            if(!PassFilter(context, message)) return;
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        public static void Error(UnityEngine.Object context, string message)
        {
            if(!PassFilter(message)) return;
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
        [HideInCallstack]
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
        [HideInCallstack]
        public static void Exception(UnityEngine.Object context, System.Exception exception)
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        public static void Warning(object message)
        {
            if(!PassFilter(message)) return;
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        public static void Warning(string message)
        {
            if(!PassFilter(message)) return;
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        public static void Warning(UnityEngine.Object context, object message)
        {
            if(!PassFilter(context, message)) return;
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        public static void Warning(UnityEngine.Object context, string message)
        {
            if(!PassFilter(context, message)) return;
            UnityEngine.Debug.LogWarning(message, context);
        }

        //
        // 概要:
        //     ///
        //     Logs a trace message to the Unity Console.
        //     ///
        //
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        public static void Trace()
        {
            var callerFrame = new StackFrame(1, true);
            var callerMethod = callerFrame.GetMethod();
            if(!PassFilter(callerFrame)) return;
            UnityEngine.Debug.Log($"Pass {callerMethod.DeclaringType.Name}.{callerMethod.Name} (at {callerFrame.GetFileName()}:{callerFrame.GetFileLineNumber()})");
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
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR"), HideInCallstack]
        public static void Trace(UnityEngine.Object context)
        {
            var callerFrame = new StackFrame(1, true);
            var callerMethod = callerFrame.GetMethod();
            if(!PassFilter(context, callerFrame)) return;
            UnityEngine.Debug.Log($"Pass {callerMethod.DeclaringType.Name}.{callerMethod.Name} (at {callerFrame.GetFileName()}:{callerFrame.GetFileLineNumber()})", context);
        }

        static bool alreadyWarnMessageWasFiltered;

        public static void OnFilterChanged()
        {
            alreadyWarnMessageWasFiltered = false;
        }

        static bool WarnMessageWasFiltered()
        {
            if(!alreadyWarnMessageWasFiltered) {
                alreadyWarnMessageWasFiltered = true;
                UnityEngine.Debug.LogWarning("A log message was filtered.");
            }
            return false;
        }

        internal static bool PassFilter(object message)
            => (FilterByMessage == null || FilterByMessage(message.ToString()))
            && (FilterByCaller == null || FilterByCaller(new StackFrame(3, true)))
            || WarnMessageWasFiltered();
        internal static bool PassFilter(string message)
            => (FilterByMessage == null || FilterByMessage(message))
            && (FilterByCaller == null || FilterByCaller(new StackFrame(3, true)))
            || WarnMessageWasFiltered();
        internal static bool PassFilter(UnityEngine.Object context, object message)
            => (FilterByObject == null || FilterByObject(context))
            && (FilterByMessage == null || FilterByMessage(message.ToString()))
            && (FilterByCaller == null || FilterByCaller(new StackFrame(3, true)))
            || WarnMessageWasFiltered();
        internal static bool PassFilter(UnityEngine.Object context, string message)
            => (FilterByObject == null || FilterByObject(context))
            && (FilterByMessage == null || FilterByMessage(message.ToString()))
            && (FilterByCaller == null || FilterByCaller(new StackFrame(3, true)))
            || WarnMessageWasFiltered();
        internal static bool PassFilter(StackFrame stackFrame)
            => FilterByCaller == null || !FilterByCaller(stackFrame)
            || WarnMessageWasFiltered();
        internal static bool PassFilter(UnityEngine.Object context, StackFrame stackFrame)
            => (FilterByObject == null || FilterByObject(context))
            && (FilterByCaller == null || FilterByCaller(stackFrame))
            || WarnMessageWasFiltered();

        public static event System.Func<UnityEngine.Object, bool> FilterByObject;
        public static event System.Func<string, bool> FilterByMessage;
        public static event System.Func<StackFrame, bool> FilterByCaller;
    }
}
