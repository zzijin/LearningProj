using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EMIT_Test
{
    internal class EMIT_Create
    {
        public static void CreateDynamicAssembly_1()
        {
            //创建动态程序集
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Assembly1"), AssemblyBuilderAccess.Run);
            //创建模块
            var module1 = assembly.DefineDynamicModule("Module1");
            //创建类
            var class1 = module1.DefineType("Type1", TypeAttributes.Class | TypeAttributes.Public);
            //创建私有字段num
            var field_Num = class1.DefineField("num", typeof(int), FieldAttributes.Private);

            //创建SetNum方法
            var method_SetNum = class1.DefineMethod("SetNum", MethodAttributes.Public, typeof(void), new System.Type[] { typeof(int) });
            var ilGenerator_Method_SetNum = method_SetNum.GetILGenerator();
            ilGenerator_Method_SetNum.Emit(OpCodes.Ldarg_0);
            ilGenerator_Method_SetNum.Emit(OpCodes.Ldarg_1);
            ilGenerator_Method_SetNum.Emit(OpCodes.Stfld, field_Num);
            ilGenerator_Method_SetNum.EmitWriteLine("Set Over");
            ilGenerator_Method_SetNum.Emit(OpCodes.Ret);

            //创建GetNum方法
            var method_GetNum = class1.DefineMethod("GetNum", MethodAttributes.Public, typeof(int), null);
            var ilGenerator_Method_GetNum = method_GetNum.GetILGenerator();
            ilGenerator_Method_GetNum.Emit(OpCodes.Ldarg_0);
            ilGenerator_Method_GetNum.Emit(OpCodes.Ldfld, field_Num);
            ilGenerator_Method_GetNum.EmitWriteLine("Get Over");
            ilGenerator_Method_GetNum.Emit(OpCodes.Ret);

            //创建输出Num方法
            var method_ConsoleNum = class1.DefineMethod("ConsoleNum", MethodAttributes.Public, typeof(void), null);
            var ilGenerator_Method_ConsoleNum = method_ConsoleNum.GetILGenerator();
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Ldstr, "Num Is {0}");
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Ldarg_0);
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Ldfld, field_Num);
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Box, typeof(int));
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) }));
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            ilGenerator_Method_ConsoleNum.EmitWriteLine("WriteLine Over");
            ilGenerator_Method_ConsoleNum.Emit(OpCodes.Ret);


            var type = class1.CreateType();
            //var type = assembly.GetType("Type1");
            var obj = assembly.CreateInstance("Type1");

            //存储程序集
            //assembly.Save("Assembly1.dll");

            var method_Set = type.GetTypeInfo().GetDeclaredMethod("SetNum");
            method_Set.Invoke(obj, new object[] { 5 });

            var method_Console = type.GetTypeInfo().GetDeclaredMethod("ConsoleNum");
            method_Console.Invoke(obj, null);

            var method_Get = type.GetTypeInfo().GetDeclaredMethod("GetNum");
            var rs = method_Get.Invoke(obj, null);
        }
    }
}
