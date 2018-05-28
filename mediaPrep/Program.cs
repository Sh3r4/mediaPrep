using System;
using NLog;

namespace mediaPrep
{
    class Program
    {
        private static Logger _logger;
        static void Main(string[] args)
        {
            LoggerWrapper.ConfigureNlog();
            _logger = LogManager.GetCurrentClassLogger();

            // Catch any propogated fatal errors
            try
            {
                var runner = new PrepRunner();
                runner.Run(args);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "A Fatal exception has caused the program to be interrupted and exit");
            }
        }
    }
}
