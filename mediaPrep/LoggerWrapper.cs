using NLog;
using NLog.Config;
using NLog.Targets;

namespace mediaPrep
{
    public static class LoggerWrapper
    {
        /// <summary>
        /// Set up the Nlog Configuration for the first time
        /// </summary>
        public static void ConfigureNlog()
        {
            var config = new LoggingConfiguration();

            // logging to file
            var fileTarget = new FileTarget
            {
                FileName = typeof(Program).FullName + ".log"
            };
            config.AddTarget("logfile", fileTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));

            // Logging to Console
            var consoleTarget = new ColoredConsoleTarget()
            {
                Layout = @"${message}",
            };
            config.AddTarget("console", consoleTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, consoleTarget));

            // Setup logger
            LogManager.Configuration = config;

        }

        /// <summary>
        /// Reconfigure the Nlog loggers to use trace level logging for super verbose output
        /// </summary>
        public static void EnableTraceLevelLogging()
        {
            foreach (var rule in LogManager.Configuration.LoggingRules)
            {

                rule.EnableLoggingForLevel(LogLevel.Trace);
            }

            LogManager.ReconfigExistingLoggers();
        }
    }
}