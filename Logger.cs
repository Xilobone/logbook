namespace Logbook
{
    public class Logger
    {
        public enum LogLevel
        {
            /// <summary>
            /// Most verbose logging level, used to follow the application flow
            /// </summary>
            Trace,

            /// <summary>
            /// Shows any decision step
            /// </summary>
            Debug,

            /// <summary>
            /// Shows all significant steps
            /// </summary>
            Info,

            /// <summary>
            /// Shows any step or state in which something unexpected occurs where the program is
            /// able to recover from without any intervention
            /// </summary>
            Warning,

            /// <summary>
            /// Shows any step or state in which omething unexpected occurs where the program is not
            /// able to fully recover from. The program may be in an invalid state
            /// </summary>
            Error,

            /// <summary>
            /// Shows any exception that completely halts the program. The program will most likely be stuck or
            /// have crashed after this
            /// </summary>
            Critical
        }
        private static Config? _config;
        private const string LINE_DELIMITER = "--------------------";

        public static void Initialize(IConfiguration configuration)
        {
            _config = new Config()
            {
                writeToConsole = bool.Parse(configuration["writeToConsole"] ?? "true"),
                writeToFile = bool.Parse(configuration["writeToFile"] ?? "false"),
                filePath = configuration["filePath"] ?? "",
                minLogLevel = Enum.Parse<LogLevel>(configuration["minLogLevel"] ?? "Info")
            };
        }

        public static void Log(string message)
        {
            Log(message, LogLevel.Info);
        }

        public static void Log(string message, LogLevel logLevel)
        {
            if (_config == null)
            {
                Console.WriteLine("Logger config not set, cannot properly log messages");

                //write line to console anyway, its better than nothing
                Console.WriteLine(message);

                return;
            }

            if (logLevel < _config.minLogLevel) return;

            string formatted = $"({DateTime.Now}|{logLevel}) {message}";

            if (_config.writeToConsole) Console.WriteLine(formatted);

            if (_config.writeToFile)
            {
                StreamWriter writer = File.AppendText(_config.filePath);
                writer.WriteLine(formatted);
                writer.Close();
            }
        }

        public static void Log<T>(ICollection<object> values)
        {
            Log(values, "", null, LogLevel.Info);
        }

        public static void Log<T>(ICollection<T> values, string title)
        {
            Log(values, title, null, LogLevel.Info);
        }

        public static void Log<T>(ICollection<T> values, string title, Func<T,string> formatter)
        {
            Log(values, title, formatter, LogLevel.Info);
        }

        public static void Log<T>(ICollection<T> values, string title, Func<T,string>? formatter, LogLevel logLevel)
        {
            if (!string.IsNullOrEmpty(title)) Log(title, logLevel);

            Log(LINE_DELIMITER, logLevel);
            foreach (T value in values)
            {
                if (value == null)
                {
                    Log("null", logLevel);
                    continue;
                }

                string message;
                    
                if (formatter != null) message = formatter(value);
                else message = value.ToString() ?? "null";

                Log(message, logLevel);
            }
            Log(LINE_DELIMITER, logLevel);

        }
        /// <summary>
        /// Represents the configuration of the logger
        /// </summary>
        private class Config
        {
            public bool writeToConsole { get; set; }
            public bool writeToFile { get; set; }
            public string filePath { get; set; } = string.Empty;
            public LogLevel minLogLevel { get; set; } = LogLevel.Info;
        }
    }
}