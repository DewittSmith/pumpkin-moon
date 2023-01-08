using System;

namespace PumpkinMoon.Core.Diagnostics
{
    public static class Debug
    {
        public enum Type
        {
            None,
            Error,
            Normal,
            Developer
        }

        public static Type LogLevel;
        private static DebugProvider debugProvider;

        public static void SetProvider(DebugProvider provider)
        {
            debugProvider = provider;
        }

        public static void LogInfo(string message, Type logLevel = Type.Normal)
        {
            if (logLevel > LogLevel)
            {
                return;
            }

            debugProvider?.LogInfo(message);
        }

        public static void LogError(string message, Type logLevel = Type.Error)
        {
            if (logLevel > LogLevel)
            {
                return;
            }

            debugProvider?.LogError(message);
        }

        public static void LogException(Exception exception, Type logLevel = Type.Error)
        {
            if (logLevel > LogLevel)
            {
                return;
            }

            debugProvider?.LogError($"{exception.Message}\n{exception.StackTrace}");
        }

        public static void LogWarning(string message, Type logLevel = Type.Developer)
        {
            if (logLevel > LogLevel)
            {
                return;
            }

            debugProvider?.LogWarning(message);
        }

        public static void Assert(bool condition, string message, Type logLevel = Type.Error)
        {
            if (logLevel > LogLevel)
            {
                return;
            }

            if (!condition)
            {
                debugProvider?.LogError(message);
            }
        }
    }
}