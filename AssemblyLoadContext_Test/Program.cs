using System.Reflection;
using System.Runtime.Loader;

namespace AssemblyLoadContext_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Test2();

            Console.ReadLine();
        }

        public static void Test1()
        {
            //加载程序集，调用后卸载
            var rs = LoadAssembly($@"{Environment.CurrentDirectory}\LoadAssembly\Test1\AssemblyLoadContext_TestLib.dll");
            rs.loadContext.Unloading += LoadContext_Unloading;

            var type = Type.GetType("AssemblyLoadContext_TestLib.TestClass");
            Console.WriteLine($"程序集AssemblyLoadContext_TestLib.TestClass在全程序域" + (type == null ? "不可见" : "可见"));

            InvokeFunc(rs.assembly, 999);
            UnloadAssembly(rs.loadContext);
        }

        public static void Test2()
        {
            //反复加载同一程序集，调用后卸载

            var array = Enumerable.Range(0, 3).Select(index =>
            {
                var loading = LoadAssembly($@"{Environment.CurrentDirectory}\LoadAssembly\Test1\AssemblyLoadContext_TestLib.dll");
                loading.loadContext.Unloading += LoadContext_Unloading;
                return (index, loading);
            }).ToArray();

            var type = Type.GetType("AssemblyLoadContext_TestLib.TestClass");
            Console.WriteLine($"程序集AssemblyLoadContext_TestLib.TestClass在全程序域" + (type == null ? "不可见" : "可见"));

            foreach (var item in array)
            {
                InvokeFunc(item.loading.assembly, item.index);
            }

            foreach (var item in array)
            {
                UnloadAssembly(item.loading.loadContext);
            }
        }

        public static void Test3()
        {
            //反复加载非同一但版本一致的程序集，调用后卸载

            var array = Enumerable.Range(1, 3).Select(index =>
            {
                var loading = LoadAssembly($@"{Environment.CurrentDirectory}\LoadAssembly\Test{index}\AssemblyLoadContext_TestLib.dll");
                loading.loadContext.Unloading += LoadContext_Unloading;
                return (index, loading);
            }).ToArray();

            var type = Type.GetType("AssemblyLoadContext_TestLib.TestClass");
            Console.WriteLine($"程序集AssemblyLoadContext_TestLib.TestClass在全程序域" + (type == null ? "不可见" : "可见"));

            foreach (var item in array)
            {
                InvokeFunc(item.loading.assembly, item.index);
            }

            foreach (var item in array)
            {
                UnloadAssembly(item.loading.loadContext);
            }
        }

        //AssemblyLoadContext卸载时的回调
        private static void LoadContext_Unloading(AssemblyLoadContext obj)
        {
            Console.WriteLine("卸载程序集:");
            foreach (var assembly in obj.Assemblies)
            {
                Console.WriteLine(assembly.FullName);
            }
            obj.Unloading -= LoadContext_Unloading;
        }

        //加载程序集
        public static (AssemblyLoadContext loadContext, Assembly assembly) LoadAssembly(string path)
        {
            var loadContext = new CustomAssemblyLoadContext(Path.GetDirectoryName(path));
            var assembly = loadContext.LoadFromAssemblyPath(path);

            return (loadContext, assembly);
        }

        //调用程序集函数
        public static void InvokeFunc(Assembly assembly, int index)
        {
            var type = assembly.GetType("AssemblyLoadContext_TestLib.TestClass");

            var obj = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public, null, null, null, null);

            var method_version = type.GetMethod("GetVersion");
            var version = method_version.Invoke(null, null);
            Console.WriteLine($"程序集版本:{version}");

            var method_Set = type.GetTypeInfo().GetDeclaredMethod("SetNum");
            method_Set.Invoke(obj, new object[] { index });


            var method_Get = type.GetTypeInfo().GetDeclaredMethod("GetNum");
            var rs = method_Get.Invoke(obj, null);
            Console.WriteLine($"外部打印-Num:{rs}");

            var method_Console = type.GetTypeInfo().GetDeclaredMethod("ConsoleNum");
            method_Console.Invoke(obj, null);

            var method_reference = type.GetMethod("TestReference");
            method_reference.Invoke(obj, null);
        }

        //卸载AssemblyLoadContext
        public static void UnloadAssembly(AssemblyLoadContext loadContext)
        {
            loadContext.Unload();
        }
    }

    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        private string _assemblyDirectory;

        public CustomAssemblyLoadContext(string assemblyDirectory)
            : base(isCollectible: true)
        {
            _assemblyDirectory = assemblyDirectory;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            //加载托管代码
            //此函数会在依赖程序集不存在时触发
            //若此函数返回null，则在整个程序域(AppDomain)中查找程序集
            var assemblyPath = Path.Combine(_assemblyDirectory, $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }

        protected override nint LoadUnmanagedDll(string unmanagedDllName)
        {
            //加载非托管代码

            return nint.Zero;
        }
    }
}
