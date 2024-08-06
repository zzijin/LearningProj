namespace TaskScheduler_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var ts = new LimitedConcurrencyLevelTaskScheduler(5);
            //使用TaskFactory并指定其TaskScheduler
            TaskFactory tf = new TaskFactory(ts);
            tf.StartNew(() =>
            {
                //执行代码
            });
            String.Intern("A");
            //创建Task并在Start时指定其TaskScheduler
            Task t = new Task(() =>
            {
                //执行代码
            });
            t.Start(ts);

            Console.WriteLine("Hello, World!");
        }
    }
}
