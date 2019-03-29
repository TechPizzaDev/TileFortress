using GeneralShare;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace TileFortress
{
    public static class Log
    {
        private static object _syncRoot = new object();
        private static StreamWriter _logWriter;

        public static bool IsInitialized { get; private set; }

        public static void Initialize(string path)
        {
            lock (_syncRoot)
            {
                if (path == null)
                    throw new ArgumentNullException(nameof(path));

                _logWriter = new StreamWriter(new FileStream(path, FileMode.Create));
                _logWriter.AutoFlush = true;
                IsInitialized = true;
            }
        }

        public static void Close()
        {
            lock (_syncRoot)
            {
                _logWriter.Dispose();
                IsInitialized = false;
            }
        }

        public static void Error(object obj, bool showPrefix = true)
        {
            Error(obj.ToString(), showPrefix);
        }

        public static void Error(Exception exception, bool showPrefix = true)
        {
            Error(exception.ToString(), showPrefix);
        }

        public static void Error(string message, bool showPrefix = true)
        {
            BaseLog(message, showPrefix, true, Thread.CurrentThread, "ERROR");
        }

        public static void Warning(object obj, bool showPrefix = true)
        {
            Warning(obj.ToString(), showPrefix);
        }
        
        public static void Warning(string message, bool showPrefix = true)
        {
            BaseLog(message, showPrefix, true, Thread.CurrentThread, "WARN");
        }

        public static void Verbal(object obj, bool showPrefix = true)
        {
            Verbal(obj.ToString(), showPrefix);
        }

        public static void Verbal(string message, bool showPrefix = true)
        {
            BaseLog(message, showPrefix, true, Thread.CurrentThread, null);
        }

        public static void Debug(object obj, bool showPrefix = true)
        {
            if (DebugUtils.IsDebuggerAttached) // check twice to prevent a string allocation
                Debug(obj.ToString(), showPrefix);
        }

        public static void Debug(string message, bool showPrefix = true)
        {
            if (DebugUtils.IsDebuggerAttached)
                BaseLog(message, showPrefix, true, Thread.CurrentThread, "DEBUG");
        }

        public static void Info(object obj, bool showPrefix = true)
        {
            Info(obj == null ? "null" : obj.ToString(), showPrefix);
        }

        public static void Info(string message, bool showPrefix = true)
        {
            BaseLog(message, showPrefix, true, Thread.CurrentThread, null);
        }

        public static void Trace(string message, bool showPrefix = true)
        {
            BaseLog(message, showPrefix, true, Thread.CurrentThread, null);
        }

        private static void BaseLog(string message, bool showPrefix, bool showTime, Thread thread, string type)
        {
            AssertInitialized();

            string prefix = null;
            if (showPrefix && (showTime || thread != null || !string.IsNullOrWhiteSpace(type)))
            {
                string time = showTime ? GetTimeSinceStart() : null;
                string typePart = string.IsNullOrWhiteSpace(type) ? null : type;
                string threadPart = thread != null && !string.IsNullOrWhiteSpace(thread.Name) ?
                    '\'' + thread.Name + '\'' : null;

                prefix = "[" + Join(" ", time, threadPart, typePart) + "] ";
            }
            WriteLine(prefix + message);
        }

        private static string Join(string separator, params string[] values)
        {
            int length = 0;
            foreach(var str in values)
            {
                if (str == null)
                    continue;

                if (length > 0)
                    length += separator.Length;
                length += str.Length;
            }

            var builder = new StringBuilder(length);
            for (int i = 0; i < values.Length; i++)
            {
                string str = values[i];
                if (str == null || str.Length == 0)
                    continue;

                if (builder.Length > 0)
                    builder.Append(separator);
                builder.Append(str);
            }

            return builder.ToString();
        }

        public static string GetTimeSinceStart()
        {
            return DebugUtils.TimeSinceStart.ToHMS();
        }

        public static void LineBreak()
        {
            WriteLine(string.Empty);
        }

        private static void AssertInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("The log is not initialized.");
        }

        private static void WriteLine(string value)
        {
            lock (_syncRoot)
            {
                AssertInitialized();

                _logWriter.WriteLine(value);
                Console.WriteLine(value);
            }
        }
    }
}
