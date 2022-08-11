using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optitracker
{
    public static class Debug
    {
        static System.IO.StreamWriter SW = null;
        public static void Start()
        {
            if (!Settings.Debug)
                return;
            if (!System.IO.Directory.Exists("DebugLog"))
                System.IO.Directory.CreateDirectory("DebugLog");
            SW = new System.IO.StreamWriter("DebugLog/" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".debug");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Log("Unhandled:" + e.ExceptionObject.ToString());
            };
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                Log("FirstChange:" + e.Exception.ToString() + ";" + e.Exception.StackTrace.ToString());
            };
        }
        public static void Log(string log)
        {
            if(SW != null)
                SW.WriteLine(DateTime.Now.ToString("HHmmss") + ";" + log);
        }
        public static void Stop()
        {
            if (SW != null)
                SW.Close();
        }
    }
}
