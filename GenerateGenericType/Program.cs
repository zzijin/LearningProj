using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenerateGenericType
{
    public class ExampleBase { }
    public interface IExampleA { }
    public interface IExampleB { }

    public class ExampleDerived : ExampleBase, IExampleA, IExampleB { }

    internal class Program
    {
        static void Main(string[] args)
        {
            AppDomain myDomain = AppDomain.CurrentDomain;
            AssemblyName myAsmName = new AssemblyName("GenericEmitExample1");
            AssemblyBuilder myAssembly =
                myDomain.DefineDynamicAssembly(myAsmName,
                    AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder myModule =
                myAssembly.DefineDynamicModule(myAsmName.Name,
                   myAsmName.Name + ".dll");

            //获取所需的Type
            Type baseType = typeof(ExampleBase);
            Type interfaceA = typeof(IExampleA);
            Type interfaceB = typeof(IExampleB);

            //定义一个公共类型Sample
            TypeBuilder myType =
                myModule.DefineType("Sample", TypeAttributes.Public);

            Console.WriteLine($"Sample类型是否为泛型类: {myType.IsGenericType}");

            //为类型Sample定义泛型参数
            string[] typeParamNames = { "TFirst", "TSecond" };
            GenericTypeParameterBuilder[] typeParams =
                myType.DefineGenericParameters(typeParamNames);

            Console.WriteLine($"Sample类型是否为泛型类: {myType.IsGenericType}");

            GenericTypeParameterBuilder TFirst = typeParams[0];
            GenericTypeParameterBuilder TSecond = typeParams[1];

            //为类型Sample定义泛型参数TFirst的约束
            TFirst.SetGenericParameterAttributes(
                GenericParameterAttributes.DefaultConstructorConstraint |
                GenericParameterAttributes.ReferenceTypeConstraint);

            //为类型Sample定义泛型参数TSecond的约束
            //要求基类为ExampleBase，并且实现IExampleA与IExampleB接口
            TSecond.SetBaseTypeConstraint(baseType);
            Type[] interfaceTypes = { interfaceA, interfaceB };
            TSecond.SetInterfaceConstraints(interfaceTypes);

            //为类型Sample定义泛型字段ExampleField
            FieldBuilder exField =
                myType.DefineField("ExampleField", TFirst,
                    FieldAttributes.Private);

            //生成List<TFirst>类型
            Type listOf = typeof(List<>);
            Type listOfTFirst = listOf.MakeGenericType(TFirst);
            //生成TFirst[]类型
            Type[] mParamTypes = { TFirst.MakeArrayType() };

            //为类型Sample定义静态公共方法exMethod
            MethodBuilder exMethod =
                myType.DefineMethod("ExampleMethod",
                    MethodAttributes.Public | MethodAttributes.Static,
                    listOfTFirst,
                    mParamTypes);

            //---构造方法exMethod
            //获取方法体构造体
            ILGenerator ilgen = exMethod.GetILGenerator();

            //构造方法exMethod的输入泛型参数:IEnumerable<TFirst>
            Type ienumOf = typeof(IEnumerable<>);
            Type TfromListOf = listOf.GetGenericArguments()[0];
            Type ienumOfT = ienumOf.MakeGenericType(TfromListOf);
            Type[] ctorArgs = { ienumOfT };

            //获取List<TFirst>类型的构造函数
            ConstructorInfo ctorPrep = listOf.GetConstructor(ctorArgs);
            ConstructorInfo ctor =
                TypeBuilder.GetConstructor(listOfTFirst, ctorPrep);

            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Newobj, ctor);
            ilgen.Emit(OpCodes.Ret);

            Type finished = myType.CreateType();
            myAssembly.Save(myAsmName.Name + ".dll");
            //---

            Type[] typeArgs = { typeof(Program), typeof(ExampleDerived) };
            Type constructed = finished.MakeGenericType(typeArgs);
            MethodInfo mi = constructed.GetMethod("ExampleMethod");

            Program[] input = { new Program(), new Program() };
            object[] arguments = { input };

            List<Program> listX =
                (List<Program>)mi.Invoke(null, arguments);

            Console.WriteLine($"输出的List<Program>包含{listX.Count}个元素");

            DisplayGenericParameters(finished);

            Console.ReadLine();
        }
        private static void DisplayGenericParameters(Type t)
        {
            if (!t.IsGenericType)
            {
                Console.WriteLine($"类型{t}不是泛型类型");
                return;
            }
            if (!t.IsGenericTypeDefinition)
            {
                t = t.GetGenericTypeDefinition();
            }

            Type[] typeParameters = t.GetGenericArguments();
            Console.WriteLine($"{Environment.NewLine}类型{t}有{typeParameters.Length}个泛型参数");

            foreach (Type tParam in typeParameters)
            {
                Console.WriteLine($"{Environment.NewLine}泛型参数{tParam}约束:");

                foreach (Type c in tParam.GetGenericParameterConstraints())
                {
                    if (c.IsInterface)
                    {
                        Console.WriteLine($"    接口约束{c}");
                    }
                    else
                    {
                        Console.WriteLine($"    基类约束{c}");
                    }
                }

                ListConstraintAttributes(tParam);
            }
        }

        private static void ListConstraintAttributes(Type t)
        {
            GenericParameterAttributes constraints =
                t.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;

            if ((constraints & GenericParameterAttributes.ReferenceTypeConstraint)
                != GenericParameterAttributes.None)
            {
                Console.WriteLine("    必须为引用类型");
            }

            if ((constraints & GenericParameterAttributes.NotNullableValueTypeConstraint)
                != GenericParameterAttributes.None)
            {
                Console.WriteLine("    不为可空值类型");
            }

            if ((constraints & GenericParameterAttributes.DefaultConstructorConstraint)
                != GenericParameterAttributes.None)
            {
                Console.WriteLine("    包含默认构造函数");
            }
        }
    }
    //public class Sample<TFirst, TSecond> where TFirst : class, new() where TSecond : IExampleA, IExampleB, ExampleBase
    //{
    //    private TFirst ExampleField;

    //    public static List<TFirst> ExampleMethod(TFirst[] P_0)
    //    {
    //        //Error decoding local variables: Signature type sequence must have at least one element.
    //        return new List<TFirst>(P_0);
    //    }
    //}
}
