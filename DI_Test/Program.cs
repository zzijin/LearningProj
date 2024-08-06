using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DI_Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            builder.ConfigureServices((context, services) =>services.AddTransient<TransientDisposable>());
            builder.ConfigureServices((context, services) => services.AddScoped<ScopedDisposable>());
            builder.ConfigureServices((context, services) => services.AddSingleton<SingletonDisposable>());

            using IHost host = builder.Build();

            ExemplifyDisposableScoping(host.Services, "Scope 1");
            Console.WriteLine();

            ExemplifyDisposableScoping(host.Services, "Scope 2");
            Console.WriteLine();

            await host.RunAsync();

            Console.ReadLine();
        }

        static void ExemplifyDisposableScoping(IServiceProvider services, string scope)
        {
            Console.WriteLine($"{scope}...");

            using IServiceScope serviceScope = services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            _ = provider.GetRequiredService<TransientDisposable>();
            _ = provider.GetRequiredService<ScopedDisposable>();
            _ = provider.GetRequiredService<SingletonDisposable>();
        }
    }

    public sealed class TransientDisposable : IDisposable
    {
        public void Dispose() => Console.WriteLine($"{nameof(TransientDisposable)}.Dispose()");
    }
    public sealed class ScopedDisposable : IDisposable
    {
        public void Dispose() => Console.WriteLine($"{nameof(ScopedDisposable)}.Dispose()");
    }
    public sealed class SingletonDisposable : IDisposable
    {
        public void Dispose() => Console.WriteLine($"{nameof(SingletonDisposable)}.Dispose()");
    }
}
