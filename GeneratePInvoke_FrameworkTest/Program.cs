using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GeneratePInvoke_FrameworkTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 创建一个动态程序集  
            AssemblyName assemblyName = new AssemblyName("PInvokeWrapper");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);

            // 创建一个持久化模块  
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule", "PInvokeWrapper.dll");

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

            ////创建一个函数封装类与实现方法
            TypeBuilder typeBuilder_Method = moduleBuilder.DefineType("PInvokeWrapper.MethodWrapper", TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);
            // 创建一个方法，并指定MethodAttributes为Public|Static|PinvokeImpl，返回类型为int，无输入参数类型
            MethodBuilder methodBuilder = typeBuilder_Method.DefineMethod("SumWrapper", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig, typeof(int), new Type[] { type_StructWrapper });
            //设置方法的MethodImplAttributes
            methodBuilder.SetImplementationFlags(MethodImplAttributes.Managed | MethodImplAttributes.IL | MethodImplAttributes.PreserveSig);
            //偷个懒，定义好DllImportAttribute属性后自动获取字段值
            Type type_DllImportAttribute = typeof(DllImportAttribute);
            DllImportAttribute dllImportAttribute = new DllImportAttribute("MyCppLibrary_Test.Dll")
            {
                EntryPoint = "Sum",
                CallingConvention = CallingConvention.Cdecl,
                CharSet = CharSet.Ansi,
                PreserveSig = true,
            };
            var dllImportAttribute_fields = type_DllImportAttribute.GetFields().ToArray();
            var dllImportAttribute_fieldValue = dllImportAttribute_fields.Select(x => x.GetValue(dllImportAttribute)).ToArray();
            //获取DllImportAttribute的构造器方法
            ConstructorInfo constructorInfo_DllImport = type_DllImportAttribute.GetConstructor(new Type[] { typeof(string) });
            //使用DllImportAttribute的指定的ConstructorInfo并将准备好的字段值放入CustomAttributeBuilder
            CustomAttributeBuilder customAttributeBuilder_DllImport = new CustomAttributeBuilder(constructorInfo_DllImport, new object[] { "MyCppLibrary_Test.Dll" }, dllImportAttribute_fields, dllImportAttribute_fieldValue);
            //设置动态函数的属性
            methodBuilder.SetCustomAttribute(customAttributeBuilder_DllImport);
            //创建此类型
            var type = typeBuilder_Method.CreateType();

            assemblyBuilder.Save("PInvokeWrapper.dll");

            Console.WriteLine("创建程序集完成！");
            Console.ReadLine();
        }
    }
}
