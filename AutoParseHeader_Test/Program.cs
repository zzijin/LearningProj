using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AutoParseHeader_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 创建一个动态程序集  
            AssemblyName assemblyName = new AssemblyName("DynamicAssemblyExample");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);

            // 创建一个模块  
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            // 创建一个类型  
            TypeBuilder typeBuilder = moduleBuilder.DefineType("DynamicType", TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            // 创建一个方法，并指定MethodAttributes为Public|Static|PinvokeImpl，返回类型为int，无输入参数类型
            MethodBuilder methodBuilder = typeBuilder.DefineMethod("DynamicMethod", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig, typeof(int), Type.EmptyTypes);
            //设置方法的MethodImplAttributes
            methodBuilder.SetImplementationFlags(MethodImplAttributes.Managed | MethodImplAttributes.IL | MethodImplAttributes.PreserveSig);

            //偷个懒，定义好DllImportAttribute属性后自动获取字段值
            Type type_DllImportAttribute = typeof(DllImportAttribute);
            DllImportAttribute dllImportAttribute = new DllImportAttribute("kernel32.dll")
            {
                EntryPoint = "GetCurrentProcessId",
                CallingConvention = CallingConvention.Winapi,
                CharSet = CharSet.None,
                PreserveSig = true,
            };
            var dllImportAttribute_fields = type_DllImportAttribute.GetFields().ToArray();
            var dllImportAttribute_fieldValue = dllImportAttribute_fields.Select(x => x.GetValue(dllImportAttribute)).ToArray();

            //获取DllImportAttribute的构造器方法
            ConstructorInfo constructorInfo_DllImport = type_DllImportAttribute.GetConstructor(new Type[] { typeof(string) });
            //使用DllImportAttribute的指定的ConstructorInfo并将准备好的字段值放入CustomAttributeBuilder
            CustomAttributeBuilder customAttributeBuilder_DllImport = new CustomAttributeBuilder(constructorInfo_DllImport, new object[] { "kernel32.dll" }, dllImportAttribute_fields, dllImportAttribute_fieldValue);
            //设置动态函数的属性
            methodBuilder.SetCustomAttribute(customAttributeBuilder_DllImport);

            //创建类型并编译指定方法
            var type = typeBuilder.CreateType();
            var func = type.GetMethod("DynamicMethod").CreateDelegate<Func<int>>();

            //调用方法获取进程ID
            var id_1 = func();
            Console.WriteLine($"进程ID：{id_1}");

            //与静态编码的方法进行对比
            var id = PInvoke_Test.GetCurrentProcessId();
            Console.WriteLine($"进程ID：{id}");
            Console.WriteLine($"{id == id_1}");

            Console.ReadLine();
        }


    }
}
