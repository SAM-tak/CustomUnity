using System.Diagnostics;

namespace CustomUnity
{
    public abstract class ScriptableObject : UnityEngine.ScriptableObject
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogInfo(object message)
        {
            UnityEngine.Debug.Log(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogInfo(string message)
        {
            UnityEngine.Debug.Log(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogError(object message)
        {
            UnityEngine.Debug.LogError(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogError(string message)
        {
            UnityEngine.Debug.LogError(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogException(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogTrace()
        {
            var callerFrame = new StackFrame(1, true);
            var callerMethod = callerFrame.GetMethod();
            UnityEngine.Debug.Log($"Pass {callerMethod.DeclaringType.Name}.{callerMethod.Name} (at {callerFrame.GetFileName()}:{callerFrame.GetFileLineNumber()})", this);
        }
    }
}
