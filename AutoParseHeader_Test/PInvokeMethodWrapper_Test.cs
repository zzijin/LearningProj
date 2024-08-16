using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoParseHeader_Test
{
    //用于研究PInvoke
    internal static class PInvokeMethodWrapper_Test
    {
        [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcessId", CallingConvention = CallingConvention.Winapi)]
        public static extern int GetCurrentProcessId();

        [DllImport("MyCppLibrary_Test.dll", CharSet = CharSet.Ansi, EntryPoint = "Sum", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sum(Point p);
        [DllImport("MyCppLibrary_Test.dll", CharSet = CharSet.Ansi, EntryPoint = "Multi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Multi(Point p);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        [MarshalAs(UnmanagedType.I4)]
        public int x;
        [MarshalAs(UnmanagedType.I4)]
        public int y;
    }
}
