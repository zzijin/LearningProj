﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HostBuilder_Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await CreateHostApplicationBuilderAsync(args);
            await CreateHostBuilderAsync(args);

            Console.ReadLine();
        }

        static async Task CreateHostApplicationBuilderAsync(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddTransient<TransientDisposable>();
            builder.Services.AddScoped<ScopedDisposable>();
            builder.Services.AddSingleton<SingletonDisposable>();
            builder.Services.AddHostedService<ExampleHostedService>();

            var services = builder.Services.ToArray();
            var configs=builder.Configuration.Sources.ToArray();
            var loggings=builder.Logging.Services.ToArray();
            using IHost host = builder.Build();

            await host.RunAsync();
        }

        static async Task CreateHostBuilderAsync(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            builder.ConfigureServices((context, services) => services.AddTransient<TransientDisposable>());
            builder.ConfigureServices((context, services) => services.AddScoped<ScopedDisposable>());
            builder.ConfigureServices((context, services) => services.AddSingleton<SingletonDisposable>());
            builder.ConfigureServices((context, services) => services.AddHostedService<ExampleHostedService>());

            //var services = builder..ToArray();
            //var configs = builder.Configuration.Sources.ToArray();
            //var loggings = builder.Logging.Services.ToArray();
            using IHost host = builder.Build();

            await host.RunAsync();
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

        public sealed class ExampleHostedService : IHostedService, IHostedLifecycleService
        {
            private readonly ILogger _logger;

            public ExampleHostedService(
                ILogger<ExampleHostedService> logger,
                IHostApplicationLifetime appLifetime, TransientDisposable tt)
            {
                _logger = logger;

                appLifetime.ApplicationStarted.Register(OnStarted);
                appLifetime.ApplicationStopping.Register(OnStopping);
                appLifetime.ApplicationStopped.Register(OnStopped);
            }

            Task IHostedLifecycleService.StartingAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("1. StartingAsync has been called.");

                return Task.CompletedTask;
            }

            Task IHostedService.StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("2. StartAsync has been called.");

                return Task.CompletedTask;
            }

            Task IHostedLifecycleService.StartedAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("3. StartedAsync has been called.");

                return Task.CompletedTask;
            }

            private void OnStarted()
            {
                _logger.LogInformation("4. OnStarted has been called.");
            }

            private void OnStopping()
            {
                _logger.LogInformation("5. OnStopping has been called.");
            }

            Task IHostedLifecycleService.StoppingAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("6. StoppingAsync has been called.");

                return Task.CompletedTask;
            }

            Task IHostedService.StopAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("7. StopAsync has been called.");

                return Task.CompletedTask;
            }

            Task IHostedLifecycleService.StoppedAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("8. StoppedAsync has been called.");

                return Task.CompletedTask;
            }

            private void OnStopped()
            {
                _logger.LogInformation("9. OnStopped has been called.");
            }
        }
    }
}
