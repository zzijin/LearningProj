using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadSynchronization_Test
{
    internal class BarrierTest
    {
        public static void BarrierSample1()
        {
            int count = 0;

            //创建Barrier示例，并且在所有参与者达到阶段后执行委托进行输出
            Barrier barrier = new Barrier(3, (b) =>
            {
                Console.WriteLine("Post-Phase action: count={0}, phase={1}", count, b.CurrentPhaseNumber);
                if (b.CurrentPhaseNumber == 2) throw new Exception("D'oh!");
            });

            //添加参与者
            barrier.AddParticipants(2);

            //减少参与者
            barrier.RemoveParticipant();

            // This is the logic run by all participants
            Action action = () =>
            {
                Interlocked.Increment(ref count);
                barrier.SignalAndWait(); // during the post-phase action, count should be 4 and phase should be 0
                Interlocked.Increment(ref count);
                barrier.SignalAndWait(); // during the post-phase action, count should be 8 and phase should be 1

                // The third time, SignalAndWait() will throw an exception and all participants will see it
                Interlocked.Increment(ref count);
                try
                {
                    barrier.SignalAndWait();
                }
                catch (BarrierPostPhaseException bppe)
                {
                    Console.WriteLine("Caught BarrierPostPhaseException: {0}", bppe.Message);
                }

                // The fourth time should be hunky-dory
                Interlocked.Increment(ref count);
                barrier.SignalAndWait(); // during the post-phase action, count should be 16 and phase should be 3
            };

            // Now launch 4 parallel actions to serve as 4 participants
            Parallel.Invoke(action, action, action, action);

            // This (5 participants) would cause an exception:
            // Parallel.Invoke(action, action, action, action, action);
            //      "System.InvalidOperationException: The number of threads using the barrier
            //      exceeded the total number of registered participants."

            // It's good form to Dispose() a barrier when you're done with it.
            barrier.Dispose();
        }

        static string[] words1 = new string[] { "brown", "jumps", "the", "fox", "quick" };
        static string[] words2 = new string[] { "dog", "lazy", "the", "over" };
        static string solution = "the quick brown fox jumps over the lazy dog.";
        static bool success = false;
        static Barrier barrier = new Barrier(2, (b) =>
        {
            //将words1与words2组合，判断是否与solution是否一致
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < words1.Length; i++)
            {
                sb.Append(words1[i]);
                sb.Append(" ");
            }
            for (int i = 0; i < words2.Length; i++)
            {
                sb.Append(words2[i]);

                if (i < words2.Length - 1)
                    sb.Append(" ");
            }
            sb.Append(".");

            Console.CursorLeft = 0;
            Console.Write("Current phase: {0}", barrier.CurrentPhaseNumber);
            if (String.CompareOrdinal(solution, sb.ToString()) == 0)
            {
                success = true;
                Console.WriteLine("\r\nThe solution was found in {0} attempts", barrier.CurrentPhaseNumber);
            }
        });

        public static void BarrierSample2()
        {
            Thread t1 = new Thread(() => Solve(words1));
            Thread t2 = new Thread(() => Solve(words2));
            t1.Start();
            t2.Start();

            Console.ReadLine();
        }

        static void Solve(string[] wordArray)
        {
            while (success == false)
            {
                //使用洗牌算法对单词组进行随机排序
                Random random = new Random();
                for (int i = wordArray.Length - 1; i > 0; i--)
                {
                    int swapIndex = random.Next(i + 1);
                    string temp = wordArray[i];
                    wordArray[i] = wordArray[swapIndex];
                    wordArray[swapIndex] = temp;
                }

                barrier.SignalAndWait();
            }
        }
    }
}
