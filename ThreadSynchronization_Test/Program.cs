using System.Xml.Linq;

namespace ThreadSynchronization_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var t1 = typeof(string);
            var t2 = typeof(string);
            var rs = object.ReferenceEquals(t1, t2);
            //SemaphoreTest.Test();
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

        struct SimpleSpinLock
        {
            //设定0代表未使用，1代表使用
            private int m_ReourceInUse;

            //获取锁
            public void Enter()
            {
                SpinWait spinWait = new SpinWait();
                //始终循环，使得线程空转
                while (true)
                {
                    //仅当m_ReourceInUse从未使用变为使用时进入
                    if (Interlocked.Exchange(ref m_ReourceInUse, 1) == 0)
                        return;
                }
            }

            //释放锁
            public void Leave()
            {
                Volatile.Write(ref m_ReourceInUse, 0);
            }
        }
    }
}
