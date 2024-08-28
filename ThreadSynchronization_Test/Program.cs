using System.Xml.Linq;

namespace ThreadSynchronization_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Example.Test();

            bool createdNew;
            //createdNew=true
            EventWaitHandle eventWait =new EventWaitHandle(false,EventResetMode.AutoReset, "A",out createdNew);
            //createdNew=false
            EventWaitHandle eventWait1 = new EventWaitHandle(false, EventResetMode.ManualReset, "A", out createdNew);
            Task.Run(() =>
            {
                Console.WriteLine("线程启动！");
                eventWait.WaitOne();
                Console.WriteLine("线程阻塞解除！#1");

                Console.WriteLine("线程阻塞！");
                eventWait.WaitOne();
                Console.WriteLine("线程阻塞解除！#2");

                Console.WriteLine("线程执行完成");
            });
            Thread.Sleep(250);
            Console.WriteLine("回车以释放");
            Console.ReadLine();
            eventWait.Set();
            Console.WriteLine("回车以释放");
            Console.ReadLine();
            eventWait.Set();
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
        class Example
        {
            private static AutoResetEvent event_1 = new AutoResetEvent(true);
            private static AutoResetEvent event_2 = new AutoResetEvent(false);

            public static void Test()
            {
                Console.WriteLine("Press Enter to create three threads and start them.\r\n" +
                                  "The threads wait on AutoResetEvent #1, which was created\r\n" +
                                  "in the signaled state, so the first thread is released.\r\n" +
                                  "This puts AutoResetEvent #1 into the unsignaled state.");
                Console.ReadLine();

                for (int i = 1; i < 4; i++)
                {
                    Thread t = new Thread(ThreadProc);
                    t.Name = "Thread_" + i;
                    t.Start();
                }
                Thread.Sleep(250);

                for (int i = 0; i < 2; i++)
                {
                    Console.WriteLine("Press Enter to release another thread.");
                    Console.ReadLine();
                    event_1.Set();
                    Thread.Sleep(250);
                }

                Console.WriteLine("\r\nAll threads are now waiting on AutoResetEvent #2.");
                for (int i = 0; i < 3; i++)
                {
                    Console.WriteLine("Press Enter to release a thread.");
                    Console.ReadLine();
                    event_2.Set();
                    Thread.Sleep(250);
                }

                // Visual Studio: Uncomment the following line.
                //Console.Readline();
            }

            static void ThreadProc()
            {
                string name = Thread.CurrentThread.Name;

                Console.WriteLine("{0} waits on AutoResetEvent #1.", name);
                event_1.WaitOne();
                Console.WriteLine("{0} is released from AutoResetEvent #1.", name);

                Console.WriteLine("{0} waits on AutoResetEvent #2.", name);
                event_2.WaitOne();
                Console.WriteLine("{0} is released from AutoResetEvent #2.", name);

                Console.WriteLine("{0} ends.", name);
            }
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
