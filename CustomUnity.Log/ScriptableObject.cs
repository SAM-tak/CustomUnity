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
        protected void LogInfo(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(this, format, args);
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
        protected void LogWarning(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(this, format, args);
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
        protected void LogError(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(this, format, args);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void LogException(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception, this);
        }
    }
}
