using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;


namespace RawLamNet
{

    public partial class API
    {
        public const string RawLamApiPath = @"RawLamAPI.dll";

        // Temporary, for debugging
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}