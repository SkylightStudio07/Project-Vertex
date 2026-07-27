using System;
using System.Diagnostics;

public static class Logger
{
    private const string TimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

    [Conditional("UNITY_EDITOR")]
    public static void Log(string msg)
    {
        UnityEngine.Debug.LogFormat(
            "[{0}] {1}",
            DateTime.Now.ToString(TimeFormat),
            msg
        );
    }

    [Conditional("UNITY_EDITOR")]
    public static void Log(object source, string msg)
    {
        UnityEngine.Debug.LogFormat(
            "[{0}] [{1}] {2}",
            DateTime.Now.ToString(TimeFormat),
            GetSourceName(source),
            msg
        );
    }

    [Conditional("UNITY_EDITOR")]
    public static void LogWarning(string msg)
    {
        UnityEngine.Debug.LogWarningFormat(
            "[{0}] {1}",
            DateTime.Now.ToString(TimeFormat),
            msg
        );
    }

    [Conditional("UNITY_EDITOR")]
    public static void LogWarning(object source, string msg)
    {
        UnityEngine.Debug.LogWarningFormat(
            "[{0}] [{1}] {2}",
            DateTime.Now.ToString(TimeFormat),
            GetSourceName(source),
            msg
        );
    }

    public static void LogError(string msg)
    {
        UnityEngine.Debug.LogErrorFormat(
            "[{0}] {1}",
            DateTime.Now.ToString(TimeFormat),
            msg
        );
    }

    public static void LogError(object source, string msg)
    {
        UnityEngine.Debug.LogErrorFormat(
            "[{0}] [{1}] {2}",
            DateTime.Now.ToString(TimeFormat),
            GetSourceName(source),
            msg
        );
    }

    private static string GetSourceName(object source)
    {
        if (source == null)
            return "Unknown";

        if (source is Type type)
            return type.Name;

        return source.GetType().Name;
    }
}   