using System;

namespace JobsCommon.Logger
{
    public class ConsoleLogger : ILogger
    {
        public void Error(string message)
        {
            Console.Error.WriteLine(message);
        }

        public void Log(string message)
        {
            Console.Out.WriteLine(message);
        }
    }
}
