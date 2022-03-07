using System.Diagnostics;

namespace CustomUnity
{
    public abstract class MonoBehaviour : UnityEngine.MonoBehaviour
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogInfo(object message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.Log(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogInfo(string message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.Log(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogWarning(object message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.LogWarning(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogWarning(string message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.LogWarning(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogError(object message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.LogError(message, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogError(string message)
        {
            if(!Log.PassFilter(this, message)) return;
            UnityEngine.Debug.LogError(message, this);
        }

        protected void LogException(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception, this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogTrace()
        {
            var callerFrame = new StackFrame(1, true);
            var callerMethod = callerFrame.GetMethod();
            if(!Log.PassFilter(this, callerFrame)) return;
            UnityEngine.Debug.Log($"Pass {callerMethod.DeclaringType.Name}.{callerMethod.Name} (at {callerFrame.GetFileName()}:{callerFrame.GetFileLineNumber()})", this);
        }
    }
}
