using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GeneratePInvoke_FrameworkTest
{
    internal static class PInvoke_Test
    {
        [DllImport("MyLibrary.dll", CharSet = CharSet.Ansi, EntryPoint = "Sum", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sum(Point p);
        [DllImport("MyLibrary.dll", CharSet = CharSet.Ansi, EntryPoint = "Multi", CallingConvention = CallingConvention.Cdecl)]
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
