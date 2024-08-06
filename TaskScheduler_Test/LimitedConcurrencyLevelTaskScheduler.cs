using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskScheduler_Test
{
    //提供一个任务调度器，限制最大并发数量
    //在线程池上运行。
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        //指示当前线程是否正在处理工作项。
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        //要执行的任务列表
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();

        //此调度器允许的最大并发级别。
        private readonly int _maxDegreeOfParallelism;

        // 指示调度程序当前是否正在处理工作项。
        private int _delegatesQueuedOrRunning = 0;

        // 创建具有指定最大并发数量的新实例。
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        // 将任务排队到调度程序。
        protected sealed override void QueueTask(Task task)
        {
            //将任务排进队列
            //若当前并发数小于指定数，则通知线程池执行任务
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // 通知线程池执行任务
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                //标识当前线程正在执行任务
                _currentThreadIsProcessingItems = true;
                try
                {
                    // 处理任务直到全部完成
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            //无任务时退出执行
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // 获取一个任务
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // 执行任务
                        base.TryExecuteTask(item);
                    }
                }
                //标识当前线程处理完成
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        // 内联执行任务
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // 若该线程没有处理任务，则不允许内联
            if (!_currentThreadIsProcessingItems) return false;

            // 内联执行任务
            if (taskWasPreviouslyQueued)
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        // 最大并发数量
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        // 获取当前在此调度器上调度的任务的枚举。
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }
}
