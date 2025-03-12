using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace Channel_Test
{
    internal class Program
    {
        static async void Main(string[] args)
        {
            //创建有界通道，容量有限的通道，容量已满时会根据BoundedChannelOptions.FullMode触发不同的边界行为
            var boundedChannel = Channel.CreateBounded<int>(
                //Channel创建选项
                new BoundedChannelOptions(10) //指定最大容量
                {
                    //当通道操作完成时，是否允许同步执行等待的延续任务
                    AllowSynchronousContinuations = true,
                    //容量已满时,触发的边界行为
                    FullMode = BoundedChannelFullMode.DropOldest,
                },
                (item) =>
                {
                    //项被删除时触发的回调
                });

            //尝试立即将数据写入通道，若通道已满或已关闭，则返回false。
            boundedChannel.Writer.TryWrite(123);
            //获取ValueTask<bool>，表示当前通道是否可写。
            if (await boundedChannel.Writer.WaitToWriteAsync())
            {
                //异步写入数据到通道，如果通道已满（有界通道），会异步等待直到有空间。如果通道已关闭，抛出ChannelClosedException。
                await boundedChannel.Writer.WriteAsync(123);
            }

            //返回IAsyncEnumerable<T>异步枚举项，适用于批量处理数据时。
            await foreach (var item in boundedChannel.Reader.ReadAllAsync())
            {
                //获取数据
            }
            //异步读取单个数据项。若通道为空但未完成，会挂起直到数据到达或通道关闭。如果通道已关闭且无数据，抛出ChannelClosedException。
            await boundedChannel.Reader.ReadAsync();
            //尝试查看通道中的下一个数据项，但不移除它。
            int rs;
            boundedChannel.Reader.TryPeek(out rs);
            //尝试立即从通道中读取一个数据项并移除它。
            boundedChannel.Reader.TryRead(out rs);
            //获取ValueTask<bool>，表示当前通道是否可读。
            if (await boundedChannel.Reader.WaitToReadAsync())
            {
                //异步读取单个数据项。若通道为空但未完成，会挂起直到数据到达或通道关闭。如果通道已关闭且无数据，抛出ChannelClosedException。
                await boundedChannel.Reader.ReadAsync();
            }

            //标记通道写入端为“已完成”状态，表示生产者不再写入新数据。调用后，所有后续的写入操作会抛出ChannelClosedException。若当前通道已完成，再次调用会抛出ChannelClosedException。
            boundedChannel.Writer.Complete();
            //尝试安全地标记通道写入端为“已完成”状态。若通道已关闭或无法完成，返回false。
            boundedChannel.Writer.TryComplete();

            //创建无界通道,即无限容量的通道，使用时需要评估内存泄露的风险
            var unboundedChannel = Channel.CreateUnbounded<int>(
                 //Channel创建选项
                 new UnboundedChannelOptions() //指定最大容量
                 {
                     //当通道操作完成时，是否允许同步执行等待的延续任务
                     AllowSynchronousContinuations = true,
                     //是否为单一生产者
                     SingleWriter = false,
                     //是否为单一消费者,是则使用SingleConsumerUnboundedChannel的实现
                     SingleReader = true,
                 });

            Console.WriteLine("Hello, World!");
        }

        //自定义的一个有界通道
        class MyChannel<T> : Channel<T>
        {
            //重写生产者
            private sealed class MyChannelReader : ChannelReader<T>
            {
                internal readonly MyChannel<T> _parent;
                public override Task Completion => _parent._completion.Task;
                public override bool CanCount => true;
                public override bool CanPeek => true;

                internal MyChannelReader(MyChannel<T> parent)
                {
                    _parent = parent;
                }

                //必须重写的两个方法,ChannelReader<T>是抽象类实现,其所有方法都支持重写
                public override bool TryRead([MaybeNullWhen(false)] out T item)
                {
                    throw new NotImplementedException();
                }

                public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
                {
                    throw new NotImplementedException();
                }
            }

            //重写消费者
            private sealed class MyChannelWriter : ChannelWriter<T>
            {
                internal readonly MyChannel<T> _parent;

                internal MyChannelWriter(MyChannel<T> parent)
                {
                    _parent = parent;
                }

                //必须重写的两个方法,ChannelWriter<T>是抽象类实现,其所有方法都支持重写
                public override bool TryWrite(T item)
                {
                    throw new NotImplementedException();
                }

                public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default)
                {
                    throw new NotImplementedException();
                }
            }

            //自定义的缓冲区，可以替换为自定义的数据结构，并在自定义的ChannelReader、ChannelWriter中操作缓冲区
            private readonly T[] _items;
            private readonly int _bufferedCapacity;
            private readonly BoundedChannelFullMode _mode;
            private readonly bool _runContinuationsAsynchronously;
            private readonly TaskCompletionSource _completion;

            internal MyChannel(int bufferedCapacity, BoundedChannelFullMode mode, bool runContinuationsAsynchronously)
            {
                _bufferedCapacity = bufferedCapacity;
                _mode = mode;
                _runContinuationsAsynchronously = runContinuationsAsynchronously;

                _items = new T[bufferedCapacity];
                _completion = new TaskCompletionSource(runContinuationsAsynchronously ? TaskCreationOptions.RunContinuationsAsynchronously : TaskCreationOptions.None);
                base.Reader = new MyChannelReader(this);
                base.Writer = new MyChannelWriter(this);
            }
        }
    }
}
