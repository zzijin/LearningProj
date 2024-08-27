namespace ThreadSynchronization_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int x;
            //以下是原子性操作，因为仅包含基础的读写操作
            x = 0;
            var y = x;
            //不是原子操作，因为语句由多次读写操作组合而成
            x++;
            x--;


            //int x = 0;
            //Task.Run(() =>
            //{
            //    for (int i = 0; i < 10000; i++)
            //    {

            //    }
            //});
            //Task.Run(() =>
            //{
            //    for (int i = 0; i < 10000; i++)
            //    {

            //    }
            //});

            Console.ReadLine();
        }

        static void VolatileTest()
        {
            ////示例1
            int x = 0;
            //以下循环可能被优化掉，导致与原本的意图不一致
            for (int i = 0; i < x; i++)
            {
                Console.WriteLine(i);
            }
        }
        static void VolatileTest1()
        {
            ////示例2
            var x = 0;
            var y = 0;
            //以下两个异步操作中代码的执行顺序可能被优化，导致与原本的意图不一致
            Task.Run(() =>
            {
                x = 1;
                var z = y;
            });
            Task.Run(() =>
            {
                y = 1;
                var z = y;
            });
        }


        static void VolatileTest2()
        {
            ////示例1
            int x = 0;
            //以下循环不会被优化,始终执行初始化表达式、条件判断,即使无法进入循环体内
            for (int i = 0; i < Volatile.Read(ref x); i++)
            {
                Console.WriteLine(i);
            }
        }
        static void VolatileTest3()
        {
            ////示例2
            var x = 0;
            var y = 0;
            //以下两个异步操作中代码的执行顺序不会被优化,语句一始终在语句二前执行
            Task.Run(() =>
            {
                Volatile.Write(ref x, 1);
                var z = y;
            });
            Task.Run(() =>
            {
                Volatile.Write(ref y, 2);
                var z = y;
            });
        }
    }
}
