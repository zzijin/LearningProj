using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace MemoryCache_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //MemoryCache是IMemoryCache的默认实现
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions()
            {
                //设置缓存的最大大小
                SizeLimit = 1024L * 1024 * 1024 * 1024,
                //设置在超出最大大小时要压缩的缓存量
                CompactionPercentage = 0.1,
                //设置连续扫描过期项之间的最小时间长度
                ExpirationScanFrequency = TimeSpan.FromMinutes(10)
            });

            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddSingleton<IMemoryCache>((provider) =>
            {
                return new MemoryCache(new MemoryCacheOptions()
                {
                    //初始化设置
                });
            });
            //或使用拓展方法
            builder.Services.AddMemoryCache(options =>
            {
                //初始化设置
            });

            memoryCache.GetOrCreate<byte[]>("CacheKey", entry =>
                {
                    byte[] data = new byte[1024];

                    //可在此处设置缓存项参数
                    //注册缓存项释放回调
                    entry.RegisterPostEvictionCallback((key,value,reason,state)=>
                    {
                    });
                    //设置缓存项滑动过期时间，缓存项每次被访问都会根据此项来延长生存时间，但不会将项生存期延长到超过绝对到期时间
                    entry.SlidingExpiration = TimeSpan.FromMinutes(60);
                    //设置相对于当前时间的绝对到期时间
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(120);
                    //设置缓存项值的大小
                    entry.Size = data.Length;
                    //设置缓存项的优先级,当触发内存压缩时，高优先级的未过期的缓存项更有可能被留存
                    entry.Priority = CacheItemPriority.Normal;

                    return data;
                }
            );
        }
    }
}
