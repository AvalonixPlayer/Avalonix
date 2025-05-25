using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AvalonixAPI
{
    public static class DiskManager
    {
        public static string EnvPath()
        {
            return Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        }
    }
}