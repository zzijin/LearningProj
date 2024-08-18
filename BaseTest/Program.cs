using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BaseTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string str1 = "Hello";
            string str2 = "Hello";
            string input1 = Console.ReadLine();
            //检索引用
            string input2 = string.Intern(input1);

            Console.WriteLine($"对象引用一致？={Object.ReferenceEquals(str2, str1)}");
            Console.WriteLine($"输入对象引用一致？={Object.ReferenceEquals(input1, str1)}");
            Console.WriteLine($"输入对象引用一致？={Object.ReferenceEquals(input2, str1)}");

            //TransformStruct();

            ////定义值类型
            //int i = 5;
            ////装箱操作
            //object o = i; //正常执行
            ////拆箱操作
            //int ii = (int)o; //显示转换为装箱前的类型-正常执行
            //long l = i; //隐式转换-正常执行
            //int? iii = (int?)o; //支持此操作-支持执行
            //long ll = (long)o; //显示转换为其他值-抛出异常InvalidCastException

            //Console.WriteLine(i.GetType()); //System.Int32
            //Console.WriteLine(o.GetType()); //System.Int32
            //Console.WriteLine(i.Equals(o)); //True
            //Console.WriteLine(i.CompareTo(o)); //0
            //Console.WriteLine(o.Equals(i)); //True


            Console.ReadLine();
        }

        //使用new对值类型实例化
        //static void TransformStruct()
        //{
        //    AStruct a = new AStruct();

        //    object o = a;
        //    IChangeParam i = (IChangeParam)o;
        //    i.ChangeParam(55);

        //    Console.WriteLine(a.ToString()); //初始值5
        //    Console.WriteLine(o.ToString()); //更改为55
        //    Console.WriteLine(i.ToString()); //更改为55
        //}

        //static void InitClass()
        //{
        //    var o = new AClass();
        //}
    }

    ////接口定义
    //interface IChangeParam
    //{
    //    void ChangeParam(int i);
    //}
    ////一个值类型定义
    //struct AStruct : IChangeParam
    //{
    //    public int AParam;

    //    public AStruct()
    //    {
    //        this.AParam = 5;
    //    }

    //    public void ChangeParam(int i)
    //    {
    //        AParam = i;
    //    }

    //    public override string ToString()
    //    {
    //        return AParam.ToString();
    //    }
    //}

    ////一个引用类型定义
    //class AClass
    //{
    //    public int AParam;
    //}
}
