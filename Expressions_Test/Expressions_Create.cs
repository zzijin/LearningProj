using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressions_Test
{
    internal class Expressions_Create
    {
        public static void CreateExpressions_1()
        {
            //将一个Lambda表达式转换为表达式树形的节点
            Expression<Func<int, int, int>> exprTree = (num1, num2) => num1 + num2;

            //动态生成方法并执行
            var func = exprTree.Compile();
            var rs = func(1, 2);

            Console.WriteLine(rs);
        }

        public static void CreateExpressions_2()
        {
            //生成表达式树
            var num1 = Expression.Parameter(typeof(int));
            var num2 = Expression.Parameter(typeof(int));
            var sum = Expression.AddAssign(num1, num2);
            //动态生成方法并执行
            var lambda = Expression.Lambda<Func<int, int, int>>(sum, num1, num2);
            var func = lambda.Compile();

            var rs = func(1, 2);
        }
    }
}
