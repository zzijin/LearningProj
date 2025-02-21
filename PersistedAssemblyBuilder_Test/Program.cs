using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System;

namespace PersistedAssemblyBuilder_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CreateDynamicAssembly_1();
            Console.ReadLine();
        }


        public static void CreateDynamicAssembly_1()
        {
            //创建动态程序集
            var assemblyBuilder = new PersistedAssemblyBuilder(new AssemblyName("Assembly1"), typeof(object).Assembly);
            //创建模块
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Module1");
            //创建类
            var typeBuilder = moduleBuilder.DefineType("Type1", TypeAttributes.Class | TypeAttributes.Public);
            //创建私有字段num
            var field_Num = typeBuilder.DefineField("num", typeof(int), FieldAttributes.Private);

            //创建SetNum方法
            var method_SetNum = typeBuilder.DefineMethod("SetNum", MethodAttributes.Public, typeof(void), new System.Type[] { typeof(int) });
            var ilGenerator_Method_SetNum = method_SetNum.GetILGenerator();
            ilGenerator_Method_SetNum.Emit(OpCodes.Ldarg_0);
            ilGenerator_Method_SetNum.Emit(OpCodes.Ldarg_1);
            ilGenerator_Method_SetNum.Emit(OpCodes.Stfld, field_Num);
            ilGenerator_Method_SetNum.EmitWriteLine("SetNum完成!");
            ilGenerator_Method_SetNum.Emit(OpCodes.Ret);

            //创建GetNum方法
            var method_GetNum = typeBuilder.DefineMethod("GetNum", MethodAttributes.Public, typeof(int), null);
            var ilGenerator_Method_GetNum = method_GetNum.GetILGenerator();
            ilGenerator_Method_GetNum.Emit(OpCodes.Ldarg_0);
            ilGenerator_Method_GetNum.Emit(OpCodes.Ldfld, field_Num);
            ilGenerator_Method_GetNum.EmitWriteLine("GetNum完成!");
            ilGenerator_Method_GetNum.Emit(OpCodes.Ret);

            //创建输出Num方法
            var method_ConsoleNum = typeBuilder.DefineMethod("ConsoleNum", MethodAttributes.Public, typeof(void), null);
            var ilGenerator_Method_ConsoleNum = method_ConsoleNum.GetILGenerator();
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Ldstr, "Num Is {0}");
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Ldarg_0);
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Ldfld, field_Num);
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Box, typeof(int));
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) }));
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            ilGenerator_Method_ConsoleNum.EmitWriteLine("打印完成!");
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Ret);

            typeBuilder.CreateType();//不可在此获取Type

            ////直接运行将抛出异常:The method or operation is not implemented.
            //var obj = assemblyBuilder.CreateInstance("Type1");

            using var stream = new MemoryStream();
            {
                assemblyBuilder.Save(stream);

                //重置内存流的位置
                stream.Position = 0;

                //AssemblyLoadContext 加载程序集
                var loadContext = new AssemblyLoadContext("Assembly1", isCollectible: true);
                var assembly = loadContext.LoadFromStream(stream);

                //加载type
                var type = assembly.GetType("Type1");
                //创建实例
                //var obj = assembly.CreateInstance("Type1"); 
                var obj = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public, null, null, null, null);

                var method_Set = type.GetTypeInfo().GetDeclaredMethod("SetNum");
                method_Set.Invoke(obj, new object[] { 999 });

                var method_Console = type.GetTypeInfo().GetDeclaredMethod("ConsoleNum");
                method_Console.Invoke(obj, null);

                var method_Get = type.GetTypeInfo().GetDeclaredMethod("GetNum");
                var rs = method_Get.Invoke(obj, null);

                //重置内存流的位置
                stream.Position = 0;

                using var fs = new FileStream(@".\DynamicAssembly.dll", FileMode.Create, FileAccess.Write, FileShare.Read);
                stream.CopyTo(fs);

                Console.WriteLine("保存完成!");
            }
        }
    }
}
