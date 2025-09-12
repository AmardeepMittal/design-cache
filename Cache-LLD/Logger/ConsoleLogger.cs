using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache_LLD.Logger
{
    public class ConsoleLogger : ILog
    {

        public void Log(string message)
        {
           Console.WriteLine(message);
        }

        public void Log(string messageFormat, params object[] args)
        {
            Console.WriteLine(string.Format(messageFormat, args));
        }
    }
}
