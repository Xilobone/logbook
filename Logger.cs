namespace Logbook
{
    /// <summary>
    /// This class serves as a more structured way to log messages, it has options to write the logs
    /// to the console and/or to a file, and to filter out non-priority messages
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// The different log levels a message can have, ranging from Trace (least urgent)
        /// to Critical (most urgent)
        /// </summary>
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

        /// <summary>
        /// The default logging channel
        /// </summary>
        public static string DefaultChannel = "Default";

        /// <summary>
        /// The database logging channel
        /// </summary>
        public static string DBChannel = "DB";

        private static Dictionary<LogLevel, ConsoleColor> consoleColors = new Dictionary<LogLevel, ConsoleColor>()
{
    { LogLevel.Trace, ConsoleColor.Gray },
    { LogLevel.Debug, ConsoleColor.DarkGray },
    { LogLevel.Info, ConsoleColor.White },
    { LogLevel.Warning, ConsoleColor.Yellow },
    { LogLevel.Error, ConsoleColor.Red },
    { LogLevel.Critical, ConsoleColor.DarkRed }
};
        private static Config? _config;
        private const string LINE_DELIMITER = "--------------------";

        /// <summary>
        /// Sets the configuration of the logger, must be called before the logger can be
        /// ued properly
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        public static void Initialize(IConfiguration configuration)
        {
            _config = new Config();
            configuration.GetSection("channels").Bind(_config.channels);
        }

        /// <summary>
        /// Logs the message with the default log level Info
        /// </summary>
        /// <param name="message">The message to be logged</param>
        /// 
        public static void Log(object? message)
        {
            Log(message, LogLevel.Info);
        }

        /// <summary>
        /// Logs the message with the specified log level, to the default channel
        /// </summary>
        /// <param name="message">The message to be logged</param>
        /// <param name="logLevel">The level to log the message at</param>
        public static void Log(object? message, LogLevel logLevel)
        {
            Log(message, logLevel, DefaultChannel);
        }
        /// <summary>
        /// Logs the message with the specified log level
        /// </summary>
        /// <param name="message">The message to be logged</param>
        /// <param name="logLevel">The level to log the message at</param>
        /// <param name="logChannel">The logging channel to use</param>
        public static void Log(object? message, LogLevel logLevel, string logChannel)
        {
            if (_config == null)
            {
                Console.WriteLine("Logger config not set, cannot properly log messages");

                //write line to console anyway, its better than nothing
                Console.WriteLine(message);

                return;
            }

            if (!_config.channels.TryGetValue(logChannel, out Channel? channel))
            {
                Console.WriteLine($"Channel {logChannel} doesnt exist");
                Console.WriteLine(message);
                return;
            }

            if (logLevel < channel.minLogLevel) return;

            string formatted = $"({DateTime.Now}|{logLevel}) {message}";

            if (channel.writeToConsole)
            {
                Console.ForegroundColor = consoleColors[logLevel];
                Console.WriteLine(formatted);
                Console.ResetColor();
            }

            if (channel.writeToFile)
            {
                try
                {

                    using Stream stream = new FileStream
                    (
                        channel.filePath,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.ReadWrite
                    );

                    using StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(formatted);
                    writer.Close();
                } catch (IOException)
                {
                    Console.WriteLine($"Could not access file");
                }
            }
        }

        /// <summary>
        /// Logs the values on seperate lines with the default loglevel Info
        /// </summary>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <param name="values">The values to be logged</param>
        public static void Log<T>(ICollection<T> values)
        {
            Log(values, "", null, LogLevel.Info);
        }

        /// <summary>
        /// Logs the values on seperate lines with the default loglevel Info
        /// </summary>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <param name="values">The values to be logged</param>
        /// <param name="title">The title to display above the logged values</param>
        public static void Log<T>(ICollection<T> values, string title)
        {
            Log(values, title, null, LogLevel.Info);
        }

        /// <summary>
        /// Logs the values on seperate lines with the default loglevel Info
        /// </summary>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <param name="values">The values to be logged</param>
        /// <param name="title">The title to display above the logged values</param>
        /// <param name="formatter">A formatter to use to display the values in a more readable format</param>
        public static void Log<T>(ICollection<T> values, string title, Func<T, string> formatter)
        {
            Log(values, title, formatter, LogLevel.Info);
        }

        /// <summary>
        /// Logs the values on seperate lines
        /// </summary>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <param name="values">The values to be logged</param>
        /// <param name="title">The title to display above the logged values</param>
        /// <param name="formatter">A formatter to use to display the values in a more readable format</param>
        /// <param name="logLevel">The log level of the values</param>
        public static void Log<T>(ICollection<T> values, string title, Func<T, string>? formatter, LogLevel logLevel)
        {
            Log(values, title, formatter, logLevel, DefaultChannel);
        }
        /// <summary>
        /// Logs the values on seperate lines
        /// </summary>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <param name="values">The values to be logged</param>
        /// <param name="title">The title to display above the logged values</param>
        /// <param name="formatter">A formatter to use to display the values in a more readable format</param>
        /// <param name="logLevel">The log level of the values</param>
        /// <param name="logChannel">The logging channel to use</param>
        public static void Log<T>(ICollection<T> values, string title, Func<T, string>? formatter, LogLevel logLevel, string logChannel)
        {
            if (!string.IsNullOrEmpty(title)) Log(title, logLevel);

            Log(LINE_DELIMITER, logLevel);
            foreach (T value in values)
            {
                if (value == null)
                {
                    Log(null, logLevel);
                    continue;
                }

                object message;

                if (formatter != null) message = formatter(value);
                else message = value;

                Log(message, logLevel);
            }
            Log(LINE_DELIMITER, logLevel);

        }

        private class Config
        {
            public Dictionary<string, Channel> channels { get; set; } = new();
        }
        /// <summary>
        /// Represents the configuration of a channel of the logger
        /// </summary>
        private class Channel
        {
            public bool writeToConsole { get; set; }
            public bool writeToFile { get; set; }
            public string filePath { get; set; } = string.Empty;
            public LogLevel minLogLevel { get; set; } = LogLevel.Info;
        }
    }
}