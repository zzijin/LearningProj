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
            AssemblyName assemblyName = new AssemblyName("PInvokeWrapper");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);

            // 创建一个模块  
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            ////创建结构体封装类
            //定义持久化模块
            TypeBuilder typeBuilder_Struct = moduleBuilder.DefineType("PInvokeWrapper.StructWrapper", TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, typeof(ValueType));
            //定义字段X
            var fieldBuilder_x = typeBuilder_Struct.DefineField("X", typeof(int), FieldAttributes.Public | FieldAttributes.HasFieldMarshal);
            //定义字段Y
            var fieldBuilder_y = typeBuilder_Struct.DefineField("Y", typeof(int), FieldAttributes.Public | FieldAttributes.HasFieldMarshal);
            //定义字段属性
            var type_MarshalAsAttribute = typeof(MarshalAsAttribute);
            MarshalAsAttribute marshalAsAttribute = new MarshalAsAttribute(UnmanagedType.I4);
            var marshalAsAttribute_fields = type_MarshalAsAttribute.GetFields().ToArray();
            var marshalAsAttribute_values = marshalAsAttribute_fields.Select(x => x.GetValue(marshalAsAttribute)).ToArray();
            CustomAttributeBuilder customAttributeBuilder_MarshalAsAttribute = new CustomAttributeBuilder(type_MarshalAsAttribute.GetConstructor(new Type[] { typeof(UnmanagedType) }),
                new object[] { UnmanagedType.I4 }, marshalAsAttribute_fields, marshalAsAttribute_values);
            //将字段加入类
            fieldBuilder_x.SetCustomAttribute(customAttributeBuilder_MarshalAsAttribute);
            fieldBuilder_y.SetCustomAttribute(customAttributeBuilder_MarshalAsAttribute);
            //建立此类型
            var type_StructWrapper = typeBuilder_Struct.CreateType();
            //实例化结构体封装
            var structWrapper = Activator.CreateInstance(type_StructWrapper);
            //为X、Y赋值为5
            foreach (var field in type_StructWrapper.GetFields())
            {
                field.SetValue(structWrapper, 5);
            }

            ////创建一个函数封装类与实现方法
            TypeBuilder typeBuilder_Method = moduleBuilder.DefineType("PInvokeWrapper.MethodWrapper", TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);
            // 创建一个方法，并指定MethodAttributes为Public|Static|PinvokeImpl，返回类型为int，无输入参数类型
            MethodBuilder methodBuilder = typeBuilder_Method.DefineMethod("SumWrapper", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig, typeof(int), new Type[] { typeof(object) });
            //设置方法的MethodImplAttributes
            methodBuilder.SetImplementationFlags(MethodImplAttributes.Managed | MethodImplAttributes.IL | MethodImplAttributes.PreserveSig);
            //偷个懒，定义好DllImportAttribute属性后自动获取字段值
            Type type_DllImportAttribute = typeof(DllImportAttribute);
            DllImportAttribute dllImportAttribute = new DllImportAttribute("kernel32.dll")
            {
                EntryPoint = "GetCurrentProcessId",
                CallingConvention = CallingConvention.Cdecl,
                CharSet = CharSet.Ansi,
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
            var type = typeBuilder_Method.CreateType();
            var method = type.GetMethod("SumWrapper").CreateDelegate<Func<object, int>>();
            var sum = method(structWrapper);

            Console.WriteLine($"Sum Is {sum}");

            Console.ReadLine();
        }

        static void WrapperGetCurrentProcessId()
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
            var id = PInvokeMethodWrapper_Test.GetCurrentProcessId();
            Console.WriteLine($"进程ID：{id}");
            Console.WriteLine($"{id == id_1}");
        }
    }
}
