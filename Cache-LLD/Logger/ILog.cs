using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache_LLD.Logger
{
    public interface ILog
    {
        void Log(string message);
        void Log(string messageFormat, params object[] args);
    }
}
