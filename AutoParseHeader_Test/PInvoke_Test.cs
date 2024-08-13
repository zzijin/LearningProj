using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoParseHeader_Test
{
    //用于研究PInvoke
    internal static class PInvoke_Test
    {
        [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcessId", CallingConvention = CallingConvention.Winapi)]
        public static extern int GetCurrentProcessId();
    }
}
