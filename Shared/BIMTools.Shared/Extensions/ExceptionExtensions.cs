using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMTools.Shared.Extensions
{
    public static class ExceptionExtensions
    {
        public static void LogException(this Exception obj)
        {
            System.Diagnostics.Debug.WriteLine(obj.InnerException.Message);
        }
    }
}
