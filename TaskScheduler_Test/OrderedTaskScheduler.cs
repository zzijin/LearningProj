using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskScheduler_Test
{
    //有序执行的任务调度器
    internal class OrderedTaskScheduler : TaskScheduler
    {
        //标识当前是否正在处理任务
        private bool _isProcessingItems = false;
        //此调度器允许的最大并发级别。
        private const int _maxDegreeOfParallelism = 1;
        //要执行的任务列表
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();

        protected override void QueueTask(Task task)
        {
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (!_isProcessingItems)
                {
                    ExecuteTasks();
                }
            }
        }

        private void ExecuteTasks()
        {
            _isProcessingItems=true;
            Thread t = new Thread(() =>
            {
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
                finally { _isProcessingItems = false; }
            });
            t.IsBackground = true;
            t.Start();
        }

        //不允许任务内联执行
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        // 最大并发数量
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        protected override IEnumerable<Task>? GetScheduledTasks()
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
