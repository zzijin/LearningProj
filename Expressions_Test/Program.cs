using System.Diagnostics;

namespace Expressions_Test
{
    internal class Program
    {
        static async void Main(string[] args)
        {
            Expressions_Create.CreateExpressions_1();
            await Task.Delay(5000);
            Console.ReadLine();
        }
    }

}
