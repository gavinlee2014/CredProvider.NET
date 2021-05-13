using log4net;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CredProvider.NET
{
    internal static class Logger
    {
        //private static readonly ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static Logger()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log4net.config"));
        }

        public static void Write(string line = null)
        {
            //var method = new StackTrace().GetFrame(1).GetMethod();
            ILog log = log4net.LogManager.GetLogger(new StackTrace().GetFrame(1).GetMethod().DeclaringType);
            log.Debug(line);
        }

    }
}
