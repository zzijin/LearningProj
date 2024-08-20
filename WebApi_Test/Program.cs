
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static System.Net.Mime.MediaTypeNames;

namespace WebApi_Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMemoryCache();
            builder.Services.Configure<MvcOptions>(opt =>
            {
                opt.Filters.Add<AsyncExceptionFilter>();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseMiddleware<Middleware_Test>();
            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync("Hello from 1st delegate.");
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });

            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync("Hello from 2nd delegate.");
                await next.Invoke();
            });



            app.Run();
        }
    }

    public class Middleware_Test
    {
        private readonly RequestDelegate _next;

        public Middleware_Test(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var conn = httpContext.Connection;//获取有关此请求的基础连接的信息。 
            var features = httpContext.Features;//获取此请求上可用的服务器和中间件提供的 HTTP 功能的集合。
            var items = httpContext.Items;//获取或设置可用于在此请求范围内共享数据的键/值集合。
            var request = httpContext.Request;//获取此请求的 HttpRequest 对象。
            var requestAborted = httpContext.RequestAborted;//在中止此请求的连接时通知，因此应取消请求操作。
            var requestServices= httpContext.RequestServices;//获取或设置提供对请求服务容器的访问权限的 IServiceProvider。
            var response = httpContext.Response;//获取此请求的 HttpResponse 对象。
            var session= httpContext.Session;//获取或设置用于管理此请求的用户会话数据的对象。
            var tession = httpContext.TraceIdentifier;//获取或设置用于在跟踪日志中表示此请求的唯一标识符。
            var user= httpContext.User;//获取或设置此请求的用户。
            var websocket = httpContext.WebSockets;//获取一个对象，该对象管理此请求的 WebSocket 连接的建立。
            //return Task.CompletedTask;
            await _next(httpContext);
        }
    }

    public class AsyncExceptionFilter : IAsyncExceptionFilter
    {
        IWebHostEnvironment _environment;

        public AsyncExceptionFilter(IWebHostEnvironment env)
        {
            _environment = env;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            //异常信息
            //context.ActionDescriptor
            //指示异常是否被处理，若已被处理，则不再向其他IAsyncExceptionFilter传递
            //context.ExceptionHandled
            //Action相应信息
            //context.Result

            string msg;
            if(_environment.IsDevelopment())
            {
                msg= context.Exception.Message;
            }
            else
            {
                msg = "服务器发生异常";
            }
            ObjectResult result = new ObjectResult(new { code = 500, message = msg });
            context.Result = result;
            context.ExceptionHandled = true;

            return Task.CompletedTask;
        }
    }
}
