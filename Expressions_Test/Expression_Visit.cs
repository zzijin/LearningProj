using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Expressions_Test
{
    internal class Expression_Visit
    {
        public static void CreateExpressions_2()
        {
            //将一个Lambda表达式转换为表达式树形的节点
            Expression<Func<int, bool>> exprTree = num => num < 5;

            ParameterExpression param = (ParameterExpression)exprTree.Parameters[0];
            BinaryExpression operation = (BinaryExpression)exprTree.Body;
            ParameterExpression left = (ParameterExpression)operation.Left;
            ConstantExpression right = (ConstantExpression)operation.Right;

            Console.WriteLine("解析表达式: {0} => {1} {2} {3}",
                              param.Name, left.Name, operation.NodeType, right.Value);

            //创建一个常数节点
            var constant = Expression.Constant(24, typeof(int));

            Console.WriteLine($"节点类型是 {constant.NodeType}");
            Console.WriteLine($"常数节点类型是 {constant.Type}");
            Console.WriteLine($"常数节点值是 {constant.Value}");
        }

        public static void CreateExpressions_3()
        {
            var one = Expression.Constant(1, typeof(int));
            var two = Expression.Constant(2, typeof(int));
            var three = Expression.Constant(3, typeof(int));
            var four = Expression.Constant(4, typeof(int));
            var addition = Expression.Add(one, two);
            var add2 = Expression.Add(three, four);
            var sum = Expression.Add(addition, add2);

            //翻译表达式树并计算出常数节点之和
            //以此来最终调试表达式树
            Func<Expression, int> aggregate = null;
            aggregate = (exp) =>
                exp.NodeType == ExpressionType.Constant ?
                (int)((ConstantExpression)exp).Value :
                aggregate(((BinaryExpression)exp).Left) + aggregate(((BinaryExpression)exp).Right);

            var theSum = aggregate(sum);
            Console.WriteLine(theSum);


            aggregate = Aggregate;
            theSum = aggregate(sum);
            Console.WriteLine(theSum);

            Expression<Func<int>> sum1 = () => 1 + (2 + (3 + 4));
            theSum = aggregate(sum1);
            Console.WriteLine(theSum);
        }
        private static int Aggregate(Expression exp)
        {
            if (exp.NodeType == ExpressionType.Constant)
            {
                var constantExp = (ConstantExpression)exp;
                Console.Error.WriteLine($"Found Constant: {constantExp.Value}");
                if (constantExp.Value is int value)
                {
                    return value;
                }
                else
                {
                    return 0;
                }
            }
            else if (exp.NodeType == ExpressionType.Add)
            {
                var addExp = (BinaryExpression)exp;
                Console.Error.WriteLine("Found Addition Expression");
                Console.Error.WriteLine("Computing Left node");
                var leftOperand = Aggregate(addExp.Left);
                Console.Error.WriteLine($"Left is: {leftOperand}");
                Console.Error.WriteLine("Computing Right node");
                var rightOperand = Aggregate(addExp.Right);
                Console.Error.WriteLine($"Right is: {rightOperand}");
                var sum = leftOperand + rightOperand;
                Console.Error.WriteLine($"Computed sum: {sum}");
                return sum;
            }
            else throw new NotSupportedException("Haven't written this yet");
        }

        public static void CreateExpressions_4()
        {
            Expression<Func<string, bool>> expr = name => name.Length > 10 && name.StartsWith("G");
            Console.WriteLine(expr);

            AndAlsoModifier treeModifier = new AndAlsoModifier();
            Expression modifiedExpr = treeModifier.Modify((Expression)expr);

            Console.WriteLine(modifiedExpr);
        }
        public class AndAlsoModifier : ExpressionVisitor
        {
            public Expression Modify(Expression expression)
            {
                return Visit(expression);
            }

            protected override Expression VisitBinary(BinaryExpression b)
            {
                if (b.NodeType == ExpressionType.AndAlso)
                {
                    Expression left = this.Visit(b.Left);
                    Expression right = this.Visit(b.Right);

                    // Make this binary expression an OrElse operation instead of an AndAlso operation.
                    return Expression.MakeBinary(ExpressionType.OrElse, left, right, b.IsLiftedToNull, b.Method);
                }

                return base.VisitBinary(b);
            }
        }
    }
}
