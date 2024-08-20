
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
            var conn = httpContext.Connection;//��ȡ�йش�����Ļ������ӵ���Ϣ�� 
            var features = httpContext.Features;//��ȡ�������Ͽ��õķ��������м���ṩ�� HTTP ���ܵļ��ϡ�
            var items = httpContext.Items;//��ȡ�����ÿ������ڴ�����Χ�ڹ������ݵļ�/ֵ���ϡ�
            var request = httpContext.Request;//��ȡ������� HttpRequest ����
            var requestAborted = httpContext.RequestAborted;//����ֹ�����������ʱ֪ͨ�����Ӧȡ�����������
            var requestServices= httpContext.RequestServices;//��ȡ�������ṩ��������������ķ���Ȩ�޵� IServiceProvider��
            var response = httpContext.Response;//��ȡ������� HttpResponse ����
            var session= httpContext.Session;//��ȡ���������ڹ����������û��Ự���ݵĶ���
            var tession = httpContext.TraceIdentifier;//��ȡ�����������ڸ�����־�б�ʾ�������Ψһ��ʶ����
            var user= httpContext.User;//��ȡ�����ô�������û���
            var websocket = httpContext.WebSockets;//��ȡһ�����󣬸ö������������ WebSocket ���ӵĽ�����
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
            //�쳣��Ϣ
            //context.ActionDescriptor
            //ָʾ�쳣�Ƿ񱻴������ѱ���������������IAsyncExceptionFilter����
            //context.ExceptionHandled
            //Action��Ӧ��Ϣ
            //context.Result

            string msg;
            if(_environment.IsDevelopment())
            {
                msg= context.Exception.Message;
            }
            else
            {
                msg = "�����������쳣";
            }
            ObjectResult result = new ObjectResult(new { code = 500, message = msg });
            context.Result = result;
            context.ExceptionHandled = true;

            return Task.CompletedTask;
        }
    }
}
