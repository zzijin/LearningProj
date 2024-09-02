using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadSynchronization_Test
{
    internal class SemaphoreTest
    {
        public static void Test()
        {
            Semaphore pool = new Semaphore(0, 3);
            var padding = 0;

            for (int i = 1; i <= 5; i++)
            {
                var num = i;
                Task.Run(() =>
                {
                    Console.WriteLine($"线程 {num} 等待信号量");
                    pool.WaitOne();

                    var rs = Interlocked.Add(ref padding, 100);

                    Console.WriteLine($"线程 {num} 获得信号量");

                    Thread.Sleep(1000 + rs);

                    Console.WriteLine($"线程 {num} 释放信号量");
                    Console.WriteLine($"线程 {num} 释放前的信号量计数为 {pool.Release()}");
                });
            }

            Thread.Sleep(500);
            Console.WriteLine("主线程释放3个信号量");
            pool.Release(3);
        }
    }
}
