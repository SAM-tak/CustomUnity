using System.Diagnostics;
using UnityEngine;

namespace CustomUnity
{
    public abstract class UIBehaviour : UnityEngine.EventSystems.UIBehaviour
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), HideInCallstack]
        protected void LogInfo(object message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.Log(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), HideInCallstack]
        protected void LogInfo(string message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.Log(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), HideInCallstack]
        protected void LogWarning(object message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.LogWarning(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), HideInCallstack]
        protected void LogWarning(string message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.LogWarning(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), HideInCallstack]
        protected void LogError(object message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.LogError(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), HideInCallstack]
        protected void LogError(string message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.LogError(message, this);
        }

        [HideInCallstack]
        protected void LogException(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), HideInCallstack]
        protected void LogTrace()
        {
            var callerFrame = new StackFrame(1, true);
            var callerMethod = callerFrame.GetMethod();
            if(!Log.PassFilter(this, callerFrame)) return;
            UnityEngine.Debug.Log($"Pass {callerMethod.DeclaringType.Name}.{callerMethod.Name} (at {callerFrame.GetFileName()}:{callerFrame.GetFileLineNumber()})", this);
        }
    }
}
