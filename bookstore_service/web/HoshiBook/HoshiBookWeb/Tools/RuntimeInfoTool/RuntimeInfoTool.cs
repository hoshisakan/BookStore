using System.Runtime.InteropServices;

namespace HoshiBookWeb.Tools.RuntimeInfoTool
{
    public class RuntimeInfo : IRuntimeInfo
    {
        public RuntimeInfo()
        {
        }
        
        public int GetEnvironmentOSPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return 0;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return 1;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return 2;
            }
            else
            {
                return -1;
            }
        }
    }
}