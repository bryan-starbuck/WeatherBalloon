using System;

namespace WeatherBalloon.Common
{
    /// <summary>
    /// Log Levels to allow filtering logging
    /// </summary>
    public enum LoggingLevel {None = 0, FatalError = 1, Error = 2, Warning = 3, Info = 4, All = 5};

    /// <summary>
    /// Simple logging class 
    /// </summary>
    public static class Logger
    {
        public static LoggingLevel Level = LoggingLevel.All;
        
        /// <summary>
        /// Log message to console if level <= current logging level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public static void Log( LoggingLevel level, string message )
        {
            if (level <= Level)
            {
                Console.WriteLine($"[{DateTime.Now}] {level} | {message}");
            }
        }

        public static void LogInfo(string message)
        {
            Log(LoggingLevel.Info, message);
        }

        public static void LogWarning(string message)
        {
            Log(LoggingLevel.Warning, message);
        }

        public static void LogError(string message)
        {
            Log(LoggingLevel.Error, message);
        }
        public static void LogFatalError(string message)
        {
            Log(LoggingLevel.FatalError, message);
        }

        public static void LogException(Exception ex)
        {
            Log(LoggingLevel.Error, "Exception: "+ ex.GetType());
            Log(LoggingLevel.Error, ex.Message);
            Log(LoggingLevel.Error, ex.StackTrace);
        }
    }
}
