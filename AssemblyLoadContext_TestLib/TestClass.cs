using AssemblyLoadContext_TestReferenceLib;

namespace AssemblyLoadContext_TestLib
{
    public class TestClass
    {
        public static string GetVersion()
        {
            //版本号分别为1~3
            return "3";
        }

        private int _num;
        public void SetNum(int num)
        {
            _num = num;
        }

        public int GetNum()
        {
            return _num;
        }

        public void ConsoleNum()
        {
            Console.WriteLine($"当前数字为{_num}");
        }

        public void TestReference()
        {
            //程序集依赖于另一个程序集
            TestReferenceClass.ConsoleReferenceClass();
        }
    }
}
