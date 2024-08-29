using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadSynchronization_Test
{
    class EventWaitHandleTest
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

        public static void Test2()
        {
            bool createdNew;
            //createdNew=true
            EventWaitHandle eventWait = new EventWaitHandle(false, EventResetMode.AutoReset, "A", out createdNew);
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
}
