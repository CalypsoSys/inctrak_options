using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IncTrak.GoalSetter.Domain
{
    internal class ServiceUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispose"></param>
        public static void SafeDispose(IDisposable dispose)
        {
            try
            {
                if (dispose != null)
                    dispose.Dispose();
            }
            catch
            {
            }
        }

        public static string CleanNonAlpha(string input)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            return rgx.Replace(input, "");
        }
    }
}
